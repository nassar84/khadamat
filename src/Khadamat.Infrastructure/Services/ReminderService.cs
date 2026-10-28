using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Khadamat.Application.DTOs;
using Khadamat.Application.Interfaces;
using Khadamat.Domain.Entities;
using Khadamat.Domain.Enums;
using Khadamat.Infrastructure.Persistence;

namespace Khadamat.Infrastructure.Services;

public class ReminderService : IReminderService
{
    private readonly KhadamatDbContext _context;
    private readonly INotificationService _notificationService;

    public ReminderService(KhadamatDbContext context, INotificationService notificationService)
    {
        _context = context;
        _notificationService = notificationService;
    }

    public async Task<List<ReminderCampaignDto>> GetRemindersAsync(ReminderFilterDto filter)
    {
        var query = _context.ReminderCampaigns.AsNoTracking().Where(r => !r.IsDeleted);

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(r => r.Title.Contains(term) || 
                                     r.Message.Contains(term) || 
                                     (r.InternalDescription != null && r.InternalDescription.Contains(term)));
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(r => r.Status == filter.Status.Value);
        }

        if (filter.TargetAudience.HasValue)
        {
            query = query.Where(r => r.TargetAudience == filter.TargetAudience.Value);
        }

        if (filter.DisplayType.HasValue)
        {
            query = query.Where(r => r.DisplayType == filter.DisplayType.Value);
        }

        query = query.OrderByDescending(r => r.CreatedAt);

        if (filter.Page > 0 && filter.PageSize > 0)
        {
            query = query.Skip((filter.Page - 1) * filter.PageSize).Take(filter.PageSize);
        }

        var items = await query.ToListAsync();
        return items.Select(MapToDto).ToList();
    }

    public async Task<int> GetRemindersCountAsync(ReminderFilterDto filter)
    {
        var query = _context.ReminderCampaigns.AsNoTracking().Where(r => !r.IsDeleted);

        if (!string.IsNullOrWhiteSpace(filter.SearchTerm))
        {
            var term = filter.SearchTerm.Trim();
            query = query.Where(r => r.Title.Contains(term) || 
                                     r.Message.Contains(term) || 
                                     (r.InternalDescription != null && r.InternalDescription.Contains(term)));
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(r => r.Status == filter.Status.Value);
        }

        if (filter.TargetAudience.HasValue)
        {
            query = query.Where(r => r.TargetAudience == filter.TargetAudience.Value);
        }

        if (filter.DisplayType.HasValue)
        {
            query = query.Where(r => r.DisplayType == filter.DisplayType.Value);
        }

        return await query.CountAsync();
    }

    public async Task<ReminderCampaignDto?> GetReminderByIdAsync(int id)
    {
        var item = await _context.ReminderCampaigns.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

        return item == null ? null : MapToDto(item);
    }

    public async Task<ReminderCampaignDto> CreateReminderAsync(CreateReminderCampaignDto dto, string? userId)
    {
        var entity = new ReminderCampaign
        {
            Title = dto.Title.Trim(),
            InternalDescription = dto.InternalDescription?.Trim(),
            Message = dto.Message.Trim(),
            Language = string.IsNullOrWhiteSpace(dto.Language) ? "ar" : dto.Language.Trim().ToLowerInvariant(),
            Status = dto.Status,
            StartDate = dto.StartDate,
            EndDate = dto.EndDate,
            RecurrenceType = dto.RecurrenceType,
            ScheduledTime = dto.ScheduledTime,
            IntervalValue = dto.IntervalValue > 0 ? dto.IntervalValue : 1,
            DaysOfWeek = dto.DaysOfWeek,
            DurationDays = dto.DurationDays,
            MaxOccurrences = dto.MaxOccurrences,
            TimeZoneId = string.IsNullOrWhiteSpace(dto.TimeZoneId) ? "Local" : dto.TimeZoneId,
            Priority = dto.Priority,
            TargetAudience = dto.TargetAudience,
            TargetRole = dto.TargetRole,
            DisplayType = dto.DisplayType,
            ActionRoute = string.IsNullOrWhiteSpace(dto.ActionRoute) ? "/" : dto.ActionRoute.Trim(),
            ImageUrl = dto.ImageUrl?.Trim(),
            DeliveryChannel = string.IsNullOrWhiteSpace(dto.DeliveryChannel) ? "Both" : dto.DeliveryChannel,
            MessageSelectionMode = string.IsNullOrWhiteSpace(dto.MessageSelectionMode) ? "Single" : dto.MessageSelectionMode,
            BankCategory = dto.BankCategory?.Trim(),
            LastSequentialIndex = dto.LastSequentialIndex,
            CampaignType = string.IsNullOrWhiteSpace(dto.CampaignType) ? "Regular" : dto.CampaignType.Trim(),
            OverridesRegularReminders = dto.OverridesRegularReminders,
            OccasionName = dto.OccasionName?.Trim(),
            MessageBankJson = dto.MessageBankItems != null && dto.MessageBankItems.Any()
                ? System.Text.Json.JsonSerializer.Serialize(dto.MessageBankItems)
                : dto.MessageBankJson,
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        // If EndDate is not set but DurationDays is set, compute EndDate
        if (!entity.EndDate.HasValue && entity.DurationDays.HasValue && entity.DurationDays.Value > 0)
        {
            entity.EndDate = entity.StartDate.AddDays(entity.DurationDays.Value);
        }

        _context.ReminderCampaigns.Add(entity);
        await _context.SaveChangesAsync();

        return MapToDto(entity);
    }

    public async Task<ReminderCampaignDto?> UpdateReminderAsync(int id, UpdateReminderCampaignDto dto, string? userId)
    {
        var entity = await _context.ReminderCampaigns.FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
        if (entity == null) return null;

        entity.Title = dto.Title.Trim();
        entity.InternalDescription = dto.InternalDescription?.Trim();
        entity.Message = dto.Message.Trim();
        entity.Language = string.IsNullOrWhiteSpace(dto.Language) ? "ar" : dto.Language.Trim().ToLowerInvariant();
        entity.Status = dto.Status;
        entity.StartDate = dto.StartDate;
        entity.EndDate = dto.EndDate;
        entity.RecurrenceType = dto.RecurrenceType;
        entity.ScheduledTime = dto.ScheduledTime;
        entity.IntervalValue = dto.IntervalValue > 0 ? dto.IntervalValue : 1;
        entity.DaysOfWeek = dto.DaysOfWeek;
        entity.DurationDays = dto.DurationDays;
        entity.MaxOccurrences = dto.MaxOccurrences;
        entity.TimeZoneId = string.IsNullOrWhiteSpace(dto.TimeZoneId) ? "Local" : dto.TimeZoneId;
        entity.Priority = dto.Priority;
        entity.TargetAudience = dto.TargetAudience;
        entity.TargetRole = dto.TargetRole;
        entity.DisplayType = dto.DisplayType;
        entity.ActionRoute = string.IsNullOrWhiteSpace(dto.ActionRoute) ? "/" : dto.ActionRoute.Trim();
        entity.ImageUrl = dto.ImageUrl?.Trim();
        entity.DeliveryChannel = string.IsNullOrWhiteSpace(dto.DeliveryChannel) ? "Both" : dto.DeliveryChannel;
        entity.MessageSelectionMode = string.IsNullOrWhiteSpace(dto.MessageSelectionMode) ? "Single" : dto.MessageSelectionMode;
        entity.BankCategory = dto.BankCategory?.Trim();
        entity.LastSequentialIndex = dto.LastSequentialIndex;
        entity.CampaignType = string.IsNullOrWhiteSpace(dto.CampaignType) ? "Regular" : dto.CampaignType.Trim();
        entity.OverridesRegularReminders = dto.OverridesRegularReminders;
        entity.OccasionName = dto.OccasionName?.Trim();
        if (dto.MessageBankItems != null && dto.MessageBankItems.Any())
        {
            entity.MessageBankJson = System.Text.Json.JsonSerializer.Serialize(dto.MessageBankItems);
        }
        else if (dto.MessageBankJson != null)
        {
            entity.MessageBankJson = dto.MessageBankJson;
        }
        entity.IsActive = dto.IsActive;
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = userId;

        if (!entity.EndDate.HasValue && entity.DurationDays.HasValue && entity.DurationDays.Value > 0)
        {
            entity.EndDate = entity.StartDate.AddDays(entity.DurationDays.Value);
        }

        await _context.SaveChangesAsync();
        return MapToDto(entity);
    }

    public async Task<bool> UpdateReminderStatusAsync(int id, ReminderStatus status, string? userId)
    {
        var entity = await _context.ReminderCampaigns.FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
        if (entity == null) return false;

        entity.Status = status;
        entity.IsActive = (status == ReminderStatus.Active);
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = userId;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteReminderAsync(int id, string? userId)
    {
        var entity = await _context.ReminderCampaigns.FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);
        if (entity == null) return false;

        entity.IsDeleted = true;
        entity.DeletedAt = DateTime.UtcNow;
        entity.DeletedBy = userId;
        entity.IsActive = false;
        entity.Status = ReminderStatus.Expired;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<ReminderCampaignDto?> DuplicateReminderAsync(int id, string? userId)
    {
        var original = await _context.ReminderCampaigns.AsNoTracking()
            .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

        if (original == null) return null;

        var duplicate = new ReminderCampaign
        {
            Title = $"{original.Title} (نسخة)",
            InternalDescription = original.InternalDescription,
            Message = original.Message,
            Language = original.Language,
            Status = ReminderStatus.Draft,
            StartDate = DateTime.UtcNow,
            EndDate = original.DurationDays.HasValue ? DateTime.UtcNow.AddDays(original.DurationDays.Value) : original.EndDate,
            RecurrenceType = original.RecurrenceType,
            ScheduledTime = original.ScheduledTime,
            IntervalValue = original.IntervalValue,
            DaysOfWeek = original.DaysOfWeek,
            DurationDays = original.DurationDays,
            MaxOccurrences = original.MaxOccurrences,
            TimeZoneId = original.TimeZoneId,
            Priority = original.Priority,
            TargetAudience = original.TargetAudience,
            TargetRole = original.TargetRole,
            DisplayType = original.DisplayType,
            BankCategory = original.BankCategory,
            LastSequentialIndex = 0,
            CampaignType = original.CampaignType,
            OverridesRegularReminders = original.OverridesRegularReminders,
            OccasionName = original.OccasionName,
            ActionRoute = original.ActionRoute,
            ImageUrl = original.ImageUrl,
            DeliveryChannel = original.DeliveryChannel ?? "Both",
            MessageSelectionMode = original.MessageSelectionMode ?? "Single",
            MessageBankJson = original.MessageBankJson,
            IsActive = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        _context.ReminderCampaigns.Add(duplicate);
        await _context.SaveChangesAsync();

        return MapToDto(duplicate);
    }

    public async Task<List<ActiveClientReminderDto>> GetActiveRemindersForClientAsync(string? userRole, bool isTester, string? language)
    {
        var nowUtc = DateTime.UtcNow;
        var lang = string.IsNullOrWhiteSpace(language) ? "ar" : language.Trim().ToLowerInvariant();

        var query = _context.ReminderCampaigns.AsNoTracking()
            .Where(r => !r.IsDeleted && r.IsActive && r.Status == ReminderStatus.Active);

        var list = await query.ToListAsync();

        var eligible = list.Where(r =>
        {
            // Date checks
            if (nowUtc < r.StartDate) return false;
            if (r.EndDate.HasValue && nowUtc > r.EndDate.Value) return false;
            if (!r.EndDate.HasValue && r.DurationDays.HasValue && nowUtc > r.StartDate.AddDays(r.DurationDays.Value)) return false;

            // Language check
            if (!string.Equals(r.Language, "all", StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(r.Language, lang, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            // Audience check
            switch (r.TargetAudience)
            {
                case ReminderTargetAudience.AllUsers:
                    return true;
                case ReminderTargetAudience.Testers:
                    return isTester; // Eligible if device is in testing mode
                case ReminderTargetAudience.Clients:
                    return string.IsNullOrEmpty(userRole) || string.Equals(userRole, "User", StringComparison.OrdinalIgnoreCase) || string.Equals(userRole, "Client", StringComparison.OrdinalIgnoreCase);
                case ReminderTargetAudience.Providers:
                    return string.Equals(userRole, "Provider", StringComparison.OrdinalIgnoreCase);
                case ReminderTargetAudience.SpecificRole:
                    return !string.IsNullOrEmpty(r.TargetRole) && string.Equals(userRole, r.TargetRole, StringComparison.OrdinalIgnoreCase);
                default:
                    return true;
            }
        }).ToList();

        return eligible.Select(r => new ActiveClientReminderDto
        {
            Id = r.Id,
            Title = r.Title,
            Message = r.Message,
            Language = r.Language,
            RecurrenceType = r.RecurrenceType,
            ScheduledTime = r.ScheduledTime,
            IntervalValue = r.IntervalValue,
            DaysOfWeek = r.DaysOfWeek,
            StartDate = r.StartDate,
            EndDate = r.EndDate,
            DurationDays = r.DurationDays,
            DisplayType = r.DisplayType,
            ActionRoute = r.ActionRoute,
            ImageUrl = r.ImageUrl,
            Priority = r.Priority,
            UpdatedAt = r.UpdatedAt ?? r.CreatedAt
        }).ToList();
    }

    public async Task<ReminderCampaignDto> EnsureDefaultTesterCampaignExistsAsync(string? userId)
    {
        var existing = await _context.ReminderCampaigns.FirstOrDefaultAsync(r => 
            !r.IsDeleted && (r.Title == "تذكير المختبرين بالاستخدام اليومي" || 
            (r.InternalDescription != null && r.InternalDescription.Contains("الاختبار المغلق"))));

        if (existing != null)
        {
            // If already exists, return it
            return MapToDto(existing);
        }

        var defaultTesterCampaign = new ReminderCampaign
        {
            Title = "تذكير المختبرين بالاستخدام اليومي",
            InternalDescription = "دعم الاختبار المغلق لتطبيق خدماوى على Google Play",
            Message = "مرحبًا بك في اختبار خدماوى! افتح التطبيق اليوم، وجرّب إحدى وظائفه، وساعدنا في اكتشاف أي مشكلة أو اقتراح تحسين قبل الإطلاق الرسمي. شكرًا لمشاركتك.",
            Language = "ar",
            Status = ReminderStatus.Active,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(14),
            RecurrenceType = ReminderRecurrenceType.Daily,
            ScheduledTime = new TimeSpan(20, 0, 0), // 8:00 PM
            IntervalValue = 1,
            DurationDays = 14,
            TimeZoneId = "Local",
            Priority = ReminderPriority.Normal,
            TargetAudience = ReminderTargetAudience.Testers,
            DisplayType = ReminderDisplayType.NotificationBar,
            ActionRoute = "/",
            DeliveryChannel = "Both",
            MessageSelectionMode = "Single",
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId ?? "System"
        };

        _context.ReminderCampaigns.Add(defaultTesterCampaign);
        await _context.SaveChangesAsync();

        return MapToDto(defaultTesterCampaign);
    }

    public async Task<(bool Success, string Message)> TriggerReminderNowAsync(int id, string? userId)
    {
        var campaign = await _context.ReminderCampaigns.FindAsync(id);
        if (campaign == null || campaign.IsDeleted)
        {
            return (false, "التذكير غير موجود");
        }

        string? roleFilter = string.IsNullOrWhiteSpace(campaign.TargetRole) ? null : campaign.TargetRole;
        string actionRoute = string.IsNullOrWhiteSpace(campaign.ActionRoute) ? "/" : campaign.ActionRoute;
        string titleToSend = campaign.Title;
        string messageToSend = campaign.Message;
        string? imageToSend = campaign.ImageUrl;

        // Check Master Reminder Bank first for RandomBank and SequentialBank
        if (campaign.MessageSelectionMode == "RandomBank" || campaign.MessageSelectionMode == "SequentialBank")
        {
            try
            {
                var bankQuery = _context.ReminderBankItems.Where(b => b.IsActive && !b.IsDeleted);
                if (!string.IsNullOrWhiteSpace(campaign.BankCategory) && campaign.BankCategory != "All" && campaign.BankCategory != "الكل")
                {
                    bankQuery = bankQuery.Where(b => b.Category == campaign.BankCategory);
                }

                var masterItems = await bankQuery.OrderBy(b => b.OrderIndex).ThenBy(b => b.Id).ToListAsync();
                if (masterItems.Count > 0)
                {
                    ReminderBankItem? chosen = null;
                    if (campaign.MessageSelectionMode == "RandomBank")
                    {
                        var rand = new Random();
                        chosen = masterItems[rand.Next(masterItems.Count)];
                    }
                    else // SequentialBank
                    {
                        chosen = masterItems.FirstOrDefault(b => b.OrderIndex > campaign.LastSequentialIndex) ?? masterItems[0];
                        campaign.LastSequentialIndex = chosen.OrderIndex;
                    }

                    if (chosen != null)
                    {
                        if (!string.IsNullOrWhiteSpace(chosen.Title)) titleToSend = chosen.Title;
                        if (!string.IsNullOrWhiteSpace(chosen.Message)) messageToSend = chosen.Message;
                        if (!string.IsNullOrWhiteSpace(chosen.ImageUrl)) imageToSend = chosen.ImageUrl;
                        if (!string.IsNullOrWhiteSpace(chosen.ActionRoute)) actionRoute = chosen.ActionRoute;

                        chosen.UsageCount++;
                        chosen.LastUsedAt = DateTime.UtcNow;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ReminderService] Error selecting master bank item: {ex.Message}");
            }
        }
        else if ((campaign.MessageSelectionMode == "ScheduledBank" || campaign.MessageSelectionMode == "Scheduled" ||
                  campaign.MessageSelectionMode == "Random") && !string.IsNullOrWhiteSpace(campaign.MessageBankJson))
        {
            try
            {
                var bankItems = System.Text.Json.JsonSerializer.Deserialize<List<ReminderMessageBankItemDto>>(campaign.MessageBankJson);
                if (bankItems != null && bankItems.Count > 0)
                {
                    ReminderMessageBankItemDto? chosenItem = null;
                    if (campaign.MessageSelectionMode.StartsWith("Random", StringComparison.OrdinalIgnoreCase))
                    {
                        var rand = new Random();
                        chosenItem = bankItems[rand.Next(bankItems.Count)];
                    }
                    else
                    {
                        chosenItem = bankItems.FirstOrDefault(b => !b.IsSent) ?? bankItems.FirstOrDefault();
                    }

                    if (chosenItem != null)
                    {
                        if (!string.IsNullOrWhiteSpace(chosenItem.Title)) titleToSend = chosenItem.Title;
                        if (!string.IsNullOrWhiteSpace(chosenItem.Message)) messageToSend = chosenItem.Message;
                        if (!string.IsNullOrWhiteSpace(chosenItem.ImageUrl)) imageToSend = chosenItem.ImageUrl;
                        if (!string.IsNullOrWhiteSpace(chosenItem.ActionRoute)) actionRoute = chosenItem.ActionRoute;

                        chosenItem.IsSent = true;
                        chosenItem.SentAt = DateTime.UtcNow;
                        chosenItem.SendCount++;
                        campaign.MessageBankJson = System.Text.Json.JsonSerializer.Serialize(bankItems);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ReminderService] Error selecting bank item: {ex.Message}");
            }
        }

        try
        {
            await _notificationService.SendBroadcastAsync(
                title: titleToSend,
                message: messageToSend,
                type: "Reminder",
                link: actionRoute,
                role: roleFilter,
                governorateId: null,
                cityId: null,
                mainCategoryId: null,
                imageUrl: imageToSend,
                deliveryChannel: campaign.DeliveryChannel ?? "Both"
            );

            campaign.LastTriggeredAt = DateTime.UtcNow;
            campaign.ExecutionCount++;
            campaign.UpdatedAt = DateTime.UtcNow;
            campaign.UpdatedBy = userId;
            await _context.SaveChangesAsync();

            return (true, $"تم إرسال التذكير '{titleToSend}' بنجاح عبر قناة ({campaign.DeliveryChannel})!");
        }
        catch (Exception ex)
        {
            return (false, $"حدث خطأ أثناء إرسال التذكير: {ex.Message}");
        }
    }

    private static ReminderCampaignDto MapToDto(ReminderCampaign r)
    {
        List<ReminderMessageBankItemDto> parsedBank = new();
        if (!string.IsNullOrWhiteSpace(r.MessageBankJson))
        {
            try 
            { 
                parsedBank = System.Text.Json.JsonSerializer.Deserialize<List<ReminderMessageBankItemDto>>(r.MessageBankJson) ?? new(); 
            } 
            catch {}
        }

        return new ReminderCampaignDto
        {
            Id = r.Id,
            Title = r.Title,
            InternalDescription = r.InternalDescription,
            Message = r.Message,
            Language = r.Language,
            Status = r.Status,
            StartDate = r.StartDate,
            EndDate = r.EndDate,
            RecurrenceType = r.RecurrenceType,
            ScheduledTime = r.ScheduledTime,
            IntervalValue = r.IntervalValue,
            DaysOfWeek = r.DaysOfWeek,
            DurationDays = r.DurationDays,
            MaxOccurrences = r.MaxOccurrences,
            TimeZoneId = r.TimeZoneId,
            Priority = r.Priority,
            TargetAudience = r.TargetAudience,
            TargetRole = r.TargetRole,
            DisplayType = r.DisplayType,
            ActionRoute = r.ActionRoute,
            ImageUrl = r.ImageUrl,
            DeliveryChannel = r.DeliveryChannel ?? "Both",
            MessageSelectionMode = r.MessageSelectionMode ?? "Single",
            MessageBankJson = r.MessageBankJson,
            BankCategory = r.BankCategory,
            LastSequentialIndex = r.LastSequentialIndex,
            CampaignType = r.CampaignType ?? "Regular",
            OverridesRegularReminders = r.OverridesRegularReminders,
            OccasionName = r.OccasionName,
            MessageBankItems = parsedBank,
            IsActive = r.IsActive,
            ExecutionCount = r.ExecutionCount,
            LastTriggeredAt = r.LastTriggeredAt,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt
        };
    }

    // Reminder Bank Management
    public async Task<List<ReminderBankItemDto>> GetBankItemsAsync(string? category = null, bool? isActive = null)
    {
        var query = _context.ReminderBankItems.AsNoTracking().Where(b => !b.IsDeleted);

        if (!string.IsNullOrWhiteSpace(category) && category != "All" && category != "الكل")
        {
            var cat = category.Trim();
            query = query.Where(b => b.Category == cat);
        }

        if (isActive.HasValue)
        {
            query = query.Where(b => b.IsActive == isActive.Value);
        }

        var items = await query.OrderBy(b => b.OrderIndex).ThenBy(b => b.Id).ToListAsync();
        return items.Select(MapBankItemToDto).ToList();
    }

    public async Task<ReminderBankItemDto?> GetBankItemByIdAsync(int id)
    {
        var item = await _context.ReminderBankItems.AsNoTracking()
            .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);
        return item != null ? MapBankItemToDto(item) : null;
    }

    public async Task<ReminderBankItemDto> CreateBankItemAsync(CreateReminderBankItemDto dto, string? userId)
    {
        var item = new ReminderBankItem
        {
            Title = dto.Title.Trim(),
            Message = dto.Message.Trim(),
            Category = string.IsNullOrWhiteSpace(dto.Category) ? "General" : dto.Category.Trim(),
            OrderIndex = dto.OrderIndex > 0 ? dto.OrderIndex : 1,
            SuggestedTime = dto.SuggestedTime,
            ImageUrl = dto.ImageUrl?.Trim(),
            ActionRoute = string.IsNullOrWhiteSpace(dto.ActionRoute) ? "/" : dto.ActionRoute.Trim(),
            IsActive = dto.IsActive,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        _context.ReminderBankItems.Add(item);
        await _context.SaveChangesAsync();
        return MapBankItemToDto(item);
    }

    public async Task<ReminderBankItemDto?> UpdateBankItemAsync(int id, UpdateReminderBankItemDto dto, string? userId)
    {
        var item = await _context.ReminderBankItems.FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);
        if (item == null) return null;

        item.Title = dto.Title.Trim();
        item.Message = dto.Message.Trim();
        item.Category = string.IsNullOrWhiteSpace(dto.Category) ? "General" : dto.Category.Trim();
        item.OrderIndex = dto.OrderIndex > 0 ? dto.OrderIndex : 1;
        item.SuggestedTime = dto.SuggestedTime;
        item.ImageUrl = dto.ImageUrl?.Trim();
        item.ActionRoute = string.IsNullOrWhiteSpace(dto.ActionRoute) ? "/" : dto.ActionRoute.Trim();
        item.IsActive = dto.IsActive;
        item.UpdatedAt = DateTime.UtcNow;
        item.UpdatedBy = userId;

        await _context.SaveChangesAsync();
        return MapBankItemToDto(item);
    }

    public async Task<bool> DeleteBankItemAsync(int id, string? userId)
    {
        var item = await _context.ReminderBankItems.FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);
        if (item == null) return false;

        item.IsDeleted = true;
        item.DeletedAt = DateTime.UtcNow;
        item.DeletedBy = userId;
        item.IsActive = false;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<string>> GetBankCategoriesAsync()
    {
        var categories = await _context.ReminderBankItems
            .AsNoTracking()
            .Where(b => !b.IsDeleted && !string.IsNullOrWhiteSpace(b.Category))
            .Select(b => b.Category.Trim())
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        return categories;
    }

    public async Task<ReminderBankItemDto?> ResolveNextBankItemAsync(int campaignId)
    {
        var campaign = await _context.ReminderCampaigns.FirstOrDefaultAsync(c => c.Id == campaignId && !c.IsDeleted);
        if (campaign == null) return null;

        var query = _context.ReminderBankItems.Where(b => b.IsActive && !b.IsDeleted);
        if (!string.IsNullOrWhiteSpace(campaign.BankCategory) && campaign.BankCategory != "All" && campaign.BankCategory != "الكل")
        {
            query = query.Where(b => b.Category == campaign.BankCategory);
        }

        var items = await query.OrderBy(b => b.OrderIndex).ThenBy(b => b.Id).ToListAsync();
        if (items.Count == 0) return null;

        ReminderBankItem chosen;
        if (campaign.MessageSelectionMode == "RandomBank")
        {
            var rand = new Random();
            chosen = items[rand.Next(items.Count)];
        }
        else // SequentialBank or default
        {
            chosen = items.FirstOrDefault(b => b.OrderIndex > campaign.LastSequentialIndex) ?? items[0];
            campaign.LastSequentialIndex = chosen.OrderIndex;
            await _context.SaveChangesAsync();
        }

        return MapBankItemToDto(chosen);
    }

    private static ReminderBankItemDto MapBankItemToDto(ReminderBankItem item)
    {
        return new ReminderBankItemDto
        {
            Id = item.Id,
            Title = item.Title,
            Message = item.Message,
            Category = item.Category,
            OrderIndex = item.OrderIndex,
            SuggestedTime = item.SuggestedTime,
            ImageUrl = item.ImageUrl,
            ActionRoute = item.ActionRoute,
            IsActive = item.IsActive,
            UsageCount = item.UsageCount,
            LastUsedAt = item.LastUsedAt,
            CreatedAt = item.CreatedAt
        };
    }
}
