using System.Collections.Generic;

namespace Khadamat.Domain.Entities;

public class Governorate : BaseEntity
{
    public string Governorate_Name_AR { get; set; } = string.Empty;
    public string Governorate_Name_EN { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool Approved { get; set; }
    public string? Notes { get; set; }
    public string? UserCreated { get; set; }

    public virtual ICollection<City> Cities { get; set; } = new List<City>();
}

public class City : BaseEntity
{
    public int GovernorateId { get; set; }
    public string City_Name_AR { get; set; } = string.Empty;
    public string City_Name_EN { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool Approved { get; set; }
    public string? Notes { get; set; }
    public string? UserCreated { get; set; }
    
    public virtual Governorate Governorate { get; set; } = null!;
    public virtual ICollection<Service> Services { get; set; } = new List<Service>();
    public virtual ICollection<ProviderProfile> ProviderProfiles { get; set; } = new List<ProviderProfile>();
}
