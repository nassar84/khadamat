using System;

namespace Khadamat.Domain.Entities;

public class Rating : BaseEntity
{
    public int ServiceId { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public int Stars { get; private set; }
    public string Comment { get; private set; } = string.Empty;
    public DateTime Date { get; private set; } = DateTime.UtcNow;
    
    public virtual Service Service { get; private set; } = null!;

    protected Rating() { }

    public Rating(int serviceId, string userId, int stars, string comment)
    {
        if (stars < 1 || stars > 5)
            throw new ArgumentOutOfRangeException(nameof(stars), "Rating must be between 1 and 5.");
            
        ServiceId = serviceId;
        UserId = userId;
        Stars = stars;
        Comment = comment;
    }
}
