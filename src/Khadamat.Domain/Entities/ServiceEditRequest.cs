using System;

namespace Khadamat.Domain.Entities;

public class ServiceEditRequest : BaseEntity
{
    public int ServiceId { get; set; }
    public string RequesterId { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;
    
    public string? ProposedName { get; set; }
    public string? ProposedDescription { get; set; }
    public string? ProposedAddress { get; set; }
    public decimal? ProposedPrice { get; set; }
    public string? ProposedPhone1 { get; set; }
    
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
    
    public virtual Service? Service { get; set; }
}
