using System;

namespace Khadamat.Domain.Entities;

public class TesterActivityLog
{
    public long Id { get; set; }

    public int TesterDeviceId { get; set; }
    public virtual TesterDevice? TesterDevice { get; set; }

    public string DeviceIdentifier { get; set; } = string.Empty;

    public string? UserId { get; set; }

    public string? TesterEmail { get; set; }

    public DateTime OpenedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Calendar date part (UTC or Cairo date) for quick daily grouping
    /// </summary>
    public DateTime VisitDate { get; set; }

    public string? IpAddress { get; set; }

    public string? DeviceModel { get; set; }

    public string? OperatingSystem { get; set; }

    public bool IsNativeApp { get; set; } = true;
}
