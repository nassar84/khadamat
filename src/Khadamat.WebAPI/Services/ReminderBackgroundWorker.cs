using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Khadamat.Application.DTOs;
using Khadamat.Application.Interfaces;
using Khadamat.Domain.Entities;
using Khadamat.Domain.Enums;
using Khadamat.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Khadamat.WebAPI.Services;

/// <summary>
/// Background worker that checks every minute for active reminders scheduled for the current time
/// and triggers broadcasts (In-App notifications + SignalR + FCM Push Notifications) via the server.
/// </summary>
public class ReminderBackgroundWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ReminderBackgroundWorker> _logger;
    private static readonly TimeZoneInfo LocalAppTimeZone = GetAppTimeZone();

    public ReminderBackgroundWorker(IServiceScopeFactory scopeFactory, ILogger<ReminderBackgroundWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[ReminderWorker] Reminder Background Worker started. TimeZone: {Tz}", LocalAppTimeZone.DisplayName);

        // Initial brief delay to let application finish startup
        await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessDueRemindersAsync(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogError(ex, "[ReminderWorker] Error occurred while processing scheduled reminders.");
            }

            // Sleep until the next minute
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("[ReminderWorker] Reminder Background Worker stopped.");
    }

    private async Task ProcessDueRemindersAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<KhadamatDbContext>();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var nowUtc = DateTime.UtcNow;
        var nowLocal = TimeZoneInfo.ConvertTimeFromUtc(nowUtc, LocalAppTimeZone);
        var currentHour = nowLocal.Hour;
        var currentMinute = nowLocal.Minute;
        var currentDayOfWeek = nowLocal.DayOfWeek.ToString();

        // 1. Fetch currently active campaigns (lightweight query)
        var activeCampaigns = await db.ReminderCampaigns
            .Where(r => r.IsActive && r.Status == ReminderStatus.Active && !r.IsDeleted)
            .ToListAsync(cancellationToken);

        if (activeCampaigns.Count == 0) return;

        // Check if there is an active special occasion campaign that overrides regular daily reminders today
        var isOccasionActiveToday = activeCampaigns.Any(c => 
            c.OverridesRegularReminders && 
            c.IsCurrentlyActive(nowUtc) && 
            IsRecurrenceDue(c, nowLocal, currentDayOfWeek));

        foreach (var campaign in activeCampaigns)
        {
            // Verify date window
            if (!campaign.IsCurrentlyActive(nowUtc))
                continue;

            // If an Occasion override is active today, suppress regular routine campaigns so occasion messages show instead
            if (isOccasionActiveToday && !campaign.OverridesRegularReminders && campaign.CampaignType == "Regular")
            {
                continue;
            }

            // Check if scheduled time is reached within a 2-minute grace window
            var scheduledToday = nowLocal.Date + campaign.ScheduledTime;
            var diffMinutes = (nowLocal - scheduledToday).TotalMinutes;
            if (diffMinutes < 0 || diffMinutes >= 2.0)
                continue;

            // Prevent duplicate triggers within the same 15-minute window
            if (campaign.LastTriggeredAt.HasValue && (nowUtc - campaign.LastTriggeredAt.Value).TotalMinutes < 15)
                continue;

            // Check recurrence rules
            if (!IsRecurrenceDue(campaign, nowLocal, currentDayOfWeek))
                continue;

            _logger.LogInformation("[ReminderWorker] Triggering reminder '{Title}' (ID: {Id}) at {Time:HH:mm}", 
                campaign.Title, campaign.Id, nowLocal);

            // 2. Dispatch broadcast notification (persists to DB + SignalR + FCM Push depending on DeliveryChannel)
            string? roleFilter = string.IsNullOrWhiteSpace(campaign.TargetRole) ? null : campaign.TargetRole;
            string actionRoute = string.IsNullOrWhiteSpace(campaign.ActionRoute) ? "/" : campaign.ActionRoute;
            string titleToSend = campaign.Title;
            string messageToSend = campaign.Message;
            string? imageToSend = campaign.ImageUrl;

            // Handle Message Bank: check master ReminderBankItems first for RandomBank / SequentialBank
            if (campaign.MessageSelectionMode == "RandomBank" || campaign.MessageSelectionMode == "SequentialBank")
            {
                try
                {
                    var bankQuery = db.ReminderBankItems.Where(b => b.IsActive && !b.IsDeleted);
                    if (!string.IsNullOrWhiteSpace(campaign.BankCategory) && campaign.BankCategory != "All" && campaign.BankCategory != "الكل")
                    {
                        bankQuery = bankQuery.Where(b => b.Category == campaign.BankCategory);
                    }

                    var masterItems = await bankQuery.OrderBy(b => b.OrderIndex).ThenBy(b => b.Id).ToListAsync(cancellationToken);
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
                            chosen.LastUsedAt = nowUtc;
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[ReminderWorker] Error selecting from Master ReminderBank for campaign {Id}", campaign.Id);
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
                            // Scheduled mode: prioritize items scheduled for today or un-sent items
                            chosenItem = bankItems.FirstOrDefault(b => !b.IsSent && b.ScheduledDateTime.HasValue && b.ScheduledDateTime.Value.Date == nowLocal.Date)
                                         ?? bankItems.FirstOrDefault(b => !b.IsSent)
                                         ?? bankItems.FirstOrDefault();
                        }

                        if (chosenItem != null)
                        {
                            if (!string.IsNullOrWhiteSpace(chosenItem.Title)) titleToSend = chosenItem.Title;
                            if (!string.IsNullOrWhiteSpace(chosenItem.Message)) messageToSend = chosenItem.Message;
                            if (!string.IsNullOrWhiteSpace(chosenItem.ImageUrl)) imageToSend = chosenItem.ImageUrl;
                            if (!string.IsNullOrWhiteSpace(chosenItem.ActionRoute)) actionRoute = chosenItem.ActionRoute;

                            chosenItem.IsSent = true;
                            chosenItem.SentAt = nowUtc;
                            chosenItem.SendCount++;
                            campaign.MessageBankJson = System.Text.Json.JsonSerializer.Serialize(bankItems);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[ReminderWorker] Error selecting from MessageBank for campaign {Id}", campaign.Id);
                }
            }

            try
            {
                await notificationService.SendBroadcastAsync(
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

                // 3. Update campaign execution tracking
                campaign.LastTriggeredAt = nowUtc;
                campaign.ExecutionCount++;

                if (campaign.RecurrenceType == ReminderRecurrenceType.Once ||
                    (campaign.MaxOccurrences.HasValue && campaign.ExecutionCount >= campaign.MaxOccurrences.Value))
                {
                    campaign.Status = ReminderStatus.Expired;
                    _logger.LogInformation("[ReminderWorker] Reminder '{Title}' (ID: {Id}) marked as Expired.", campaign.Title, campaign.Id);
                }

                await db.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ReminderWorker] Failed to trigger reminder '{Title}' (ID: {Id})", campaign.Title, campaign.Id);
            }
        }
    }

    private static bool IsRecurrenceDue(ReminderCampaign campaign, DateTime nowLocal, string currentDayOfWeek)
    {
        switch (campaign.RecurrenceType)
        {
            case ReminderRecurrenceType.Daily:
                return true;

            case ReminderRecurrenceType.Weekly:
                if (!string.IsNullOrWhiteSpace(campaign.DaysOfWeek))
                {
                    var days = campaign.DaysOfWeek.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                    return days.Any(d => string.Equals(d, currentDayOfWeek, StringComparison.OrdinalIgnoreCase));
                }
                return true;

            case ReminderRecurrenceType.Once:
                return campaign.ExecutionCount == 0;

            case ReminderRecurrenceType.IntervalDays:
                if (campaign.LastTriggeredAt.HasValue && campaign.IntervalValue > 1)
                {
                    var daysSinceLast = (nowLocal.Date - campaign.LastTriggeredAt.Value.Date).TotalDays;
                    return daysSinceLast >= campaign.IntervalValue;
                }
                return true;

            default:
                return true;
        }
    }

    private static TimeZoneInfo GetAppTimeZone()
    {
        // Egypt / Cairo timezone (UTC+2 or UTC+3 daylight saving)
        string[] candidateIds = { "Egypt Standard Time", "Africa/Cairo", "Arabic Standard Time" };
        foreach (var id in candidateIds)
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch { }
        }
        return TimeZoneInfo.Local;
    }
}
