using System;

namespace Khadamat.Domain.Entities;

public class Comment : BaseEntity
{
    public int PostId { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public string Text { get; private set; } = string.Empty;
    
    public virtual Post Post { get; private set; } = null!;

    protected Comment() { }

    public Comment(int postId, string userId, string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Comment text cannot be empty.");

        PostId = postId;
        UserId = userId;
        Text = text;
    }
}
