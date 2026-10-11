using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Khadamat.Application.DTOs;

namespace Khadamat.MobileApp.Services;

public interface ILocalReminderScheduler
{
    /// <summary>
    /// Initializes channels and syncs reminders on app launch
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Fetches active reminders from server and syncs local device schedule
    /// </summary>
    Task SyncRemindersAsync(bool force = false);

    /// <summary>
    /// Checks and returns in-app reminders if any are due
    /// </summary>
    Task<List<ActiveClientReminderDto>> GetActiveInAppRemindersAsync();

    /// <summary>
    /// Requests notification permission on Android 13+
    /// </summary>
    Task<bool> RequestNotificationPermissionAsync();

    /// <summary>
    /// Checks whether reminders are enabled by the user in settings
    /// </summary>
    bool AreRemindersEnabled();

    /// <summary>
    /// Sets whether reminders are enabled by the user
    /// </summary>
    Task SetRemindersEnabledAsync(bool enabled);

    /// <summary>
    /// Marks an in-app reminder as seen so it does not annoy user repeatedly
    /// </summary>
    void MarkInAppReminderSeen(int reminderId);

    /// <summary>
    /// Event triggered when an in-app reminder should be presented
    /// </summary>
    event Action<ActiveClientReminderDto>? OnInAppReminderDue;
}
