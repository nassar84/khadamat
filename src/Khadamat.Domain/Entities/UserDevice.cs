using System;

namespace Khadamat.Domain.Entities;

public class UserDevice : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string DeviceToken { get; set; } = string.Empty;
    public string Platform { get; set; } = "android"; // "android", "ios", "web"
    public string? DeviceId { get; set; }
    public string? AppVersion { get; set; }
    public DateTime LastActiveAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}
