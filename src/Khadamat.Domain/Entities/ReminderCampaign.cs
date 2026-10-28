using System;
using Khadamat.Domain.Enums;

namespace Khadamat.Domain.Entities;

public class ReminderCampaign : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? InternalDescription { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Language { get; set; } = "ar"; // ar, en, all
    
    public ReminderStatus Status { get; set; } = ReminderStatus.Active;
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; }
    
    public ReminderRecurrenceType RecurrenceType { get; set; } = ReminderRecurrenceType.Daily;
    public TimeSpan ScheduledTime { get; set; } = new TimeSpan(20, 0, 0); // 8:00 PM
    public int IntervalValue { get; set; } = 1;
    public string? DaysOfWeek { get; set; } // Comma-separated: "Sunday,Monday,Tuesday..."
    public int? DurationDays { get; set; }
    public int? MaxOccurrences { get; set; }
    public string TimeZoneId { get; set; } = "Local"; // Local or specific timezone
    
    public ReminderPriority Priority { get; set; } = ReminderPriority.Normal;
    public ReminderTargetAudience TargetAudience { get; set; } = ReminderTargetAudience.AllUsers;
    public string? TargetRole { get; set; }
    
    public ReminderDisplayType DisplayType { get; set; } = ReminderDisplayType.NotificationBar;
    public string ActionRoute { get; set; } = "/"; // Target internal page e.g. "/", "/profile", "/search"
    public string? ImageUrl { get; set; } // Rich image banner for notifications
    public string DeliveryChannel { get; set; } = "Both"; // Both, PushOnly, InAppOnly
    public string MessageSelectionMode { get; set; } = "Single"; // Single, ScheduledBank, RandomBank, SequentialBank
    public string? MessageBankJson { get; set; }
    public string? BankCategory { get; set; } // Category filter for bank items (e.g. "Morning", "Evening", "Midday", or null for All)
    public int LastSequentialIndex { get; set; } = 0; // Tracks which bank message index was sent last
    
    public string CampaignType { get; set; } = "Regular"; // Regular, Occasion
    public bool OverridesRegularReminders { get; set; } = false; // When true, replaces regular daily reminders on active occasion days
    public string? OccasionName { get; set; } // e.g. "شم النسيم", "شهر رمضان", "يوم الجمعة", "العشر الأوائل"
    
    public bool IsActive { get; set; } = true;
    public int ExecutionCount { get; set; } = 0;
    public DateTime? LastTriggeredAt { get; set; }

    /// <summary>
    /// Helper to determine if the campaign is currently running and valid
    /// </summary>
    public bool IsCurrentlyActive(DateTime nowUtc)
    {
        if (!IsActive || Status != ReminderStatus.Active || IsDeleted)
            return false;

        if (nowUtc < StartDate)
            return false;

        if (EndDate.HasValue && nowUtc > EndDate.Value)
            return false;

        if (DurationDays.HasValue && nowUtc > StartDate.AddDays(DurationDays.Value))
            return false;

        return true;
    }
}
