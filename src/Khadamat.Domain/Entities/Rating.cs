using System;

namespace Khadamat.Domain.Entities;

public class Rating : BaseEntity
{
    public int ServiceId { get; private set; }
    public string UserId { get; private set; } = string.Empty;
    public int Stars { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime Date { get; set; } = DateTime.UtcNow;
    
    public int? ServiceRequestId { get; private set; }
    
    public virtual Service Service { get; private set; } = null!;
    public virtual ServiceRequest? ServiceRequest { get; private set; }

    protected Rating() { }

    public Rating(int serviceId, string userId, int stars, string comment, int? serviceRequestId = null)
    {
        if (stars < 1 || stars > 5)
            throw new ArgumentOutOfRangeException(nameof(stars), "Rating must be between 1 and 5.");
            
        ServiceId = serviceId;
        UserId = userId;
        Stars = stars;
        Comment = comment;
        ServiceRequestId = serviceRequestId;
    }
}
