namespace Khadamat.Application.DTOs;

public class AppSettingsDto
{
    public string ApplicationName { get; set; } = string.Empty;
    public string ApplicationNameAr { get; set; } = string.Empty;
    public string ApplicationNameEn { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    
    // APK Settings
    public string ApkFilename { get; set; } = string.Empty;
    public string ApkIconUrl { get; set; } = string.Empty;

    public string PrimaryColor { get; set; } = string.Empty;
    public string SecondaryColor { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public bool IsMaintenanceMode { get; set; }
    public string WelcomeMessage { get; set; } = string.Empty;

    // Sound Settings
    public string OpenAppSound { get; set; } = string.Empty;
    public string FindServiceSound { get; set; } = string.Empty;
    public string OpenDetailsSound { get; set; } = string.Empty;
    public string MessageReceivedSound { get; set; } = string.Empty;
    public string NotificationReceivedSound { get; set; } = string.Empty;

    // System Features Control
    public bool AllowUserRegistration { get; set; }
    public bool RequireEmailVerification { get; set; }
    public int MaxServicesPerProvider { get; set; }
    public bool EnableReviewAutoApproval { get; set; }

    // Marketplace Settings (Adding these as they probably exist in entity but missed in DTO)
    public int MarketplaceDefaultListingDays { get; set; } = 30;
    public int MarketplaceMaxListingsPerUser { get; set; } = 10;
    public bool MarketplaceRequireApproval { get; set; } = true;
    public bool MarketplaceAutoExpire { get; set; } = true;

    // App Info & Social
    public string FacebookUrl { get; set; } = string.Empty;
    public string TwitterUrl { get; set; } = string.Empty;
    public string InstagramUrl { get; set; } = string.Empty;

    // Legal
    public string TermsAndConditions { get; set; } = string.Empty;
    public string PrivacyPolicy { get; set; } = string.Empty;
}

public class UpdateAppSettingsRequest : AppSettingsDto
{
}
