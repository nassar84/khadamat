using System;

namespace Khadamat.Domain.Entities;

public class Favorite : BaseEntity
{
    public string UserId { get; private set; } = string.Empty;
    public int? ServiceId { get; private set; }
    public int? ProviderId { get; private set; }
    
    public virtual Service? Service { get; private set; }
    public virtual ProviderProfile? Provider { get; private set; }

    protected Favorite() { }

    public Favorite(string userId, int? serviceId = null, int? providerId = null)
    {
        if (serviceId == null && providerId == null)
            throw new ArgumentException("Favorite must be linked to either a Service or a Provider.");

        UserId = userId;
        ServiceId = serviceId;
        ProviderId = providerId;
    }
}
