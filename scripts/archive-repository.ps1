# Creates a private full-history repository archive.
# Remote synchronization is intentionally conservative: fetch is safe, pull is
# fast-forward-only and only runs for a clean working tree. The script never
# stashes, pushes, merges or rebases local work automatically.

$ErrorActionPreference = "Stop"

$script:CheckMark = [char]0x2713
$script:CrossMark = [char]0x2717
$script:StepCount = 7
$script:StagingRoot = $null

function Write-Step {
    param(
        [int]$Number,
        [string]$Message
    )

    $percent = [int](($Number - 1) / $script:StepCount * 100)

    Write-Progress `
        -Id 0 `
        -Activity "Archive Repository" `
        -Status "[$Number/$script:StepCount] $Message" `
        -PercentComplete $percent

    Write-Host ""
    Write-Host "[$Number/$script:StepCount] $Message" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Message)

    Write-Host "      $script:CheckMark $Message" -ForegroundColor Green
}

function Write-WarningMessage {
    param([string]$Message)

    Write-Host "      ! $Message" -ForegroundColor Yellow
}

function Test-IsArtifactPath {
    param(
        [string]$RelativePath,
        [string[]]$ArtifactDirectoryNames
    )

    $segments = $RelativePath -split '[\\/]'

    foreach ($segment in $segments) {
        if ($ArtifactDirectoryNames -contains $segment) {
            return $true
        }
    }

    return $false
}

try {
    Clear-Host
    Write-Host "Archive Repository" -ForegroundColor White
    Write-Host "==================" -ForegroundColor DarkGray

    Write-Step 1 "Validating repository"

    $gitCommand = Get-Command git -ErrorAction SilentlyContinue

    if ($null -eq $gitCommand) {
        throw "Git is not available on PATH."
    }

    $repositoryRoot = (
        git -C $PSScriptRoot rev-parse --show-toplevel 2>$null
    ).Trim()

    if ([string]::IsNullOrWhiteSpace($repositoryRoot)) {
        throw "The script is not located inside a Git repository."
    }

    $repositoryRoot = [System.IO.Path]::GetFullPath($repositoryRoot)
    $repositoryName = Split-Path $repositoryRoot -Leaf
    $parentDirectory = Split-Path $repositoryRoot -Parent
    $archivePath = Join-Path $parentDirectory "$repositoryName.zip"

    $gitDirectory = (
        git -C $repositoryRoot rev-parse --absolute-git-dir 2>$null
    ).Trim()

    if ([string]::IsNullOrWhiteSpace($gitDirectory)) {
        throw "Git metadata could not be located."
    }

    $gitDirectory = [System.IO.Path]::GetFullPath($gitDirectory)
    $expectedGitDirectory = Join-Path $repositoryRoot ".git"

    if (-not [string]::Equals(
            $gitDirectory.TrimEnd('\', '/'),
            $expectedGitDirectory.TrimEnd('\', '/'),
            [System.StringComparison]::OrdinalIgnoreCase)) {
        throw "The Git directory is outside the repository root. Full-history archiving is not supported for this worktree layout."
    }

    Write-Success "Git repository detected"
    Write-Success "Repository root: $repositoryRoot"

    Write-Step 2 "Synchronizing repository"

    $gitStatus = @(git -C $repositoryRoot status --short)
    $workingTreeIsClean = $gitStatus.Count -eq 0
    $branchName = (git -C $repositoryRoot branch --show-current).Trim()
    $upstream = (git -C $repositoryRoot rev-parse --abbrev-ref --symbolic-full-name '@{upstream}' 2>$null)

    if ($null -ne $upstream) {
        $upstream = $upstream.Trim()
    }

    if ([string]::IsNullOrWhiteSpace($branchName)) {
        Write-WarningMessage "HEAD is detached; automatic pull is skipped"
    }
    elseif ([string]::IsNullOrWhiteSpace($upstream)) {
        Write-WarningMessage "Branch $branchName has no upstream; automatic pull is skipped"
    }
    else {
        $remoteName = $upstream.Split('/')[0]

        Write-Success "Branch: $branchName"
        Write-Success "Upstream: $upstream"

        & git -C $repositoryRoot fetch --prune $remoteName 2>&1 | Out-Null

        if ($LASTEXITCODE -ne 0) {
            Write-WarningMessage "Could not fetch $remoteName; archiving the current local state"
        }
        else {
            Write-Success "Fetched latest state from $remoteName"

            $counts = (git -C $repositoryRoot rev-list --left-right --count "HEAD...$upstream").Trim() -split '\s+'

            if ($LASTEXITCODE -ne 0 -or $counts.Count -ne 2) {
                Write-WarningMessage "Could not compare the local branch with $upstream; automatic pull is skipped"
            }
            else {
                $aheadCount = [int]$counts[0]
                $behindCount = [int]$counts[1]

                if ($aheadCount -eq 0 -and $behindCount -eq 0) {
                    Write-Success "Already up to date"
                }
                elseif ($aheadCount -eq 0 -and $behindCount -gt 0) {
                    if ($workingTreeIsClean) {
                        & git -C $repositoryRoot pull --ff-only 2>&1 | Out-Null

                        if ($LASTEXITCODE -ne 0) {
                            throw "Fast-forward pull failed. The archive was not created."
                        }

                        Write-Success "Fast-forwarded $branchName by $behindCount commit(s)"
                        $gitStatus = @(git -C $repositoryRoot status --short)
                        $workingTreeIsClean = $gitStatus.Count -eq 0
                    }
                    else {
                        Write-WarningMessage "Local branch is $behindCount commit(s) behind $upstream"
                        Write-WarningMessage "Pull skipped because the working tree has local changes"
                    }
                }
                elseif ($aheadCount -gt 0 -and $behindCount -eq 0) {
                    Write-WarningMessage "Local branch is $aheadCount commit(s) ahead of $upstream; nothing was pushed"
                }
                else {
                    Write-WarningMessage "Local and remote branches have diverged ($aheadCount ahead, $behindCount behind)"
                    Write-WarningMessage "Automatic merge or rebase is intentionally skipped"
                }
            }
        }
    }

    Write-Step 3 "Inspecting repository"

    Write-Success "Repository name: $repositoryName"
    Write-Success "Archive path: $archivePath"
    Write-Success "Git metadata will be preserved"

    $gitStatus = @(git -C $repositoryRoot status --short)

    if ($gitStatus.Count -eq 0) {
        Write-Success "Working tree is clean"
    }
    else {
        Write-WarningMessage "Working tree has $($gitStatus.Count) changed or untracked item(s); they will be included"
    }

    Write-Step 4 "Preparing clean staging copy"

    $artifactDirectoryNames = @(
        "bin",
        "obj",
        "TestResults",
        "artifacts",
        ".artifacts"
    )

    $allFiles = @(
        Get-ChildItem `
            -LiteralPath $repositoryRoot `
            -File `
            -Recurse `
            -Force
    )

    $filesToCopy = [System.Collections.Generic.List[System.IO.FileInfo]]::new()
    $excludedFiles = 0

    foreach ($file in $allFiles) {
        $relativePath = $file.FullName.Substring($repositoryRoot.Length).TrimStart('\', '/')

        if (Test-IsArtifactPath $relativePath $artifactDirectoryNames) {
            $excludedFiles++
            continue
        }

        $filesToCopy.Add($file)
    }

    $script:StagingRoot = Join-Path `
        ([System.IO.Path]::GetTempPath()) `
        ("archive-repository-{0}-{1}" -f $repositoryName, [Guid]::NewGuid().ToString("N"))

    $stagedRepository = Join-Path $script:StagingRoot $repositoryName
    New-Item -ItemType Directory -Path $stagedRepository -Force | Out-Null

    $copyCount = $filesToCopy.Count

    for ($index = 0; $index -lt $copyCount; $index++) {
        $file = $filesToCopy[$index]
        $relativePath = $file.FullName.Substring($repositoryRoot.Length).TrimStart('\', '/')
        $destination = Join-Path $stagedRepository $relativePath
        $destinationDirectory = Split-Path $destination -Parent

        if (-not (Test-Path -LiteralPath $destinationDirectory)) {
            New-Item -ItemType Directory -Path $destinationDirectory -Force | Out-Null
        }

        [System.IO.File]::Copy($file.FullName, $destination, $true)

        $copyPercent = if ($copyCount -eq 0) {
            100
        }
        else {
            [int](($index + 1) / $copyCount * 100)
        }

        Write-Progress `
            -Id 1 `
            -ParentId 0 `
            -Activity "Preparing staging copy" `
            -Status "$($index + 1) of $copyCount files" `
            -PercentComplete $copyPercent
    }

    Write-Progress -Id 1 -ParentId 0 -Activity "Preparing staging copy" -Completed

    Write-Success "Staged $copyCount file(s)"

    if ($excludedFiles -gt 0) {
        Write-Success "Excluded $excludedFiles build/test artifact file(s)"
    }
    else {
        Write-Success "No build/test artifacts found"
    }

    if (-not (Test-Path -LiteralPath (Join-Path $stagedRepository ".git"))) {
        throw "The staging copy does not contain .git metadata."
    }

    Write-Success "Full .git directory staged"

    Write-Step 5 "Preparing archive destination"

    if (Test-Path -LiteralPath $archivePath) {
        Remove-Item -LiteralPath $archivePath -Force
        Write-Success "Removed existing archive: $archivePath"
    }
    else {
        Write-Success "No existing archive to remove"
    }

    Write-Step 6 "Creating ZIP archive"

    Add-Type -AssemblyName System.IO.Compression
    Add-Type -AssemblyName System.IO.Compression.FileSystem

    $stagedFiles = @(
        Get-ChildItem `
            -LiteralPath $stagedRepository `
            -File `
            -Recurse `
            -Force
    )

    $archiveStream = [System.IO.File]::Open(
        $archivePath,
        [System.IO.FileMode]::CreateNew,
        [System.IO.FileAccess]::ReadWrite,
        [System.IO.FileShare]::None)

    try {
        $zipArchive = [System.IO.Compression.ZipArchive]::new(
            $archiveStream,
            [System.IO.Compression.ZipArchiveMode]::Create,
            $false)

        try {
            $zipCount = $stagedFiles.Count

            for ($index = 0; $index -lt $zipCount; $index++) {
                $file = $stagedFiles[$index]
                $relativePath = $file.FullName.Substring($stagedRepository.Length).TrimStart('\', '/')
                $entryName = "$repositoryName/$($relativePath.Replace('\', '/'))"

                [System.IO.Compression.ZipFileExtensions]::CreateEntryFromFile(
                    $zipArchive,
                    $file.FullName,
                    $entryName,
                    [System.IO.Compression.CompressionLevel]::Optimal) | Out-Null

                $zipPercent = if ($zipCount -eq 0) {
                    100
                }
                else {
                    [int](($index + 1) / $zipCount * 100)
                }

                Write-Progress `
                    -Id 1 `
                    -ParentId 0 `
                    -Activity "Creating ZIP archive" `
                    -Status "$($index + 1) of $zipCount files" `
                    -PercentComplete $zipPercent
            }
        }
        finally {
            if ($null -ne $zipArchive) {
                $zipArchive.Dispose()
            }
        }
    }
    finally {
        $archiveStream.Dispose()
    }

    Write-Progress -Id 1 -ParentId 0 -Activity "Creating ZIP archive" -Completed
    Write-Success "ZIP archive created"

    Write-Step 7 "Verifying archive"

    if (-not (Test-Path -LiteralPath $archivePath)) {
        throw "The archive was not created."
    }

    $archiveInfo = Get-Item -LiteralPath $archivePath

    if ($archiveInfo.Length -le 0) {
        throw "The archive is empty."
    }

    $verificationArchive = [System.IO.Compression.ZipFile]::OpenRead($archivePath)

    try {
        $entryNames = @($verificationArchive.Entries | ForEach-Object FullName)
        $gitHeadEntry = "$repositoryName/.git/HEAD"

        if ($entryNames -notcontains $gitHeadEntry) {
            throw "Archive verification failed: .git/HEAD is missing."
        }

        if ($entryNames.Count -ne $stagedFiles.Count) {
            throw "Archive verification failed: expected $($stagedFiles.Count) file(s), found $($entryNames.Count)."
        }
    }
    finally {
        $verificationArchive.Dispose()
    }

    $sizeMb = [Math]::Round($archiveInfo.Length / 1MB, 2)

    Write-Success "Archive contains complete Git metadata"
    Write-Success "Verified $($stagedFiles.Count) archived file(s)"
    Write-Success "Archive size: $sizeMb MB"

    Write-Progress -Id 0 -Activity "Archive Repository" -Completed

    Write-Host ""
    Write-Host "Completed successfully." -ForegroundColor Green
    Write-Host ""
    Write-Host "Archive: " -NoNewline
    Write-Host $archivePath -ForegroundColor White
    Write-Host ""
    Write-WarningMessage "The archive contains full Git history and may contain sensitive historical or local data. Keep it private."
}
catch {
    Write-Progress -Id 1 -Activity "Archive Repository" -Completed -ErrorAction SilentlyContinue
    Write-Progress -Id 0 -Activity "Archive Repository" -Completed -ErrorAction SilentlyContinue

    Write-Host ""
    Write-Host "$script:CrossMark Archive failed" -ForegroundColor Red
    Write-Host "  $($_.Exception.Message)" -ForegroundColor Red

    throw
}
finally {
    if ($null -ne $script:StagingRoot -and
        (Test-Path -LiteralPath $script:StagingRoot)) {
        Remove-Item -LiteralPath $script:StagingRoot -Recurse -Force -ErrorAction SilentlyContinue
    }
}
