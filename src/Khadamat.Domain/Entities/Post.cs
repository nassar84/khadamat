using System;
using System.Collections.Generic;

namespace Khadamat.Domain.Entities;

public class Post : BaseEntity
{
    public int ProviderId { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public string? ImageUrl { get; private set; }
    
    public virtual ProviderProfile Provider { get; private set; } = null!;
    public virtual ICollection<Comment> Comments { get; private set; } = new List<Comment>();
    public virtual ICollection<Like> Likes { get; private set; } = new List<Like>();

    protected Post() { }

    public Post(int providerId, string content, string? imageUrl)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Post content cannot be empty.");
            
        ProviderId = providerId;
        Content = content;
        ImageUrl = imageUrl;
    }
}
