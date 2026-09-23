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

public class AdminProviderSubscriptionDto
{
    public int ProviderId { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string OwnerName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string? CityName { get; set; }
    public string? Photo { get; set; }
    
    // Subscription Details
    public int? SubscriptionId { get; set; }
    public int? PlanId { get; set; }
    public string PlanName { get; set; } = "لا يوجد";
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; }
    public bool IsExpired => EndDate.HasValue && EndDate.Value < DateTime.UtcNow;
    public int DaysRemainingCount => EndDate.HasValue ? (int)Math.Ceiling((EndDate.Value - DateTime.UtcNow).TotalDays) : 0;
    public int ServicesCount { get; set; }
    public int MaxServices { get; set; }
}

public class ExtendSubscriptionsRequest
{
    public List<int> ProviderIds { get; set; } = new();
    public bool All { get; set; }
    public int DaysToAdd { get; set; } = 90;
}

public class ToggleSubscriptionsStatusRequest
{
    public List<int> ProviderIds { get; set; } = new();
    public bool All { get; set; }
    public bool IsActive { get; set; }
}

public class AssignPlanRequest
{
    public int ProviderId { get; set; }
    public int PlanId { get; set; }
    public int? CustomDays { get; set; }
}

