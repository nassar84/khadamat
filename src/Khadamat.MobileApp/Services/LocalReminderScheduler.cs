using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Khadamat.Application.Common.Models;
using Khadamat.Application.DTOs;
using Khadamat.Domain.Enums;
using Microsoft.Maui.Networking;
using Microsoft.Maui.Storage;

namespace Khadamat.MobileApp.Services;

public class LocalReminderScheduler : ILocalReminderScheduler
{
    private readonly HttpClient _httpClient;
    private readonly IPlatformReminderScheduler _platformScheduler;
    private const string PrefKeyEnabled = "RemindersEnabled";
    private const string PrefKeyCachedReminders = "CachedActiveReminders";
    private const string PrefKeyLastSync = "RemindersLastSyncTime";
    private const string PrefKeySeenInApp = "SeenInAppReminder_";

    public event Action<ActiveClientReminderDto>? OnInAppReminderDue;

    public LocalReminderScheduler(HttpClient httpClient, IPlatformReminderScheduler platformScheduler)
    {
        _httpClient = httpClient;
        _platformScheduler = platformScheduler;
    }

    public async Task InitializeAsync()
    {
        try
        {
            _platformScheduler.Initialize();

            // Request permission quietly if not requested yet
            if (AreRemindersEnabled())
            {
                await RequestNotificationPermissionAsync();
            }

            // Sync from server or local cache
            await SyncRemindersAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LocalReminderScheduler] Initialize error: {ex.Message}");
        }
    }

    public bool AreRemindersEnabled()
    {
        return Preferences.Default.Get(PrefKeyEnabled, true);
    }

    public Task SetRemindersEnabledAsync(bool enabled)
    {
        Preferences.Default.Set(PrefKeyEnabled, enabled);
        if (!enabled)
        {
            _platformScheduler.CancelAllNotifications();
        }
        else
        {
            // Reschedule cached reminders
            RescheduleLocalNotificationsFromCache();
        }
        return Task.CompletedTask;
    }

    public async Task<bool> RequestNotificationPermissionAsync()
    {
        return await _platformScheduler.RequestPermissionAsync();
    }

    public async Task SyncRemindersAsync(bool force = false)
    {
        if (!AreRemindersEnabled()) return;

        // Check network
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            // Offline fallback: load from local cache
            Console.WriteLine("[LocalReminderScheduler] Offline: using cached reminders");
            RescheduleLocalNotificationsFromCache();
            return;
        }

        try
        {
            var apiBase = Preferences.Default.Get("ApiBaseUrl", "https://khadamawy.eis-dev.com/").TrimEnd('/') + "/";
            var url = $"{apiBase}v1/reminders/active?isTester=true&lang=ar";

            var response = await _httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<List<ActiveClientReminderDto>>>();
                if (apiResponse?.Success == true && apiResponse.Data != null)
                {
                    var reminders = apiResponse.Data;
                    var json = JsonSerializer.Serialize(reminders);
                    Preferences.Default.Set(PrefKeyCachedReminders, json);
                    Preferences.Default.Set(PrefKeyLastSync, DateTime.UtcNow.ToString("O"));

                    // Cancel obsolete notifications and schedule new/updated ones
                    SyncWithPlatform(reminders);
                }
            }
            else
            {
                Console.WriteLine($"[LocalReminderScheduler] Server returned {response.StatusCode}, falling back to cache");
                RescheduleLocalNotificationsFromCache();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[LocalReminderScheduler] Sync failed ({ex.Message}), falling back to cache");
            RescheduleLocalNotificationsFromCache();
        }
    }

    private void SyncWithPlatform(List<ActiveClientReminderDto> reminders)
    {
        if (!AreRemindersEnabled()) return;

        // Reset and schedule fresh to ensure no orphaned alarms exist
        _platformScheduler.CancelAllNotifications();

        foreach (var rem in reminders.Where(r => r.DisplayType == ReminderDisplayType.NotificationBar))
        {
            _platformScheduler.ScheduleNotification(rem);
        }

        // Check in-app reminders
        _ = CheckAndTriggerInAppRemindersAsync(reminders);
    }

    private void RescheduleLocalNotificationsFromCache()
    {
        var cached = GetCachedReminders();
        if (cached != null && cached.Count > 0)
        {
            SyncWithPlatform(cached);
        }
    }

    public static List<ActiveClientReminderDto> GetCachedReminders()
    {
        try
        {
            var json = Preferences.Default.Get(PrefKeyCachedReminders, "");
            if (string.IsNullOrEmpty(json)) return new List<ActiveClientReminderDto>();

            return JsonSerializer.Deserialize<List<ActiveClientReminderDto>>(json) ?? new List<ActiveClientReminderDto>();
        }
        catch
        {
            return new List<ActiveClientReminderDto>();
        }
    }

    public Task<List<ActiveClientReminderDto>> GetActiveInAppRemindersAsync()
    {
        var cached = GetCachedReminders();
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");

        var inAppList = cached.Where(r => 
            (r.DisplayType == ReminderDisplayType.InAppBanner || 
             r.DisplayType == ReminderDisplayType.InAppDialog || 
             r.DisplayType == ReminderDisplayType.TaskReminder) &&
            Preferences.Default.Get(PrefKeySeenInApp + r.Id, "") != today
        ).ToList();

        return Task.FromResult(inAppList);
    }

    public void MarkInAppReminderSeen(int reminderId)
    {
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        Preferences.Default.Set(PrefKeySeenInApp + reminderId, today);
    }

    private async Task CheckAndTriggerInAppRemindersAsync(List<ActiveClientReminderDto>? list = null)
    {
        try
        {
            var inApp = list != null 
                ? list.Where(r => r.DisplayType != ReminderDisplayType.NotificationBar).ToList()
                : await GetActiveInAppRemindersAsync();

            var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
            foreach (var rem in inApp)
            {
                if (Preferences.Default.Get(PrefKeySeenInApp + rem.Id, "") != today)
                {
                    OnInAppReminderDue?.Invoke(rem);
                    break; // Trigger at most one per session so user is not overwhelmed
                }
            }
        }
        catch { }
    }
}
