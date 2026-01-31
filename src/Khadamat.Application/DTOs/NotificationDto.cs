using System;

namespace Khadamat.Application.DTOs;

public class NotificationDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? RelatedLink { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SendNotificationRequest
{
    public string? UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? RelatedLink { get; set; }
    
    // Broadcast filters
    public string? TargetRole { get; set; }
    public int? GovernorateId { get; set; }
    public int? CityId { get; set; }
    public int? MainCategoryId { get; set; }
}
