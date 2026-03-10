using System;

namespace Khadamat.Domain.Entities;

/// <summary>
/// Ad pricing packages (Basic, Silver, Gold, Platinum).
/// </summary>
public class AdPackage : BaseEntity
{
    public string Name { get; private set; } = string.Empty;          // Basic, Silver, Gold, Platinum
    public string? Description { get; private set; }
    public decimal Price { get; private set; }
    public int DurationDays { get; private set; }
    public string Tier { get; private set; } = AdPackageTier.Basic;
    public int MaxAds { get; private set; }                           // Max ads per campaign
    public bool IsFeatured { get; private set; }                      // Gold/Platinum
    public bool IsSponsored { get; private set; }                     // Appears in search
    public bool IsBanner { get; private set; }                        // Full-width banner
    public int PriorityBoost { get; private set; }                    // Added to AdScore
    public bool IsActive { get; private set; } = true;

    protected AdPackage() { }

    public AdPackage(string name, decimal price, int durationDays,
        string tier, int maxAds,
        bool isFeatured = false, bool isSponsored = false, bool isBanner = false,
        int priorityBoost = 0, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Package name is required.");

        Name = name;
        Price = price;
        DurationDays = durationDays;
        Tier = tier;
        MaxAds = maxAds;
        IsFeatured = isFeatured;
        IsSponsored = isSponsored;
        IsBanner = isBanner;
        PriorityBoost = priorityBoost;
        Description = description;
        IsActive = true;
    }

    public void Deactivate() { IsActive = false; UpdatedAt = DateTime.UtcNow; }

    public void UpdateDetails(string name, decimal price, int durationDays,
        string tier, int maxAds,
        bool isFeatured, bool isSponsored, bool isBanner,
        int priorityBoost, string? description)
    {
        Name = name;
        Price = price;
        DurationDays = durationDays;
        Tier = tier;
        MaxAds = maxAds;
        IsFeatured = isFeatured;
        IsSponsored = isSponsored;
        IsBanner = isBanner;
        PriorityBoost = priorityBoost;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }
}

public static class AdPackageTier
{
    public const string Basic    = "Basic";
    public const string Silver   = "Silver";
    public const string Gold     = "Gold";
    public const string Platinum = "Platinum";
}
