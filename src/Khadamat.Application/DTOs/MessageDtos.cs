using System;

namespace Khadamat.Application.DTOs;

public class MessageDto
{
    public int Id { get; set; }
    public string SenderId { get; set; } = string.Empty;
    public string ReceiverId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // Flattened
    public string? OtherPartyName { get; set; }
    public string? OtherPartyPhoto { get; set; }
}

public class ConversationDto
{
    public string OtherPartyId { get; set; } = string.Empty;
    public string OtherPartyName { get; set; } = string.Empty;
    public string? OtherPartyPhoto { get; set; }
    public string LastMessage { get; set; } = string.Empty;
    public DateTime LastMessageTime { get; set; }
    public int UnreadCount { get; set; }
}

public class SendMessageRequest
{
    public string ReceiverId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
}
