using System;
using System.Collections.Generic;

namespace Khadamat.Domain.Entities;

/// <summary>
/// Represents an advertising campaign grouping one or many advertisements.
/// </summary>
public class AdCampaign : BaseEntity
{
    public string AdvertiserId { get; private set; } = string.Empty; // FK → ApplicationUser.Id
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public decimal Budget { get; private set; }
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public string Status { get; private set; } = AdCampaignStatus.Draft; // Draft, Active, Paused, Ended
    public string PaymentType { get; private set; } = AdPaymentType.Free; // Free, Trial, Paid, Promo
    public int? PackageId { get; private set; } // FK → AdPackage

    // Navigation
    public virtual AdPackage? Package { get; private set; }
    public virtual ICollection<Advertisement> Advertisements { get; private set; } = new List<Advertisement>();

    protected AdCampaign() { }

    public AdCampaign(string advertiserId, string name, decimal budget,
        DateTime startDate, DateTime endDate,
        string paymentType = AdPaymentType.Free, int? packageId = null)
    {
        if (string.IsNullOrWhiteSpace(advertiserId)) throw new ArgumentException("AdvertiserId is required.");
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Campaign name is required.");
        if (endDate <= startDate) throw new ArgumentException("EndDate must be after StartDate.");

        AdvertiserId = advertiserId;
        Name = name;
        Budget = budget;
        StartDate = startDate;
        EndDate = endDate;
        PaymentType = paymentType;
        PackageId = packageId;
        Status = AdCampaignStatus.Draft;
    }

    public void Activate() { Status = AdCampaignStatus.Active; UpdatedAt = DateTime.UtcNow; }
    public void Pause()    { Status = AdCampaignStatus.Paused; UpdatedAt = DateTime.UtcNow; }
    public void End()      { Status = AdCampaignStatus.Ended;  UpdatedAt = DateTime.UtcNow; }

    public void Extend(int days)
    {
        EndDate = EndDate.AddDays(days);
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsActive()
    {
        var now = DateTime.UtcNow;
        return Status == AdCampaignStatus.Active && now >= StartDate && now <= EndDate && !IsDeleted;
    }
}

public static class AdCampaignStatus
{
    public const string Draft  = "Draft";
    public const string Active = "Active";
    public const string Paused = "Paused";
    public const string Ended  = "Ended";
}

public static class AdPaymentType
{
    public const string Free  = "Free";
    public const string Trial = "Trial";
    public const string Paid  = "Paid";
    public const string Promo = "Promo";
}
