using System;
using Khadamat.Domain.Enums;

namespace Khadamat.Domain.Entities;

public class ServiceRequest : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public int ServiceId { get; set; }
    public int ProviderId { get; set; }
    
    public RequestStatus Status { get; set; } = RequestStatus.Pending;
    public string? Notes { get; set; }
    public string? ProviderNotes { get; set; }
    
    public DateTime? PreferredDate { get; set; }
    
    // Navigation Properties
    public virtual Service Service { get; set; } = null!;
    public virtual ProviderProfile Provider { get; set; } = null!;
}
