using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        foreach (var campaign in activeCampaigns)
        {
            // Verify date window
            if (!campaign.IsCurrentlyActive(nowUtc))
                continue;

            // Check if scheduled time matches current hour & minute
            if (campaign.ScheduledTime.Hours != currentHour || campaign.ScheduledTime.Minutes != currentMinute)
                continue;

            // Prevent duplicate triggers within the same 5-minute window
            if (campaign.LastTriggeredAt.HasValue && (nowUtc - campaign.LastTriggeredAt.Value).TotalMinutes < 5)
                continue;

            // Check recurrence rules
            if (!IsRecurrenceDue(campaign, nowLocal, currentDayOfWeek))
                continue;

            _logger.LogInformation("[ReminderWorker] Triggering reminder '{Title}' (ID: {Id}) at {Time:HH:mm}", 
                campaign.Title, campaign.Id, nowLocal);

            // 2. Dispatch broadcast notification (persists to DB + SignalR + FCM Push)
            string? roleFilter = string.IsNullOrWhiteSpace(campaign.TargetRole) ? null : campaign.TargetRole;
            string actionRoute = string.IsNullOrWhiteSpace(campaign.ActionRoute) ? "/" : campaign.ActionRoute;

            try
            {
                await notificationService.SendBroadcastAsync(
                    title: campaign.Title,
                    message: campaign.Message,
                    type: "Reminder",
                    link: actionRoute,
                    role: roleFilter,
                    governorateId: null,
                    cityId: null,
                    mainCategoryId: null
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
