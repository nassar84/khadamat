
namespace Khadamat.Domain.Entities;

public class AppSettings : BaseEntity
{
    public string ApplicationName { get; set; } = "خدماوي";
    public string ApplicationNameAr { get; set; } = "خدماوي";
    public string ApplicationNameEn { get; set; } = "Khadamawi";
    public string LogoUrl { get; set; } = "";
    
    // APK Settings
    public string ApkFilename { get; set; } = "khadamat.apk";
    public string ApkIconUrl { get; set; } = "";

    public string PrimaryColor { get; set; } = "#6366f1";
    public string SecondaryColor { get; set; } = "#a855f7";
    public string ContactEmail { get; set; } = "";
    public string ContactPhone { get; set; } = "";
    public bool IsMaintenanceMode { get; set; } = false;
    public string WelcomeMessage { get; set; } = "مرحباً بكم في منصة خدماوي";
    
    // Sound Settings (Filenames in wwwroot/audio or similar)
    public string OpenAppSound { get; set; } = "bic_ring1.mp3";
    public string FindServiceSound { get; set; } = "find_service.mp3";
    public string OpenDetailsSound { get; set; } = "open_details.mp3";
    public string MessageReceivedSound { get; set; } = "message_received.mp3";
    public string NotificationReceivedSound { get; set; } = "notification_received.mp3";
    
    // System Features Control
    public bool AllowUserRegistration { get; set; } = true;
    public bool RequireEmailVerification { get; set; } = false;
    public int MaxServicesPerProvider { get; set; } = 10;
    public bool EnableReviewAutoApproval { get; set; } = true;

    // Marketplace Settings
    public int MarketplaceDefaultListingDays { get; set; } = 30;  // مدة عرض الإعلان الافتراضية بالأيام
    public int MarketplaceMaxListingsPerUser { get; set; } = 10; // الحد الأقصى للإعلانات لكل مستخدم
    public bool MarketplaceRequireApproval { get; set; } = true;  // هل يحتاج الإعلان لموافقة
    public bool MarketplaceAutoExpire { get; set; } = true;        // إنهاء الإعلانات تلقائياً عند انتهاء المدة

    // App Info & Social
    public string FacebookUrl { get; set; } = "";
    public string TwitterUrl { get; set; } = "";
    public string InstagramUrl { get; set; } = "";
    
    // Legal
    public string TermsAndConditions { get; set; } = "";
    public string PrivacyPolicy { get; set; } = "";
}
