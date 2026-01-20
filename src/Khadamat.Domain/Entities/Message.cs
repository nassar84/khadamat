using System;

namespace Khadamat.Domain.Entities;

public class Message : BaseEntity
{
    public string SenderId { get; private set; } = string.Empty;
    public string ReceiverId { get; private set; } = string.Empty;
    public string Content { get; private set; } = string.Empty;
    public bool IsRead { get; private set; }
    
    protected Message() { }

    public Message(string senderId, string receiverId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Message content cannot be empty.");

        SenderId = senderId;
        ReceiverId = receiverId;
        Content = content;
        IsRead = false;
    }

    public void MarkAsRead()
    {
        IsRead = true;
    }
}
