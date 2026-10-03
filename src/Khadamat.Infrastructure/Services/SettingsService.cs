using Khadamat.Application.Common.Models;
using Khadamat.Application.DTOs;
using Khadamat.Application.Interfaces;
using Khadamat.Domain.Entities;
using Khadamat.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Khadamat.Infrastructure.Services;

public class SettingsService : ISettingsService
{
    private readonly KhadamatDbContext _context;

    public SettingsService(KhadamatDbContext context)
    {
        _context = context;
    }

    public async Task<ApiResponse<AppSettingsDto>> GetSettingsAsync()
    {
        var settings = await _context.AppSettings.FirstOrDefaultAsync();
        
        if (settings == null)
        {
            // Seed default settings if not exists
            settings = new AppSettings { ContactEmail = "khadamawy@gmail.com" };
            _context.AppSettings.Add(settings);
            await _context.SaveChangesAsync();
        }
        else if (string.IsNullOrWhiteSpace(settings.ContactEmail))
        {
            settings.ContactEmail = "khadamawy@gmail.com";
            await _context.SaveChangesAsync();
        }

        return ApiResponse<AppSettingsDto>.Succeed(new AppSettingsDto
        {
            ApplicationName = settings.ApplicationName,
            ApplicationNameAr = settings.ApplicationNameAr,
            ApplicationNameEn = settings.ApplicationNameEn,
            LogoUrl = settings.LogoUrl,
            ApkFilename = settings.ApkFilename,
            ApkIconUrl = settings.ApkIconUrl,
            PrimaryColor = settings.PrimaryColor,
            SecondaryColor = settings.SecondaryColor,
            ContactEmail = settings.ContactEmail,
            ContactPhone = settings.ContactPhone,
            ContactWhatsApp = settings.ContactWhatsApp,
            InstaPayNumber = settings.InstaPayNumber,
            VodafoneCashNumber = settings.VodafoneCashNumber,
            OtherWalletNumber = settings.OtherWalletNumber,
            PaymentInstructions = settings.PaymentInstructions,
            IsMaintenanceMode = settings.IsMaintenanceMode,
            WelcomeMessage = settings.WelcomeMessage,
            OpenAppSound = settings.OpenAppSound,
            FindServiceSound = settings.FindServiceSound,
            OpenDetailsSound = settings.OpenDetailsSound,
            MessageReceivedSound = settings.MessageReceivedSound,
            NotificationReceivedSound = settings.NotificationReceivedSound,
            AllowUserRegistration = settings.AllowUserRegistration,
            RequireEmailVerification = settings.RequireEmailVerification,
            MaxServicesPerProvider = settings.MaxServicesPerProvider,
            EnableReviewAutoApproval = settings.EnableReviewAutoApproval,
            MarketplaceDefaultListingDays = settings.MarketplaceDefaultListingDays,
            MarketplaceMaxListingsPerUser = settings.MarketplaceMaxListingsPerUser,
            MarketplaceRequireApproval = settings.MarketplaceRequireApproval,
            MarketplaceAutoExpire = settings.MarketplaceAutoExpire,
            FacebookUrl = settings.FacebookUrl,
            TwitterUrl = settings.TwitterUrl,
            InstagramUrl = settings.InstagramUrl,
            TermsAndConditions = settings.TermsAndConditions,
            PrivacyPolicy = settings.PrivacyPolicy,
            AppShareUrl = settings.AppShareUrl,
            AppShareText = settings.AppShareText,
            AppStoreUrl = settings.AppStoreUrl,
            GooglePlayUrl = settings.GooglePlayUrl
        });
    }

    public async Task<ApiResponse<bool>> UpdateSettingsAsync(UpdateAppSettingsRequest request)
    {
        try
        {
            var settings = await _context.AppSettings.FirstOrDefaultAsync();
            
            if (settings == null)
            {
                settings = new AppSettings();
                _context.AppSettings.Add(settings);
            }

            if (!string.IsNullOrEmpty(request.ApplicationName)) settings.ApplicationName = request.ApplicationName;
            if (!string.IsNullOrEmpty(request.ApplicationNameAr)) settings.ApplicationNameAr = request.ApplicationNameAr;
            if (!string.IsNullOrEmpty(request.ApplicationNameEn)) settings.ApplicationNameEn = request.ApplicationNameEn;
            if (request.LogoUrl != null) settings.LogoUrl = request.LogoUrl;
            if (request.ApkFilename != null) settings.ApkFilename = request.ApkFilename;
            if (request.ApkIconUrl != null) settings.ApkIconUrl = request.ApkIconUrl;
            if (!string.IsNullOrEmpty(request.PrimaryColor)) settings.PrimaryColor = request.PrimaryColor;
            if (!string.IsNullOrEmpty(request.SecondaryColor)) settings.SecondaryColor = request.SecondaryColor;
            if (request.ContactEmail != null) settings.ContactEmail = request.ContactEmail;
            if (request.ContactPhone != null) settings.ContactPhone = request.ContactPhone;
            if (request.ContactWhatsApp != null) settings.ContactWhatsApp = request.ContactWhatsApp;
            if (request.InstaPayNumber != null) settings.InstaPayNumber = request.InstaPayNumber;
            if (request.VodafoneCashNumber != null) settings.VodafoneCashNumber = request.VodafoneCashNumber;
            if (request.OtherWalletNumber != null) settings.OtherWalletNumber = request.OtherWalletNumber;
            if (request.PaymentInstructions != null) settings.PaymentInstructions = request.PaymentInstructions;
            settings.IsMaintenanceMode = request.IsMaintenanceMode;
            if (request.WelcomeMessage != null) settings.WelcomeMessage = request.WelcomeMessage;
            if (request.OpenAppSound != null) settings.OpenAppSound = request.OpenAppSound;
            if (request.FindServiceSound != null) settings.FindServiceSound = request.FindServiceSound;
            if (request.OpenDetailsSound != null) settings.OpenDetailsSound = request.OpenDetailsSound;
            if (request.MessageReceivedSound != null) settings.MessageReceivedSound = request.MessageReceivedSound;
            if (request.NotificationReceivedSound != null) settings.NotificationReceivedSound = request.NotificationReceivedSound;
            settings.AllowUserRegistration = request.AllowUserRegistration;
            settings.RequireEmailVerification = request.RequireEmailVerification;
            if (request.MaxServicesPerProvider > 0) settings.MaxServicesPerProvider = request.MaxServicesPerProvider;
            settings.EnableReviewAutoApproval = request.EnableReviewAutoApproval;
            if (request.MarketplaceDefaultListingDays > 0) settings.MarketplaceDefaultListingDays = request.MarketplaceDefaultListingDays;
            if (request.MarketplaceMaxListingsPerUser > 0) settings.MarketplaceMaxListingsPerUser = request.MarketplaceMaxListingsPerUser;
            settings.MarketplaceRequireApproval = request.MarketplaceRequireApproval;
            settings.MarketplaceAutoExpire = request.MarketplaceAutoExpire;
            if (request.FacebookUrl != null) settings.FacebookUrl = request.FacebookUrl;
            if (request.TwitterUrl != null) settings.TwitterUrl = request.TwitterUrl;
            if (request.InstagramUrl != null) settings.InstagramUrl = request.InstagramUrl;
            if (request.TermsAndConditions != null) settings.TermsAndConditions = request.TermsAndConditions;
            if (request.PrivacyPolicy != null) settings.PrivacyPolicy = request.PrivacyPolicy;
            if (request.AppShareUrl != null) settings.AppShareUrl = request.AppShareUrl;
            if (request.AppShareText != null) settings.AppShareText = request.AppShareText;
            if (request.AppStoreUrl != null) settings.AppStoreUrl = request.AppStoreUrl;
            if (request.GooglePlayUrl != null) settings.GooglePlayUrl = request.GooglePlayUrl;
            settings.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return ApiResponse<bool>.Succeed(true, "تم تحديث الإعدادات بنجاح");
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.Fail($"خطأ في حفظ الإعدادات بقاعدة البيانات: {ex.Message}");
        }
    }
}
