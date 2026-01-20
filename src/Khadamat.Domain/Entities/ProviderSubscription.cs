using System;
using Khadamat.Domain.Exceptions;

namespace Khadamat.Domain.Entities;

public class ProviderSubscription : BaseEntity
{
    public int ProviderId { get; private set; }
    public int PlanId { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public bool IsActive { get; private set; }
    
    public virtual ProviderProfile Provider { get; private set; } = null!;
    public virtual SubscriptionPlan Plan { get; private set; } = null!;

    protected ProviderSubscription() { }

    public ProviderSubscription(int providerId, int planId, int durationInDays)
    {
        ProviderId = providerId;
        PlanId = planId;
        StartDate = DateTime.UtcNow;
        EndDate = StartDate.AddDays(durationInDays);
        IsActive = true;
    }

    public void Cancel()
    {
        IsActive = false;
    }
}
