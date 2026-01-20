using System;
using System.Collections.Generic;

namespace Khadamat.Domain.Entities;

public class ProviderProfile : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string Photo { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public int? CityId { get; set; }
    public virtual City? City { get; set; }
    public string ContactNumber { get; set; } = string.Empty;
    public string? WebsiteUrl { get; set; }
    public string? InstagramUrl { get; set; }
    public string? TwitterUrl { get; set; }
    public bool Verified { get; set; }
    public string? IdCardImage { get; set; }
    public string? CertificateImage { get; set; }
    public int? SubscriptionId { get; set; }
    
    // Navigation Properties
    // Using simple Reference for now, can be updated to specific type later
    public virtual ProviderSubscription? Subscription { get; set; } 
    public virtual ICollection<Service> Services { get; set; } = new List<Service>();
    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
}
