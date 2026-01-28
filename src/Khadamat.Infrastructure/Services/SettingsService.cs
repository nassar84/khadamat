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
            LogoUrl = settings.LogoUrl,
            PrimaryColor = settings.PrimaryColor,
            SecondaryColor = settings.SecondaryColor,
            ContactEmail = settings.ContactEmail,
            ContactPhone = settings.ContactPhone,
            IsMaintenanceMode = settings.IsMaintenanceMode,
            WelcomeMessage = settings.WelcomeMessage,
            AllowUserRegistration = settings.AllowUserRegistration,
            RequireEmailVerification = settings.RequireEmailVerification,
            MaxServicesPerProvider = settings.MaxServicesPerProvider,
            EnableReviewAutoApproval = settings.EnableReviewAutoApproval,
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
        settings.LogoUrl = request.LogoUrl;
        settings.PrimaryColor = request.PrimaryColor;
        settings.SecondaryColor = request.SecondaryColor;
        settings.ContactEmail = request.ContactEmail;
        settings.ContactPhone = request.ContactPhone;
        settings.IsMaintenanceMode = request.IsMaintenanceMode;
        settings.WelcomeMessage = request.WelcomeMessage;
        settings.AllowUserRegistration = request.AllowUserRegistration;
        settings.RequireEmailVerification = request.RequireEmailVerification;
        settings.MaxServicesPerProvider = request.MaxServicesPerProvider;
        settings.EnableReviewAutoApproval = request.EnableReviewAutoApproval;
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
