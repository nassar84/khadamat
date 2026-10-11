using System;
using System.Threading.Tasks;
using Khadamat.Application.DTOs;

namespace Khadamat.MobileApp.Services;

/// <summary>
/// Fallback reminder scheduler for desktop/Windows and platforms without native Android alarm APIs
/// </summary>
public class FallbackReminderScheduler : IPlatformReminderScheduler
{
    public void Initialize()
    {
        Console.WriteLine("[FallbackReminderScheduler] Initialized for non-Android platform");
    }

    public void ScheduleNotification(ActiveClientReminderDto reminder)
    {
        Console.WriteLine($"[FallbackReminderScheduler] Notification scheduled in memory: {reminder.Title} at {reminder.ScheduledTime}");
    }

    public void CancelNotification(int reminderId)
    {
        Console.WriteLine($"[FallbackReminderScheduler] Notification cancelled in memory: {reminderId}");
    }

    public void CancelAllNotifications()
    {
        Console.WriteLine("[FallbackReminderScheduler] All notifications cancelled in memory");
    }

    public Task<bool> RequestPermissionAsync()
    {
        return Task.FromResult(true);
    }
}
