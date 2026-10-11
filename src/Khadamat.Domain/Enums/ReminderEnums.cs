namespace Khadamat.Domain.Enums;

public enum ReminderStatus
{
    Draft = 0,
    Active = 1,
    Paused = 2,
    Expired = 3
}

public enum ReminderRecurrenceType
{
    Once = 0,
    Daily = 1,
    Weekly = 2,
    SpecificDaysOfWeek = 3,
    IntervalHours = 4,
    IntervalDays = 5
}

public enum ReminderDisplayType
{
    NotificationBar = 0, // إشعار محلي في شريط إشعارات النظام
    InAppBanner = 1,     // شريط أو بطاقة تنبيهية داخل التطبيق
    InAppDialog = 2,     // نافذة حوارية داخل التطبيق
    TaskReminder = 3     // تذكير بمهمة محددة مع توجيه مباشر
}

public enum ReminderTargetAudience
{
    AllUsers = 0,
    Testers = 1,
    Clients = 2,
    Providers = 3,
    SpecificRole = 4
}

public enum ReminderPriority
{
    Low = 0,
    Normal = 1,
    High = 2,
    Urgent = 3
}
