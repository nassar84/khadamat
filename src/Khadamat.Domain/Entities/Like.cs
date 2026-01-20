using System;

namespace Khadamat.Domain.Entities;

public class Like : BaseEntity
{
    public string UserId { get; private set; } = string.Empty;
    public int? ServiceId { get; private set; }
    public int? PostId { get; private set; }
    
    public virtual Service? Service { get; private set; }
    public virtual Post? Post { get; private set; }

    protected Like() { }

    public Like(string userId, int? serviceId = null, int? postId = null)
    {
        UserId = userId;
        ServiceId = serviceId;
        PostId = postId;
    }
}
