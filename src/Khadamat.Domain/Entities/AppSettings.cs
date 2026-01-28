
namespace Khadamat.Domain.Entities;

public class AppSettings : BaseEntity
{
    public string ApplicationName { get; set; } = "خدمات";
    public string LogoUrl { get; set; } = "";
    public string PrimaryColor { get; set; } = "#6366f1";
    public string SecondaryColor { get; set; } = "#a855f7";
    public string ContactEmail { get; set; } = "";
    public string ContactPhone { get; set; } = "";
    public bool IsMaintenanceMode { get; set; } = false;
    public string WelcomeMessage { get; set; } = "مرحباً بكم في تطبيق خدمات";
    
    // System Features Control
    public bool AllowUserRegistration { get; set; } = true;
    public bool RequireEmailVerification { get; set; } = false;
    public int MaxServicesPerProvider { get; set; } = 10;
    public bool EnableReviewAutoApproval { get; set; } = true;

    // App Info & Social
    public string FacebookUrl { get; set; } = "";
    public string TwitterUrl { get; set; } = "";
    public string InstagramUrl { get; set; } = "";
    
    // Legal
    public string TermsAndConditions { get; set; } = "";
    public string PrivacyPolicy { get; set; } = "";
}
