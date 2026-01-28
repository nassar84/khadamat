namespace Khadamat.Application.DTOs;

public class AppSettingsDto
{
    public string ApplicationName { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public string PrimaryColor { get; set; } = string.Empty;
    public string SecondaryColor { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public bool IsMaintenanceMode { get; set; }
    public string WelcomeMessage { get; set; } = string.Empty;

    // System Features Control
    public bool AllowUserRegistration { get; set; }
    public bool RequireEmailVerification { get; set; }
    public int MaxServicesPerProvider { get; set; }
    public bool EnableReviewAutoApproval { get; set; }

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
