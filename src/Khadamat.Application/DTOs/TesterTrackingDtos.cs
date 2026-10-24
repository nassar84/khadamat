using System;
using System.Collections.Generic;

namespace Khadamat.Application.DTOs;

public class TesterPingRequest
{
    public string DeviceIdentifier { get; set; } = string.Empty;
    public string? TesterEmail { get; set; }
    public string? TesterName { get; set; }
    public string? DeviceModel { get; set; }
    public string? OperatingSystem { get; set; }
    public bool IsNativeApp { get; set; } = true;
}

public class TesterIdentifyRequest
{
    public string DeviceIdentifier { get; set; } = string.Empty;
    public string TesterEmail { get; set; } = string.Empty;
    public string? TesterName { get; set; }
    public string? PhoneNumber { get; set; }
}

public class AssignTesterRequest
{
    public string? TesterName { get; set; }
    public string? TesterEmail { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Notes { get; set; }
}

public class TesterDeviceDto
{
    public int Id { get; set; }
    public string DeviceIdentifier { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public string? TesterEmail { get; set; }
    public string? TesterName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? DeviceModel { get; set; }
    public string? OperatingSystem { get; set; }
    public string? IpAddress { get; set; }
    public DateTime FirstSeenAt { get; set; }
    public DateTime LastSeenAt { get; set; }
    public DateTime? LastStreakDate { get; set; }
    public int ConsecutiveDaysStreak { get; set; }
    public int TotalActiveDays { get; set; }
    public int TargetDays { get; set; } = 14;
    public bool HasMissedStreak { get; set; }
    public string? Notes { get; set; }

    // Calculated fields
    public bool IsOpenToday { get; set; }
    public bool IsTrackingEnabled { get; set; } = true;
    public int DaysRemaining => Math.Max(0, TargetDays - ConsecutiveDaysStreak);
    public bool IsCompleted => ConsecutiveDaysStreak >= TargetDays;
}

public class TesterActivityLogDto
{
    public long Id { get; set; }
    public DateTime OpenedAt { get; set; }
    public DateTime VisitDate { get; set; }
    public string? IpAddress { get; set; }
    public string? DeviceModel { get; set; }
    public string? OperatingSystem { get; set; }
    public bool IsNativeApp { get; set; }
}

public class TestersDashboardSummaryDto
{
    public bool IsTrackingEnabled { get; set; } = true;
    public int TotalTesters { get; set; }
    public int ActiveTodayCount { get; set; }
    public int InactiveTodayCount { get; set; }
    public int CompletedTestersCount { get; set; }
    public int StreakBrokenCount { get; set; }
    public List<TesterDeviceDto> Testers { get; set; } = new();
}
