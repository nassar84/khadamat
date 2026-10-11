using System;
using System.Collections.Generic;
using Khadamat.Domain.Enums;

namespace Khadamat.Application.DTOs;

public class ReminderCampaignDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? InternalDescription { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Language { get; set; } = "ar";
    
    public ReminderStatus Status { get; set; }
    public string StatusText => Status switch
    {
        ReminderStatus.Draft => "مسودة",
        ReminderStatus.Active => "مفعّل",
        ReminderStatus.Paused => "متوقف مؤقتاً",
        ReminderStatus.Expired => "منتهي",
        _ => "غير محدد"
    };

    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    public ReminderRecurrenceType RecurrenceType { get; set; }
    public string RecurrenceTypeText => RecurrenceType switch
    {
        ReminderRecurrenceType.Once => "مرة واحدة",
        ReminderRecurrenceType.Daily => "يومياً",
        ReminderRecurrenceType.Weekly => "أسبوعياً",
        ReminderRecurrenceType.SpecificDaysOfWeek => "أيام محددة",
        ReminderRecurrenceType.IntervalHours => "كل عدد ساعات",
        ReminderRecurrenceType.IntervalDays => "كل عدد أيام",
        _ => "أخرى"
    };

    public TimeSpan ScheduledTime { get; set; }
    public string ScheduledTimeString => ScheduledTime.ToString(@"hh\:mm");
    
    public int IntervalValue { get; set; } = 1;
    public string? DaysOfWeek { get; set; }
    public int? DurationDays { get; set; }
    public int? MaxOccurrences { get; set; }
    public string TimeZoneId { get; set; } = "Local";
    
    public ReminderPriority Priority { get; set; }
    public string PriorityText => Priority switch
    {
        ReminderPriority.Low => "منخفضة",
        ReminderPriority.Normal => "عادية",
        ReminderPriority.High => "مرتفعة",
        ReminderPriority.Urgent => "عاجلة",
        _ => "عادية"
    };

    public ReminderTargetAudience TargetAudience { get; set; }
    public string TargetAudienceText => TargetAudience switch
    {
        ReminderTargetAudience.AllUsers => "جميع المستخدمين",
        ReminderTargetAudience.Testers => "مختبرو التطبيق",
        ReminderTargetAudience.Clients => "العملاء",
        ReminderTargetAudience.Providers => "مقدمو الخدمات",
        ReminderTargetAudience.SpecificRole => "دور محدد",
        _ => "الكل"
    };
    public string? TargetRole { get; set; }
    
    public ReminderDisplayType DisplayType { get; set; }
    public string DisplayTypeText => DisplayType switch
    {
        ReminderDisplayType.NotificationBar => "إشعار محلي بالنظام",
        ReminderDisplayType.InAppBanner => "شريط تنبيهي بالتطبيق",
        ReminderDisplayType.InAppDialog => "نافذة حوارية منبثقة",
        ReminderDisplayType.TaskReminder => "تذكير بمهمة محددة",
        _ => "إشعار محلي"
    };

    public string ActionRoute { get; set; } = "/";
    public bool IsActive { get; set; }
    public int ExecutionCount { get; set; }
    public DateTime? LastTriggeredAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateReminderCampaignDto
{
    public string Title { get; set; } = string.Empty;
    public string? InternalDescription { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Language { get; set; } = "ar";
    
    public ReminderStatus Status { get; set; } = ReminderStatus.Active;
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime? EndDate { get; set; }
    
    public ReminderRecurrenceType RecurrenceType { get; set; } = ReminderRecurrenceType.Daily;
    public TimeSpan ScheduledTime { get; set; } = new TimeSpan(20, 0, 0); // 8:00 PM
    public int IntervalValue { get; set; } = 1;
    public string? DaysOfWeek { get; set; }
    public int? DurationDays { get; set; }
    public int? MaxOccurrences { get; set; }
    public string TimeZoneId { get; set; } = "Local";
    
    public ReminderPriority Priority { get; set; } = ReminderPriority.Normal;
    public ReminderTargetAudience TargetAudience { get; set; } = ReminderTargetAudience.AllUsers;
    public string? TargetRole { get; set; }
    
    public ReminderDisplayType DisplayType { get; set; } = ReminderDisplayType.NotificationBar;
    public string ActionRoute { get; set; } = "/";
    public bool IsActive { get; set; } = true;
}

public class UpdateReminderCampaignDto : CreateReminderCampaignDto
{
    public int Id { get; set; }
}

public class ReminderFilterDto
{
    public string? SearchTerm { get; set; }
    public ReminderStatus? Status { get; set; }
    public ReminderTargetAudience? TargetAudience { get; set; }
    public ReminderDisplayType? DisplayType { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Lightweight payload for mobile app synchronization
/// </summary>
public class ActiveClientReminderDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Language { get; set; } = "ar";
    public ReminderRecurrenceType RecurrenceType { get; set; }
    public TimeSpan ScheduledTime { get; set; }
    public int IntervalValue { get; set; } = 1;
    public string? DaysOfWeek { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? DurationDays { get; set; }
    public ReminderDisplayType DisplayType { get; set; }
    public string ActionRoute { get; set; } = "/";
    public ReminderPriority Priority { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class UpdateReminderStatusRequest
{
    public ReminderStatus Status { get; set; }
}
