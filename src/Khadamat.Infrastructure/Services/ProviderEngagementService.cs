using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Khadamat.Application.Common.Models;
using Khadamat.Application.DTOs;
using Khadamat.Application.Interfaces;
using Khadamat.Domain.Entities;
using Khadamat.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Khadamat.Infrastructure.Services;

public class ProviderEngagementService : IProviderEngagementService
{
    private readonly KhadamatDbContext _context;
    private readonly INotificationService _notificationService;
    private readonly IConfiguration _configuration;

    public ProviderEngagementService(
        KhadamatDbContext context,
        INotificationService notificationService,
        IConfiguration configuration)
    {
        _context = context;
        _notificationService = notificationService;
        _configuration = configuration;
    }

    public async Task<EngagementKpiDto> GetEngagementKpisAsync()
    {
        var services = await _context.Services
            .Include(s => s.City)
            .Include(s => s.Images)
            .Include(s => s.ProviderProfile)
            .ToListAsync();

        var kpi = new EngagementKpiDto
        {
            TotalServices = services.Count
        };

        int totalPct = 0;
        foreach (var svc in services)
        {
            var comp = EvaluateServiceCompleteness(svc);
            totalPct += comp.Percentage;
            if (comp.Percentage >= 90)
                kpi.CompleteServices++;
            else
                kpi.IncompleteServices++;

            if (!comp.Checklist["MainImage"] || !comp.Checklist["WorkImages"])
                kpi.NeedsImages++;
            if (!comp.Checklist["Description"])
                kpi.NeedsDescription++;
            if (!comp.Checklist["Location"])
                kpi.NeedsLocation++;
            if (!comp.Checklist["Contacts"])
                kpi.NeedsContacts++;
        }

        kpi.AverageCompletenessPercentage = services.Count > 0 ? Math.Round((double)totalPct / services.Count, 1) : 0;

        // Logs counters
        var logs = await _context.ProviderEngagementLogs.ToListAsync();
        kpi.ConfirmedWhatsAppSent = logs.Count(l => l.Channel == "WhatsApp" && l.Status == "SendConfirmed");
        kpi.PushNotificationsSent = logs.Count(l => l.Channel == "PushNotification" && l.Status == "PushSent");
        kpi.InAppNotificationsSent = logs.Count(l => l.Channel == "InAppNotification");
        kpi.CompletedAfterEngagement = logs.Count(l => l.Status == "DataCompleted" || l.Status == "ImagesReceived");

        return kpi;
    }

    public async Task<List<ServiceEngagementSummaryDto>> GetEngageableServicesAsync(
        string? searchTerm = null,
        int? cityId = null,
        int? governorateId = null,
        string? profession = null,
        string? completenessFilter = null,
        string? contactStatus = null,
        string? missingField = null)
    {
        var query = _context.Services
            .Include(s => s.City).ThenInclude(c => c!.Governorate)
            .Include(s => s.Category)
            .Include(s => s.SubCategory)
            .Include(s => s.SpecializationEntity)
            .Include(s => s.ProviderProfile).ThenInclude(p => p!.City)
            .Include(s => s.Images)
            .AsNoTracking();

        if (cityId.HasValue && cityId.Value > 0)
        {
            query = query.Where(s => s.CityId == cityId || s.ProviderProfile!.CityId == cityId);
        }

        if (governorateId.HasValue && governorateId.Value > 0)
        {
            query = query.Where(s => s.City!.GovernorateId == governorateId.Value || 
                                     s.ProviderProfile!.City!.GovernorateId == governorateId.Value);
        }

        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var st = searchTerm.Trim();
            query = query.Where(s => s.Name.Contains(st) || 
                                     s.Description.Contains(st) || 
                                     (s.Phone1 != null && s.Phone1.Contains(st)) ||
                                     (s.WhatsApp != null && s.WhatsApp.Contains(st)) ||
                                     (s.ProviderProfile != null && s.ProviderProfile.BusinessName.Contains(st)) ||
                                     (s.Address != null && s.Address.Contains(st)));
        }

        var list = await query.OrderByDescending(s => s.CreatedAt).ToListAsync();

        // Get all engagement logs grouped by ServiceId to get latest status
        var serviceIds = list.Select(s => s.Id).ToList();
        var latestLogs = await _context.ProviderEngagementLogs
            .Where(l => serviceIds.Contains(l.ServiceId))
            .OrderByDescending(l => l.PreparedAt)
            .ToListAsync();

        var logsByService = latestLogs.GroupBy(l => l.ServiceId).ToDictionary(g => g.Key, g => g.ToList());
        var titleRules = await _context.ProfessionalTitleRules.Where(r => r.IsActive).OrderByDescending(r => r.Priority).ToListAsync();

        var result = new List<ServiceEngagementSummaryDto>();

        foreach (var svc in list)
        {
            var comp = EvaluateServiceCompleteness(svc);
            var detectedCity = ResolveCityName(svc);
            var detectedGov = svc.City?.Governorate?.Governorate_Name_AR ?? "المنوفية";
            var detectedTitle = ResolveProfessionalTitle(svc, titleRules);
            var providerName = !string.IsNullOrWhiteSpace(svc.ProviderProfile?.BusinessName)
                ? svc.ProviderProfile.BusinessName
                : svc.Name;

            var phone = !string.IsNullOrWhiteSpace(svc.WhatsApp) ? svc.WhatsApp : svc.Phone1;
            var formattedPhone = FormatEgyptianPhoneNumber(phone);

            var summary = new ServiceEngagementSummaryDto
            {
                ServiceId = svc.Id,
                ServiceName = svc.Name,
                ProviderName = providerName,
                ProviderUserId = svc.ProviderProfile?.UserId ?? svc.UserCreated,
                ProviderProfileId = svc.ProviderProfileId,
                Phone1 = svc.Phone1,
                WhatsApp = svc.WhatsApp,
                FormattedPhone = formattedPhone,
                CityId = svc.CityId ?? svc.ProviderProfile?.CityId,
                CityName = detectedCity,
                GovernorateId = svc.City?.GovernorateId,
                GovernorateName = detectedGov,
                CategoryName = svc.Category?.Name ?? svc.SubCategory?.Name,
                Specialization = svc.Specialization ?? svc.SpecializationEntity?.Name,
                DetectedTitle = detectedTitle,
                CompletenessPercentage = comp.Percentage,
                MissingFields = comp.MissingFields,
                HasMainImage = comp.Checklist["MainImage"],
                WorkImagesCount = svc.Images?.Count ?? 0,
                HasValidLocation = comp.Checklist["Location"],
                HasValidDescription = comp.Checklist["Description"],
                HasValidContacts = comp.Checklist["Contacts"],
                DoNotContact = svc.DoNotContact,
                CreatedAt = svc.CreatedAt
            };

            if (logsByService.TryGetValue(svc.Id, out var serviceLogs) && serviceLogs.Any())
            {
                var lastLog = serviceLogs.First();
                summary.LastContactDate = lastLog.SentConfirmedAt ?? lastLog.PreparedAt;
                summary.ContactAttemptsCount = serviceLogs.Count;
                summary.LastContactStatus = GetArabicStatusName(lastLog.Status);
                summary.LastContactChannel = lastLog.Channel;
            }

            // Apply in-memory filters
            if (!string.IsNullOrEmpty(completenessFilter))
            {
                if (completenessFilter == "incomplete" && summary.CompletenessPercentage >= 90) continue;
                if (completenessFilter == "complete" && summary.CompletenessPercentage < 90) continue;
                if (completenessFilter == "low" && summary.CompletenessPercentage > 50) continue;
            }

            if (!string.IsNullOrEmpty(missingField))
            {
                if (missingField == "images" && summary.HasMainImage && summary.WorkImagesCount > 0) continue;
                if (missingField == "description" && summary.HasValidDescription) continue;
                if (missingField == "location" && summary.HasValidLocation) continue;
                if (missingField == "contacts" && summary.HasValidContacts) continue;
            }

            if (!string.IsNullOrEmpty(contactStatus))
            {
                if (contactStatus == "not_contacted" && summary.ContactAttemptsCount > 0) continue;
                if (contactStatus == "contacted" && summary.ContactAttemptsCount == 0) continue;
                if (contactStatus == "confirmed" && summary.LastContactStatus != "تم تأكيد الإرسال") continue;
                if (contactStatus == "opt_out" && !summary.DoNotContact) continue;
            }

            if (!string.IsNullOrEmpty(profession))
            {
                var prof = profession.Trim().ToLowerInvariant();
                bool matches = (summary.Specialization != null && summary.Specialization.ToLowerInvariant().Contains(prof)) ||
                               (summary.CategoryName != null && summary.CategoryName.ToLowerInvariant().Contains(prof)) ||
                               (summary.ServiceName.ToLowerInvariant().Contains(prof));
                if (!matches) continue;
            }

            result.Add(summary);
        }

        return result;
    }

    public ServiceCompletenessResultDto EvaluateServiceCompleteness(Service service)
    {
        var result = new ServiceCompletenessResultDto();
        int score = 0;

        // 1. Service Title (15%)
        bool hasTitle = !string.IsNullOrWhiteSpace(service.Name) && service.Name.Trim().Length >= 3;
        if (hasTitle) score += 15;
        result.Checklist["Title"] = hasTitle;

        // 2. Description (15% if >= 25 chars, 5% if short)
        bool hasDesc = !string.IsNullOrWhiteSpace(service.Description) && service.Description.Trim().Length >= 25;
        if (hasDesc) score += 15;
        else if (!string.IsNullOrWhiteSpace(service.Description)) score += 5;
        result.Checklist["Description"] = hasDesc;
        if (!hasDesc) result.MissingFields.Add("استكمال وصف الخدمة والمميزات");

        // 3. Contacts (20%)
        bool hasPhone = !string.IsNullOrWhiteSpace(service.Phone1) || !string.IsNullOrWhiteSpace(service.WhatsApp);
        if (hasPhone) score += 20;
        result.Checklist["Contacts"] = hasPhone;
        if (!hasPhone) result.MissingFields.Add("أرقام التواصل أو الواتساب");

        // 4. Location & Address (15%)
        bool hasLocation = (service.CityId.HasValue && service.CityId.Value > 0) || 
                           (!string.IsNullOrWhiteSpace(service.Address) && service.Address.Trim().Length >= 5);
        if (hasLocation) score += 15;
        result.Checklist["Location"] = hasLocation;
        if (!hasLocation) result.MissingFields.Add("تحديد موقع الخدمة والعنوان");

        // 5. Main Image (15%)
        bool hasMainImg = !string.IsNullOrWhiteSpace(service.ImageUrl) && 
                          !service.ImageUrl.Contains("placeholder") && 
                          !service.ImageUrl.Contains("default");
        if (hasMainImg) score += 15;
        result.Checklist["MainImage"] = hasMainImg;
        if (!hasMainImg) result.MissingFields.Add("إضافة صورة شخصية أو شعار النشاط");

        // 6. Portfolio / Work Images (10%)
        bool hasWorkImages = service.Images != null && service.Images.Any(i => !i.IsDeleted);
        if (hasWorkImages) score += 10;
        result.Checklist["WorkImages"] = hasWorkImages;
        if (!hasWorkImages) result.MissingFields.Add("إضافة صور الأعمال السابقة والورشة");

        // 7. Category / Specialization (10%)
        bool hasCat = service.CategoryId.HasValue || service.SubCategoryId.HasValue || !string.IsNullOrWhiteSpace(service.Specialization);
        if (hasCat) score += 10;
        result.Checklist["Category"] = hasCat;

        result.Percentage = Math.Min(100, score);
        return result;
    }

    public async Task<PreparedEngagementMessageDto> PrepareEngagementMessageAsync(
        int serviceId,
        string reminderType,
        string channel = "WhatsApp",
        int? customCampaignId = null,
        string? customTitle = null,
        string? customCity = null,
        string? distinctionReason = null,
        bool? isDistinctionEnabled = null)
    {
        var service = await _context.Services
            .Include(s => s.City).ThenInclude(c => c!.Governorate)
            .Include(s => s.Category)
            .Include(s => s.SubCategory)
            .Include(s => s.SpecializationEntity)
            .Include(s => s.ProviderProfile).ThenInclude(p => p!.City)
            .Include(s => s.Images)
            .Include(s => s.Ratings)
            .FirstOrDefaultAsync(s => s.Id == serviceId);

        if (service == null)
            throw new KeyNotFoundException($"الخدمة رقم {serviceId} غير موجودة");

        var validRatings = service.Ratings?.Where(r => !r.IsDeleted).ToList() ?? new List<Rating>();
        int positiveRatingsCount = validRatings.Count(r => r.Stars >= 4);
        double avgRating = validRatings.Any() ? Math.Round(validRatings.Average(r => r.Stars), 1) : 0;

        // Distinction Configuration
        var distConfig = await _context.ServiceDistinctionConfigs.FirstOrDefaultAsync() ?? new ServiceDistinctionConfig();
        bool distEnabled = isDistinctionEnabled ?? (distConfig.IsEnabledGlobally && service.IsDistinctionEnabled);

        string finalReason = distinctionReason ?? "";
        if (string.IsNullOrWhiteSpace(finalReason) || finalReason == "Auto")
        {
            if (positiveRatingsCount > 0)
                finalReason = "PositiveReviews";
            else if (service.DistinctionReason == "CommunityNomination")
                finalReason = "CommunityNomination";
            else if (service.DistinctionReason == "SpecialPromotion")
                finalReason = "SpecialPromotion";
            else if (service.DistinctionReason == "AdminSelection")
                finalReason = "AdminSelection";
            else
                finalReason = "None";
        }

        // Strict rule: never claim PositiveReviews if there are no real documented reviews in DB
        if (finalReason == "PositiveReviews" && positiveRatingsCount == 0)
        {
            finalReason = "None";
        }

        string distinctionParagraph = "";
        if (distEnabled)
        {
            if (finalReason != "None")
            {
                string reasonText = finalReason switch
                {
                    "AdminSelection" => distConfig.AdminSelectionText,
                    "CommunityNomination" => distConfig.CommunityNominationText,
                    "PositiveReviews" => distConfig.PositiveReviewsText,
                    "SpecialPromotion" => distConfig.SpecialPromotionText,
                    _ => distConfig.AdminSelectionText
                };
                distinctionParagraph = $"{distConfig.HeaderText}\n\n{reasonText} ❤️\n\n{distConfig.FollowUpText}\n";
            }
            else
            {
                distinctionParagraph = $"{distConfig.GeneralWelcomeText}\n";
            }
        }

        var comp = EvaluateServiceCompleteness(service);
        var providerUserId = service.ProviderProfile?.UserId ?? service.UserCreated;
        bool hasPushDevice = false;
        if (!string.IsNullOrEmpty(providerUserId))
        {
            hasPushDevice = await _context.UserDevices.AnyAsync(d => d.UserId == providerUserId && d.IsActive);
        }

        var titleRules = await _context.ProfessionalTitleRules
            .Where(r => r.IsActive)
            .OrderByDescending(r => r.Priority)
            .ToListAsync();

        var profTitle = !string.IsNullOrWhiteSpace(customTitle)
            ? customTitle
            : ResolveProfessionalTitle(service, titleRules);

        var cityName = !string.IsNullOrWhiteSpace(customCity)
            ? customCity
            : ResolveCityName(service);

        var govName = service.City?.Governorate?.Governorate_Name_AR ?? "المنوفية";
        var providerName = !string.IsNullOrWhiteSpace(service.ProviderProfile?.BusinessName)
            ? service.ProviderProfile.BusinessName
            : service.Name;

        var phone = !string.IsNullOrWhiteSpace(service.WhatsApp) ? service.WhatsApp : service.Phone1;
        var cleanPhone = FormatEgyptianPhoneNumber(phone);

        // Resolve Marketing Campaign:
        MarketingCampaign? campaign = null;
        if (customCampaignId.HasValue && customCampaignId.Value > 0)
        {
            campaign = await _context.MarketingCampaigns.FindAsync(customCampaignId.Value);
        }

        if (campaign == null && (service.CityId.HasValue || !string.IsNullOrWhiteSpace(cityName)))
        {
            campaign = await _context.MarketingCampaigns
                .FirstOrDefaultAsync(c => c.IsActive && !c.IsDefault && 
                                         (c.CityId == service.CityId || c.CityName == cityName));
        }

        if (campaign == null)
        {
            campaign = await _context.MarketingCampaigns.FirstOrDefaultAsync(c => c.IsActive && c.IsDefault);
        }

        // Get Reminder Template:
        var reminder = await _context.ProviderEngagementReminders
            .FirstOrDefaultAsync(r => r.IsActive && r.ReminderType == reminderType);

        var baseUrl = _configuration["AppUrl"] ?? "https://khadamawy.eis-dev.com";
        var appDownloadUrl = campaign?.AppDownloadUrl ?? "https://play.google.com/store/apps/details?id=com.nassar84.khadamawy";
        var fbUrl = campaign?.FacebookCommunityUrl ?? "https://www.facebook.com/profile.php?id=61595461124579";
        var serviceUrl = $"{baseUrl.TrimEnd('/')}/share/service/{service.Id}";

        // Personalized greeting
        var greeting = profTitle.Contains("مس") || profTitle.Contains("دكتورة") || profTitle.Contains("باشمهندسة") || profTitle.Contains("أستاذة")
            ? $"أهلاً بيكي يا {profTitle} {providerName}"
            : $"أهلاً بيك يا {profTitle} {providerName}";

        // Message text resolution
        string rawTemplate = "";
        if (reminderType == "WelcomeAfterRegistration" && campaign != null && !string.IsNullOrWhiteSpace(campaign.MessageTemplate))
        {
            rawTemplate = campaign.MessageTemplate;
        }
        else if (reminder != null && !string.IsNullOrWhiteSpace(reminder.WhatsAppTemplate))
        {
            rawTemplate = reminder.WhatsAppTemplate;
        }
        else
        {
            rawTemplate = "👋 {PersonalizedGreeting} ❤️\n{DistinctionParagraph}\nتم تسجيل خدمتك ({ServiceName}) على خدماوي!\nزور خدمتك: {ServiceUrl}\nوحمل التطبيق: {AppDownloadUrl}";
        }

        var missingText = comp.MissingFields.Any() 
            ? string.Join("، ", comp.MissingFields)
            : "الملف مكتمل بنجاح";

        var fullMessage = rawTemplate
            .Replace("{PersonalizedGreeting}", greeting)
            .Replace("{DistinctionParagraph}", distinctionParagraph)
            .Replace("{ProviderName}", providerName)
            .Replace("{ProfessionalTitle}", profTitle)
            .Replace("{ServiceName}", service.Name)
            .Replace("{CityName}", !string.IsNullOrEmpty(cityName) ? cityName : "مدينتك")
            .Replace("{GovernorateName}", govName)
            .Replace("{ServiceUrl}", serviceUrl)
            .Replace("{AppDownloadUrl}", appDownloadUrl)
            .Replace("{FacebookPageUrl}", fbUrl)
            .Replace("{MissingFields}", missingText)
            .Replace("{ProfileCompletionPercentage}", $"{comp.Percentage}%");

        // If template did not have {DistinctionParagraph} placeholder, inject it right after the greeting
        if (!rawTemplate.Contains("{DistinctionParagraph}") && !string.IsNullOrWhiteSpace(distinctionParagraph))
        {
            fullMessage = fullMessage.Replace(greeting, $"{greeting}\n\n{distinctionParagraph.Trim()}");
        }

        var waClickUrl = !string.IsNullOrEmpty(cleanPhone)
            ? $"https://wa.me/{cleanPhone}?text={Uri.EscapeDataString(fullMessage)}"
            : "";

        var inAppTitle = reminder != null ? reminder.PushTitleTemplate.Replace("{ServiceName}", service.Name) : $"تنبيه بخصوص {service.Name}";
        var inAppBody = reminder != null ? reminder.PushBodyTemplate.Replace("{ServiceName}", service.Name) : "يرجى مراجعة وتحديث بيانات خدمتك";

        return new PreparedEngagementMessageDto
        {
            ServiceId = service.Id,
            RecipientName = providerName,
            RecipientPhone = cleanPhone,
            ProfessionalTitle = profTitle,
            PersonalizedGreeting = greeting,
            CityName = cityName,
            GovernorateName = govName,
            CampaignId = campaign?.Id,
            CampaignName = campaign?.CampaignName ?? "حملة عامة",
            Subject = reminder?.Title ?? "متابعة الخدمة",
            FullMessageText = fullMessage,
            WhatsAppClickUrl = waClickUrl,
            InAppTitle = inAppTitle,
            InAppBody = inAppBody,
            DeepLinkRoute = $"/service/{service.Id}",
            HasActivePushDevice = hasPushDevice,
            CompletenessPercentage = comp.Percentage,
            MissingFields = comp.MissingFields,
            DoNotContact = service.DoNotContact,
            DistinctionReason = finalReason,
            IsDistinctionEnabled = distEnabled,
            HasPositiveReviews = positiveRatingsCount > 0,
            PositiveReviewsCount = positiveRatingsCount,
            AverageRating = avgRating
        };
    }

    public async Task<ApiResponse<int>> ExecuteEngagementAsync(
        SendEngagementRequestDto request,
        string adminUserId,
        string adminUserName)
    {
        var service = await _context.Services
            .Include(s => s.City)
            .Include(s => s.ProviderProfile)
            .FirstOrDefaultAsync(s => s.Id == request.ServiceId);

        if (service == null)
            return ApiResponse<int>.Fail("الخدمة غير موجودة");

        if (service.DoNotContact)
            return ApiResponse<int>.Fail("صاحب الخدمة طلب عدم التواصل معه (Opt-Out). تم احترام طلبه.");

        var prepared = await PrepareEngagementMessageAsync(
            request.ServiceId,
            request.ReminderType,
            request.Channel,
            request.CampaignId,
            request.ProfessionalTitle,
            request.CityName,
            request.DistinctionReason,
            request.IsDistinctionIncluded);

        var finalMessage = !string.IsNullOrWhiteSpace(request.CustomMessage)
            ? request.CustomMessage
            : prepared.FullMessageText;

        var finalTitle = !string.IsNullOrWhiteSpace(request.CustomTitle)
            ? request.CustomTitle
            : prepared.InAppTitle;

        var log = new ProviderEngagementLog
        {
            ServiceId = service.Id,
            ProviderUserId = service.ProviderProfile?.UserId ?? service.UserCreated,
            ProviderProfileId = service.ProviderProfileId,
            Channel = request.Channel,
            ReminderType = request.ReminderType,
            RecipientName = prepared.RecipientName,
            RecipientPhone = prepared.RecipientPhone,
            ProfessionalTitle = request.ProfessionalTitle ?? prepared.ProfessionalTitle,
            CityName = request.CityName ?? prepared.CityName,
            CampaignId = request.CampaignId ?? prepared.CampaignId,
            CampaignName = prepared.CampaignName,
            Title = finalTitle,
            MessageText = finalMessage,
            AdminUserId = adminUserId,
            AdminUserName = adminUserName,
            PreparedAt = DateTime.UtcNow,
            DistinctionReason = request.DistinctionReason ?? prepared.DistinctionReason,
            IsDistinctionIncluded = request.IsDistinctionIncluded,
            AdminNotes = request.AdminNotes
        };

        if (!string.IsNullOrWhiteSpace(request.DistinctionReason))
        {
            service.SetDistinction(request.DistinctionReason, request.IsDistinctionIncluded, null);
        }

        if (request.Channel == "WhatsApp")
        {
            log.Status = "WhatsAppOpened";
            _context.ProviderEngagementLogs.Add(log);
            await _context.SaveChangesAsync();
            return ApiResponse<int>.Succeed(log.Id, "تم تجهيز رسالة واتساب وفتح المحادثة بنجاح. لا تنسَ الضغط على 'تأكيد إرسال الرسالة' بعد الإرسال الفعلي.");
        }
        else if (request.Channel == "InAppNotification")
        {
            var targetUser = service.ProviderProfile?.UserId ?? service.UserCreated;
            if (string.IsNullOrEmpty(targetUser))
            {
                log.Status = "InAppFailed";
                log.AdminNotes = (log.AdminNotes ?? "") + " [فشل: الخدمة غير مربوطة بمستخدم مسجل]";
                _context.ProviderEngagementLogs.Add(log);
                await _context.SaveChangesAsync();
                return ApiResponse<int>.Fail("فشل: هذه الخدمة غير مرتبطة بمستخدم مسجل في التطبيق.");
            }

            await _notificationService.SendNotificationAsync(
                targetUser,
                finalTitle,
                finalMessage,
                type: "ProviderEngagement",
                link: $"/service/{service.Id}",
                deliveryChannel: "InAppOnly");

            log.Status = "InAppCreated";
            log.SentConfirmedAt = DateTime.UtcNow;
            _context.ProviderEngagementLogs.Add(log);
            await _context.SaveChangesAsync();
            return ApiResponse<int>.Succeed(log.Id, "تم إرسال التنبيه الداخلي في التطبيق بنجاح!");
        }
        else if (request.Channel == "PushNotification")
        {
            var targetUser = service.ProviderProfile?.UserId ?? service.UserCreated;
            if (string.IsNullOrEmpty(targetUser))
            {
                log.Status = "PushFailed";
                log.AdminNotes = (log.AdminNotes ?? "") + " [فشل: الخدمة غير مربوطة بمستخدم مسجل]";
                _context.ProviderEngagementLogs.Add(log);
                await _context.SaveChangesAsync();
                return ApiResponse<int>.Fail("فشل: هذه الخدمة غير مرتبطة بمستخدم مسجل.");
            }

            bool hasDevice = await _context.UserDevices.AnyAsync(d => d.UserId == targetUser && d.IsActive);
            if (!hasDevice)
            {
                log.Status = "PushFailed";
                log.AdminNotes = (log.AdminNotes ?? "") + " [فشل: لا يوجد جهاز نشط مسجل لمقدم الخدمة]";
                _context.ProviderEngagementLogs.Add(log);
                await _context.SaveChangesAsync();
                return ApiResponse<int>.Fail("تعذر الإرسال: مقدم الخدمة لم يسجل الدخول من تطبيق الموبايل أو ليس لديه جهاز نشط.");
            }

            await _notificationService.SendNotificationAsync(
                targetUser,
                finalTitle,
                finalMessage,
                type: "ProviderEngagement",
                link: $"/service/{service.Id}",
                deliveryChannel: "PushOnly");

            log.Status = "PushSent";
            log.SentConfirmedAt = DateTime.UtcNow;
            _context.ProviderEngagementLogs.Add(log);
            await _context.SaveChangesAsync();
            return ApiResponse<int>.Succeed(log.Id, "تم إرسال الإشعار الفوري (Push Notification) بنجاح!");
        }

        return ApiResponse<int>.Fail("وسيلة التواصل غير صالحة");
    }

    public async Task<ApiResponse<bool>> ConfirmWhatsAppSentAsync(int logId, string? notes = null)
    {
        var log = await _context.ProviderEngagementLogs.FindAsync(logId);
        if (log == null)
            return ApiResponse<bool>.Fail("السجل غير موجود");

        log.Status = "SendConfirmed";
        log.SentConfirmedAt = DateTime.UtcNow;
        if (!string.IsNullOrEmpty(notes))
        {
            log.AdminNotes = string.IsNullOrEmpty(log.AdminNotes) ? notes : $"{log.AdminNotes} | {notes}";
        }

        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Succeed(true, "تم تسجيل تأكيد إرسال الرسالة بنجاح!");
    }

    public async Task<ApiResponse<bool>> UpdateEngagementStatusAsync(int logId, string newStatus, string? notes = null)
    {
        var log = await _context.ProviderEngagementLogs.FindAsync(logId);
        if (log == null)
            return ApiResponse<bool>.Fail("السجل غير موجود");

        log.Status = newStatus;
        if (!string.IsNullOrEmpty(notes))
        {
            log.AdminNotes = string.IsNullOrEmpty(log.AdminNotes) ? notes : $"{log.AdminNotes} | {notes}";
        }

        // If status indicates completion, check if service is now complete
        if (newStatus == "DataCompleted" || newStatus == "ImagesReceived")
        {
            var svc = await _context.Services.FindAsync(log.ServiceId);
            if (svc != null)
            {
                svc.Approve("تم استكمال البيانات وتأكيد المتابعة");
            }
        }
        else if (newStatus == "DoNotContact")
        {
            var svc = await _context.Services.FindAsync(log.ServiceId);
            if (svc != null)
            {
                svc.SetDoNotContact(true);
            }
        }

        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Succeed(true, "تم تحديث حالة المتابعة بنجاح");
    }

    public async Task<ApiResponse<bool>> ToggleOptOutAsync(int serviceId, bool optOut, string? notes = null)
    {
        var svc = await _context.Services.FindAsync(serviceId);
        if (svc == null)
            return ApiResponse<bool>.Fail("الخدمة غير موجودة");

        svc.SetDoNotContact(optOut);

        var log = new ProviderEngagementLog
        {
            ServiceId = serviceId,
            Channel = "System",
            ReminderType = "OptOutChange",
            RecipientName = svc.Name,
            RecipientPhone = svc.Phone1 ?? svc.WhatsApp ?? "",
            ProfessionalTitle = "إعدادات",
            CityName = "",
            Title = optOut ? "إيقاف المتابعة والتواصل (Opt-Out)" : "إعادة تفعيل المتابعة والتواصل",
            MessageText = optOut ? "طلب صاحب الخدمة إيقاف التواصل معه" : "تم إلغاء حظر التواصل",
            Status = optOut ? "DoNotContact" : "Active",
            AdminNotes = notes,
            PreparedAt = DateTime.UtcNow,
            SentConfirmedAt = DateTime.UtcNow
        };

        _context.ProviderEngagementLogs.Add(log);
        await _context.SaveChangesAsync();

        return ApiResponse<bool>.Succeed(true, optOut ? "تم حظر إرسال التذكيرات لهذا الحساب احتراماً لرغبته" : "تم تفعيل التواصل مجدداً");
    }

    public async Task<List<MarketingCampaignDto>> GetCampaignsAsync()
    {
        return await _context.MarketingCampaigns
            .OrderByDescending(c => c.IsActive)
            .ThenByDescending(c => c.CreatedAt)
            .Select(c => new MarketingCampaignDto
            {
                Id = c.Id,
                CampaignName = c.CampaignName,
                CityId = c.CityId,
                CityName = c.CityName,
                GovernorateId = c.GovernorateId,
                GovernorateName = c.GovernorateName,
                IsActive = c.IsActive,
                IsDefault = c.IsDefault,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                Slogan = c.Slogan,
                MessageTemplate = c.MessageTemplate,
                SubscriptionOfferMonths = c.SubscriptionOfferMonths,
                SubscriptionOfferText = c.SubscriptionOfferText,
                AppDownloadUrl = c.AppDownloadUrl,
                FacebookCommunityUrl = c.FacebookCommunityUrl,
                TargetStage = c.TargetStage
            })
            .ToListAsync();
    }

    public async Task<ApiResponse<int>> SaveCampaignAsync(MarketingCampaignDto dto)
    {
        MarketingCampaign? entity;
        if (dto.Id > 0)
        {
            entity = await _context.MarketingCampaigns.FindAsync(dto.Id);
            if (entity == null) return ApiResponse<int>.Fail("الحملة غير موجودة");
        }
        else
        {
            entity = new MarketingCampaign();
            _context.MarketingCampaigns.Add(entity);
        }

        entity.CampaignName = dto.CampaignName;
        entity.CityId = dto.CityId > 0 ? dto.CityId : null;
        entity.CityName = dto.CityName ?? "";
        entity.GovernorateId = dto.GovernorateId > 0 ? dto.GovernorateId : null;
        entity.GovernorateName = dto.GovernorateName ?? "";
        entity.IsActive = dto.IsActive;
        entity.IsDefault = dto.IsDefault;
        entity.StartDate = dto.StartDate;
        entity.EndDate = dto.EndDate;
        entity.Slogan = dto.Slogan;
        entity.MessageTemplate = dto.MessageTemplate;
        entity.SubscriptionOfferMonths = dto.SubscriptionOfferMonths;
        entity.SubscriptionOfferText = dto.SubscriptionOfferText;
        entity.AppDownloadUrl = dto.AppDownloadUrl;
        entity.FacebookCommunityUrl = dto.FacebookCommunityUrl;
        entity.TargetStage = dto.TargetStage;

        await _context.SaveChangesAsync();
        return ApiResponse<int>.Succeed(entity.Id, "تم حفظ بيانات الحملة بنجاح");
    }

    public async Task<ApiResponse<bool>> DeleteCampaignAsync(int id)
    {
        var campaign = await _context.MarketingCampaigns.FindAsync(id);
        if (campaign == null) return ApiResponse<bool>.Fail("الحملة غير موجودة");

        campaign.IsDeleted = true;
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Succeed(true, "تم حذف الحملة");
    }

    public async Task<List<ProfessionalTitleRuleDto>> GetProfessionalTitleRulesAsync()
    {
        return await _context.ProfessionalTitleRules
            .OrderByDescending(r => r.Priority)
            .Select(r => new ProfessionalTitleRuleDto
            {
                Id = r.Id,
                ProfessionKeyword = r.ProfessionKeyword,
                MaleTitle = r.MaleTitle,
                FemaleTitle = r.FemaleTitle,
                DefaultTitle = r.DefaultTitle,
                IsActive = r.IsActive,
                Priority = r.Priority,
                Notes = r.Notes
            })
            .ToListAsync();
    }

    public async Task<ApiResponse<int>> SaveProfessionalTitleRuleAsync(ProfessionalTitleRuleDto dto)
    {
        ProfessionalTitleRule? entity;
        if (dto.Id > 0)
        {
            entity = await _context.ProfessionalTitleRules.FindAsync(dto.Id);
            if (entity == null) return ApiResponse<int>.Fail("القاعدة غير موجودة");
        }
        else
        {
            entity = new ProfessionalTitleRule();
            _context.ProfessionalTitleRules.Add(entity);
        }

        entity.ProfessionKeyword = dto.ProfessionKeyword.Trim();
        entity.MaleTitle = dto.MaleTitle.Trim();
        entity.FemaleTitle = dto.FemaleTitle.Trim();
        entity.DefaultTitle = dto.DefaultTitle.Trim();
        entity.IsActive = dto.IsActive;
        entity.Priority = dto.Priority;
        entity.Notes = dto.Notes;

        await _context.SaveChangesAsync();
        return ApiResponse<int>.Succeed(entity.Id, "تم حفظ القاعدة بنجاح");
    }

    public async Task<ApiResponse<bool>> DeleteProfessionalTitleRuleAsync(int id)
    {
        var rule = await _context.ProfessionalTitleRules.FindAsync(id);
        if (rule == null) return ApiResponse<bool>.Fail("القاعدة غير موجودة");

        rule.IsDeleted = true;
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Succeed(true, "تم حذف القاعدة");
    }

    public async Task<List<ProviderEngagementReminderDto>> GetReminderTemplatesAsync()
    {
        return await _context.ProviderEngagementReminders
            .OrderBy(r => r.DisplayOrder)
            .Select(r => new ProviderEngagementReminderDto
            {
                Id = r.Id,
                ReminderType = r.ReminderType,
                Title = r.Title,
                Description = r.Description,
                TargetChannel = r.TargetChannel,
                WhatsAppTemplate = r.WhatsAppTemplate,
                PushTitleTemplate = r.PushTitleTemplate,
                PushBodyTemplate = r.PushBodyTemplate,
                DeepLinkRoute = r.DeepLinkRoute,
                DaysAfterRegistration = r.DaysAfterRegistration,
                IsActive = r.IsActive,
                DisplayOrder = r.DisplayOrder
            })
            .ToListAsync();
    }

    public async Task<ApiResponse<bool>> SaveReminderTemplateAsync(ProviderEngagementReminderDto dto)
    {
        var entity = await _context.ProviderEngagementReminders.FindAsync(dto.Id);
        if (entity == null) return ApiResponse<bool>.Fail("القالب غير موجود");

        entity.Title = dto.Title;
        entity.Description = dto.Description;
        entity.TargetChannel = dto.TargetChannel;
        entity.WhatsAppTemplate = dto.WhatsAppTemplate;
        entity.PushTitleTemplate = dto.PushTitleTemplate;
        entity.PushBodyTemplate = dto.PushBodyTemplate;
        entity.DeepLinkRoute = dto.DeepLinkRoute;
        entity.DaysAfterRegistration = dto.DaysAfterRegistration;
        entity.IsActive = dto.IsActive;
        entity.DisplayOrder = dto.DisplayOrder;

        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Succeed(true, "تم حفظ القالب بنجاح");
    }

    public async Task<List<EngagementLogDto>> GetEngagementLogsAsync(int? serviceId = null, int limit = 100)
    {
        var q = _context.ProviderEngagementLogs
            .Include(l => l.Service)
            .AsNoTracking();

        if (serviceId.HasValue && serviceId.Value > 0)
        {
            q = q.Where(l => l.ServiceId == serviceId.Value);
        }

        return await q.OrderByDescending(l => l.PreparedAt)
            .Take(limit)
            .Select(l => new EngagementLogDto
            {
                Id = l.Id,
                ServiceId = l.ServiceId,
                ServiceName = l.Service != null ? l.Service.Name : $"خدمة #{l.ServiceId}",
                RecipientName = l.RecipientName,
                RecipientPhone = l.RecipientPhone,
                ProfessionalTitle = l.ProfessionalTitle,
                CityName = l.CityName,
                Channel = l.Channel,
                ReminderType = l.ReminderType,
                ReminderTypeTitle = GetReminderTitle(l.ReminderType),
                Title = l.Title,
                MessageText = l.MessageText,
                Status = l.Status,
                StatusArabic = GetArabicStatusName(l.Status),
                AdminUserName = l.AdminUserName,
                PreparedAt = l.PreparedAt,
                SentConfirmedAt = l.SentConfirmedAt,
                AdminNotes = l.AdminNotes
            })
            .ToListAsync();
    }

    // ── Helper Methods ────────────────────────────────────────────────────────

    private string ResolveCityName(Service service)
    {
        if (service.City != null && !string.IsNullOrWhiteSpace(service.City.City_Name_AR))
            return service.City.City_Name_AR.Trim();

        // Check address text for famous cities
        if (!string.IsNullOrWhiteSpace(service.Address))
        {
            var addr = service.Address;
            if (addr.Contains("سرس") || addr.Contains("سرس الليان")) return "سرس الليان";
            if (addr.Contains("شبين") || addr.Contains("شبين الكوم")) return "شبين الكوم";
            if (addr.Contains("منوف")) return "منوف";
            if (addr.Contains("أشمون") || addr.Contains("اشمون")) return "أشمون";
            if (addr.Contains("الباجور")) return "الباجور";
            if (addr.Contains("قويسنا")) return "قويسنا";
        }

        if (service.ProviderProfile?.City != null && !string.IsNullOrWhiteSpace(service.ProviderProfile.City.City_Name_AR))
            return service.ProviderProfile.City.City_Name_AR.Trim();

        if (!string.IsNullOrWhiteSpace(service.ProviderProfile?.Location))
            return service.ProviderProfile.Location.Trim();

        return "سرس الليان"; // Default launch city
    }

    private string ResolveProfessionalTitle(Service service, List<ProfessionalTitleRule> rules)
    {
        var textToScan = $"{service.Name} {service.Specialization} {service.Category?.Name} {service.SubCategory?.Name} {service.Description}".ToLowerInvariant();

        foreach (var rule in rules)
        {
            if (string.IsNullOrWhiteSpace(rule.ProfessionKeyword)) continue;
            var kw = rule.ProfessionKeyword.Trim().ToLowerInvariant();

            if (textToScan.Contains(kw))
            {
                // Strict requirement: Teachers are ALWAYS "مستر" / "مس"
                if (kw == "مدرس" || kw == "معلم")
                {
                    return textToScan.Contains("مدرسة") || textToScan.Contains("معلمة") ? "مس" : "مستر";
                }

                // If feminine indicators present
                if (textToScan.Contains("طبيبة") || textToScan.Contains("مهندسة") || textToScan.Contains("دكتورة") || textToScan.Contains("أستاذة") || textToScan.Contains("محامية"))
                {
                    return !string.IsNullOrEmpty(rule.FemaleTitle) ? rule.FemaleTitle : rule.DefaultTitle;
                }

                return !string.IsNullOrEmpty(rule.MaleTitle) ? rule.MaleTitle : rule.DefaultTitle;
            }
        }

        return "أستاذ";
    }

    private string FormatEgyptianPhoneNumber(string? rawPhone)
    {
        if (string.IsNullOrWhiteSpace(rawPhone)) return "";

        // Normalize Arabic digits
        var normalized = rawPhone
            .Replace('٠', '0').Replace('١', '1').Replace('٢', '2').Replace('٣', '3').Replace('٤', '4')
            .Replace('٥', '5').Replace('٦', '6').Replace('٧', '7').Replace('٨', '8').Replace('٩', '9');

        // Extract digits only
        var digits = Regex.Replace(normalized, @"[^\d]", "");

        if (digits.StartsWith("002")) digits = digits.Substring(2);
        if (digits.StartsWith("+2")) digits = digits.Substring(2);

        // If local 11-digit mobile starting with 01
        if (digits.StartsWith("01") && digits.Length == 11)
        {
            return "2" + digits; // e.g. 2010xxxxxxxx
        }

        // If 10-digit mobile starting with 1
        if (digits.StartsWith("1") && digits.Length == 10)
        {
            return "20" + digits;
        }

        // If already 2010... (12 digits)
        if (digits.StartsWith("201") && digits.Length == 12)
        {
            return digits;
        }

        return digits;
    }

    private static string GetArabicStatusName(string status)
    {
        return status switch
        {
            "Prepared" => "تم تجهيز الرسالة",
            "WhatsAppOpened" => "تم فتح واتساب",
            "SendConfirmed" => "تم تأكيد الإرسال",
            "PushSent" => "تم إرسال Push",
            "PushFailed" => "فشل إرسال Push",
            "InAppCreated" => "تم إنشاء تنبيه داخلي",
            "InAppFailed" => "فشل التنبيه الداخلي",
            "ProviderReplied" => "تم الرد من صاحب الخدمة",
            "ImagesReceived" => "تم استلام صور",
            "DataCompleted" => "تم استكمال البيانات",
            "DoNotContact" => "طلب عدم التواصل",
            _ => status
        };
    }

    private static string GetReminderTitle(string type)
    {
        return type switch
        {
            "WelcomeAfterRegistration" => "ترحيب بعد التسجيل",
            "CompleteBasicData" => "استكمال البيانات",
            "AddProfileOrLogo" => "إضافة صورة أو لوجو",
            "AddWorkImages" => "إضافة صور الأعمال",
            "CompleteDescription" => "استكمال الوصف",
            "CompleteLocation" => "تحديد الموقع",
            "UpdateContacts" => "تحديث أرقام التواصل",
            "UpdateProfile" => "تحديث الملف الشخصي",
            "EncourageEngagement" => "تنشيط وتشجيع",
            "Custom" => "رسالة مخصصة",
            _ => type
        };
    }

    public async Task<ServiceDistinctionConfigDto> GetDistinctionConfigAsync()
    {
        var config = await _context.ServiceDistinctionConfigs.FirstOrDefaultAsync();
        if (config == null)
        {
            config = new ServiceDistinctionConfig();
            _context.ServiceDistinctionConfigs.Add(config);
            await _context.SaveChangesAsync();
        }

        return new ServiceDistinctionConfigDto
        {
            Id = config.Id,
            IsEnabledGlobally = config.IsEnabledGlobally,
            HeaderText = config.HeaderText,
            AdminSelectionText = config.AdminSelectionText,
            CommunityNominationText = config.CommunityNominationText,
            PositiveReviewsText = config.PositiveReviewsText,
            SpecialPromotionText = config.SpecialPromotionText,
            GeneralWelcomeText = config.GeneralWelcomeText,
            FollowUpText = config.FollowUpText
        };
    }

    public async Task<ApiResponse<bool>> SaveDistinctionConfigAsync(ServiceDistinctionConfigDto dto)
    {
        var config = await _context.ServiceDistinctionConfigs.FirstOrDefaultAsync();
        if (config == null)
        {
            config = new ServiceDistinctionConfig();
            _context.ServiceDistinctionConfigs.Add(config);
        }

        config.UpdateConfig(
            dto.IsEnabledGlobally,
            dto.HeaderText,
            dto.AdminSelectionText,
            dto.CommunityNominationText,
            dto.PositiveReviewsText,
            dto.SpecialPromotionText,
            dto.GeneralWelcomeText,
            dto.FollowUpText);

        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Succeed(true, "تم حفظ إعدادات تميز الخدمات بنجاح");
    }

    public async Task<ApiResponse<bool>> SetServiceDistinctionAsync(int serviceId, string reason, bool isEnabled, string? notes = null)
    {
        var service = await _context.Services.FindAsync(serviceId);
        if (service == null)
            return ApiResponse<bool>.Fail("الخدمة غير موجودة");

        service.SetDistinction(reason, isEnabled, notes);
        await _context.SaveChangesAsync();
        return ApiResponse<bool>.Succeed(true, "تم تحديث تميز الخدمة بنجاح");
    }
}
