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
            settings = new AppSettings();
            _context.AppSettings.Add(settings);
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
            PrivacyPolicy = settings.PrivacyPolicy
        });
    }

    public async Task<ApiResponse<bool>> UpdateSettingsAsync(UpdateAppSettingsRequest request)
    {
        var settings = await _context.AppSettings.FirstOrDefaultAsync();
        
        if (settings == null)
        {
            settings = new AppSettings();
            _context.AppSettings.Add(settings);
        }

        settings.ApplicationName = request.ApplicationName;
        settings.ApplicationNameAr = request.ApplicationNameAr;
        settings.ApplicationNameEn = request.ApplicationNameEn;
        settings.LogoUrl = request.LogoUrl;
        settings.ApkFilename = request.ApkFilename;
        settings.ApkIconUrl = request.ApkIconUrl;
        settings.PrimaryColor = request.PrimaryColor;
        settings.SecondaryColor = request.SecondaryColor;
        settings.ContactEmail = request.ContactEmail;
        settings.ContactPhone = request.ContactPhone;
        settings.IsMaintenanceMode = request.IsMaintenanceMode;
        settings.WelcomeMessage = request.WelcomeMessage;
        settings.OpenAppSound = request.OpenAppSound;
        settings.FindServiceSound = request.FindServiceSound;
        settings.OpenDetailsSound = request.OpenDetailsSound;
        settings.MessageReceivedSound = request.MessageReceivedSound;
        settings.NotificationReceivedSound = request.NotificationReceivedSound;
        settings.AllowUserRegistration = request.AllowUserRegistration;
        settings.RequireEmailVerification = request.RequireEmailVerification;
        settings.MaxServicesPerProvider = request.MaxServicesPerProvider;
        settings.EnableReviewAutoApproval = request.EnableReviewAutoApproval;
        settings.MarketplaceDefaultListingDays = request.MarketplaceDefaultListingDays;
        settings.MarketplaceMaxListingsPerUser = request.MarketplaceMaxListingsPerUser;
        settings.MarketplaceRequireApproval = request.MarketplaceRequireApproval;
        settings.MarketplaceAutoExpire = request.MarketplaceAutoExpire;
        settings.FacebookUrl = request.FacebookUrl;
        settings.TwitterUrl = request.TwitterUrl;
        settings.InstagramUrl = request.InstagramUrl;
        settings.TermsAndConditions = request.TermsAndConditions;
        settings.PrivacyPolicy = request.PrivacyPolicy;
        settings.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Succeed(true, "تم تحديث الإعدادات بنجاح");
    }
}
