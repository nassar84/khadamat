using System;
using System.Collections.Generic;

namespace Khadamat.Domain.Entities;

public class TesterDevice : BaseEntity
{
    /// <summary>
    /// Persistent client-generated unique identifier (UUID) saved across sessions
    /// </summary>
    public string DeviceIdentifier { get; set; } = string.Empty;

    /// <summary>
    /// Identity User ID if logged in
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Google Play tester email
    /// </summary>
    public string? TesterEmail { get; set; }

    /// <summary>
    /// Friendly name or tester full name
    /// </summary>
    public string? TesterName { get; set; }

    /// <summary>
    /// WhatsApp or contact phone number for quick reminders
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Device model detected from UserAgent (e.g. Samsung SM-S918B)
    /// </summary>
    public string? DeviceModel { get; set; }

    /// <summary>
    /// Operating system detected (e.g. Android 14)
    /// </summary>
    public string? OperatingSystem { get; set; }

    /// <summary>
    /// Last detected IP address
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// First time this device opened the application
    /// </summary>
    public DateTime FirstSeenAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Most recent time this device opened the application
    /// </summary>
    public DateTime LastSeenAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// The date of the last streak increment (stored as Date only, e.g. 2026-10-24)
    /// </summary>
    public DateTime? LastStreakDate { get; set; }

    /// <summary>
    /// Current consecutive days streak (resets to 1 if user skips a day)
    /// </summary>
    public int ConsecutiveDaysStreak { get; set; } = 0;

    /// <summary>
    /// Total unique days opened on record (permanent history, never cleared on streak reset)
    /// </summary>
    public int TotalActiveDays { get; set; } = 0;

    /// <summary>
    /// Total target days required by Google Play (default: 14)
    /// </summary>
    public int TargetDays { get; set; } = 14;

    /// <summary>
    /// Flag indicating whether this tester previously missed a day and had their streak reset
    /// </summary>
    public bool HasMissedStreak { get; set; } = false;

    /// <summary>
    /// Admin notes
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Detailed history of all visits (never deleted on reset)
    /// </summary>
    public virtual ICollection<TesterActivityLog> ActivityLogs { get; set; } = new List<TesterActivityLog>();
}
