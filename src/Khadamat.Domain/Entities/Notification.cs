using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Khadamat.Domain.Entities;

public class Notification : BaseEntity
{
    [Required]
    public string UserId { get; set; } = string.Empty; // Recipient

    [Required]
    [MaxLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Message { get; set; } = string.Empty;

    public string? RelatedLink { get; set; } // e.g., "/service/1"

    public string? ImageUrl { get; set; } // Banner image for rich push notification and notifications center

    public bool IsRead { get; set; } = false;

    public string Type { get; set; } = "System"; // System, Message, Order, etc.

    // Constructor
    public Notification() {}

    public Notification(string userId, string title, string message, string type = "System", string? link = null, string? imageUrl = null)
    {
        UserId = userId;
        Title = title;
        Message = message;
        Type = type;
        RelatedLink = link;
        ImageUrl = imageUrl;
        CreatedAt = DateTime.UtcNow;
    }
}
