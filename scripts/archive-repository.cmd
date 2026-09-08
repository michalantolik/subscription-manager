@echo off
setlocal

set "SCRIPT=%~dp0archive-repository.ps1"

if not exist "%SCRIPT%" (
    echo [ERROR] PowerShell archive script not found:
    echo         %SCRIPT%
    exit /b 1
)

where pwsh >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    pwsh -NoProfile -ExecutionPolicy Bypass -File "%SCRIPT%" %*
    exit /b %ERRORLEVEL%
)

where powershell.exe >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%SCRIPT%" %*
    exit /b %ERRORLEVEL%
)

echo [ERROR] PowerShell is not available on PATH.
exit /b 1
