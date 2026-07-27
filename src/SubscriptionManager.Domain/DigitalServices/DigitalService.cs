namespace SubscriptionManager.Domain.DigitalServices;

public sealed class DigitalService
{
    private DigitalService()
    {
    }

    private DigitalService(
        Guid id,
        string key,
        string name,
        bool isPredefined,
        Guid? ownerId,
        DigitalServiceCategory category,
        string? customCategoryName,
        string? iconKey,
        string? managementUrl,
        bool isActive,
        int sortOrder,
        DateTimeOffset createdAt)
    {
        if (id == Guid.Empty)
        {
            throw new ArgumentException(
                "Digital service ID cannot be empty.",
                nameof(id));
        }

        if (!Enum.IsDefined(category))
        {
            throw new ArgumentOutOfRangeException(
                nameof(category),
                "Digital service category is not supported.");
        }

        if (isPredefined && ownerId is not null)
        {
            throw new ArgumentException(
                "A predefined digital service cannot have an owner.",
                nameof(ownerId));
        }

        if (!isPredefined &&
            (ownerId is null || ownerId == Guid.Empty))
        {
            throw new ArgumentException(
                "A custom digital service must have an owner.",
                nameof(ownerId));
        }

        if (sortOrder < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sortOrder),
                "Digital service sort order cannot be negative.");
        }

        Id = id;
        IsPredefined = isPredefined;
        OwnerId = ownerId;
        Category = category;
        IsActive = isActive;
        SortOrder = sortOrder;
        CreatedAt = createdAt;

        SetKey(key);
        SetName(name);
        SetCustomCategoryName(customCategoryName);
        SetIconKey(iconKey);
        SetManagementUrl(managementUrl);
    }

    public Guid Id { get; }

    public string Key { get; private set; } = null!;

    public string Name { get; private set; } = null!;

    public bool IsPredefined { get; private set; }

    public Guid? OwnerId { get; private set; }

    public DigitalServiceCategory Category { get; private set; }

    public string? CustomCategoryName { get; private set; }

    public string? IconKey { get; private set; }

    public string? ManagementUrl { get; private set; }

    public bool IsActive { get; private set; }

    public int SortOrder { get; private set; }

    public DateTimeOffset CreatedAt { get; }

    public static DigitalService CreatePredefined(
        Guid id,
        string key,
        string name,
        DigitalServiceCategory category,
        string? iconKey,
        string? managementUrl,
        int sortOrder,
        DateTimeOffset createdAt)
    {
        return new DigitalService(
            id,
            key,
            name,
            isPredefined: true,
            ownerId: null,
            category,
            customCategoryName: null,
            iconKey,
            managementUrl,
            isActive: true,
            sortOrder,
            createdAt);
    }

    public static DigitalService CreateCustom(
        Guid id,
        Guid ownerId,
        string key,
        string name,
        DigitalServiceCategory category,
        string? customCategoryName,
        string? iconKey,
        string? managementUrl,
        DateTimeOffset createdAt)
    {
        return new DigitalService(
            id,
            key,
            name,
            isPredefined: false,
            ownerId,
            category,
            customCategoryName,
            iconKey,
            managementUrl,
            isActive: true,
            sortOrder: 0,
            createdAt);
    }

    private void SetKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException(
                "Digital service key cannot be empty.",
                nameof(key));
        }

        Key = key.Trim().ToLowerInvariant();
    }

    private void SetName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException(
                "Digital service name cannot be empty.",
                nameof(name));
        }

        Name = name.Trim();
    }

    private void SetCustomCategoryName(
        string? customCategoryName)
    {
        if (Category != DigitalServiceCategory.Other &&
            !string.IsNullOrWhiteSpace(customCategoryName))
        {
            throw new ArgumentException(
                "A custom category name can only be used with the Other category.",
                nameof(customCategoryName));
        }

        CustomCategoryName =
            string.IsNullOrWhiteSpace(customCategoryName)
                ? null
                : customCategoryName.Trim();
    }

    private void SetIconKey(string? iconKey)
    {
        IconKey = string.IsNullOrWhiteSpace(iconKey)
            ? null
            : iconKey.Trim();
    }

    private void SetManagementUrl(string? managementUrl)
    {
        if (string.IsNullOrWhiteSpace(managementUrl))
        {
            ManagementUrl = null;
            return;
        }

        var normalizedUrl = managementUrl.Trim();

        if (!Uri.TryCreate(normalizedUrl, UriKind.Absolute, out var uri) ||
            (uri.Scheme != Uri.UriSchemeHttp &&
             uri.Scheme != Uri.UriSchemeHttps))
        {
            throw new ArgumentException(
                "Management URL must be a valid HTTP or HTTPS URL.",
                nameof(managementUrl));
        }

        ManagementUrl = normalizedUrl;
    }
}
