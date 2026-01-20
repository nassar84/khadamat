using System;

namespace Khadamat.Domain.Entities;

public class SubscriptionPlan : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int DurationInDays { get; private set; }
    public int MaxServices { get; private set; }
    public bool IsFeatured { get; private set; }
    
    protected SubscriptionPlan() { }

    public SubscriptionPlan(string name, decimal price, int durationInDays, int maxServices, bool isFeatured)
    {
        Name = name;
        Price = price;
        DurationInDays = durationInDays;
        MaxServices = maxServices;
        IsFeatured = isFeatured;
    }
}
