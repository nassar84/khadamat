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
    public string? ProposedPhone2 { get; set; }
    public string? ProposedWhatsApp { get; set; }
    
    public string Status { get; set; } = "Pending"; // Pending, ForwardedToProvider, Approved, Rejected, Canceled
    public string? AdminNotes { get; set; }
    public string? ProviderNotes { get; set; }

    // Flags for partial approval
    public bool ApprovedName { get; set; }
    public bool ApprovedDescription { get; set; }
    public bool ApprovedAddress { get; set; }
    public bool ApprovedPrice { get; set; }
    public bool ApprovedPhone1 { get; set; }
    
    public virtual Service? Service { get; set; }
}
