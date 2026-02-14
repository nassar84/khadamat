using System;

namespace Khadamat.Application.DTOs;

public class SubscriptionPlanDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int DurationInDays { get; set; }
    public int MaxServices { get; set; }
    public bool IsFeatured { get; set; }
}

public class ProviderSubscriptionDto
{
    public int Id { get; set; }
    public int PlanId { get; set; }
    public string PlanName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public int DaysRemainingCount => (EndDate - DateTime.UtcNow).Days;
}

public class SubscribeRequest
{
    public int PlanId { get; set; }
}
