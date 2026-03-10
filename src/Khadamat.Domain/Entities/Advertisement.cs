using System;

namespace Khadamat.Domain.Entities;

/// <summary>
/// A single advertisement linked to a campaign.
/// Extends the existing Ad table concept with campaign/monetization columns.
/// </summary>
public class Advertisement : BaseEntity
{
    // ── Campaign & Advertiser ──────────────────────────────────────────────
    public int CampaignId { get; private set; }
    public string AdvertiserId { get; private set; } = string.Empty; // FK → ApplicationUser.Id

    // ── Content ───────────────────────────────────────────────────────────
    public string Title { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public string? ImagePath { get; private set; }
    public string? RedirectUrl { get; private set; }

    // ── Targeting ─────────────────────────────────────────────────────────
    public int? CategoryId { get; private set; }
    public int? SubCategoryId { get; private set; }
    public int? ServiceId { get; private set; }
    public int? CityId { get; private set; }

    // ── Placement & Type ──────────────────────────────────────────────────
    public string AdType { get; private set; } = AdvertisementType.Native;   // Sponsored, Banner, Native
    public string Placement { get; private set; } = AdPlacement.Search;      // Home, Search, Category, Sidebar

    // ── Scheduling & Monetization ─────────────────────────────────────────
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    public string Status { get; private set; } = AdvertisementStatus.PendingApproval;
    public string PaymentType { get; private set; } = AdPaymentType.Free;

    // ── Smart Ranking ─────────────────────────────────────────────────────
    public int Priority { get; private set; }           // Manual override from admin
    public int DisplayOrder { get; private set; }
    public decimal Bid { get; private set; }            // How much advertiser bids
    public double AdScore { get; private set; }         // Calculated: Bid*0.5 + Rating*0.2 + CTR*0.2 + Location*0.1

    // ── Live Analytics ────────────────────────────────────────────────────
    public int Impressions { get; private set; }
    public int Clicks { get; private set; }
    public double CTR => Impressions > 0 ? (double)Clicks / Impressions : 0;

    // ── Admin ─────────────────────────────────────────────────────────────
    public string? AdminNotes { get; private set; }
    public bool IsApproved { get; private set; }

    // ── Navigation ────────────────────────────────────────────────────────
    public virtual AdCampaign Campaign { get; private set; } = null!;
    public virtual Category? Category { get; private set; }
    public virtual SubCategory? SubCategory { get; private set; }
    public virtual Service? Service { get; private set; }
    public virtual City? City { get; private set; }

    protected Advertisement() { }

    public Advertisement(
        int campaignId, string advertiserId,
        string title, string? description,
        string adType, string placement,
        DateTime startDate, DateTime endDate,
        string paymentType = AdPaymentType.Free,
        int? categoryId = null, int? subCategoryId = null, int? serviceId = null, int? cityId = null,
        decimal bid = 0, int priority = 0)
    {
        if (string.IsNullOrWhiteSpace(advertiserId)) throw new ArgumentException("AdvertiserId required.");
        if (string.IsNullOrWhiteSpace(title)) throw new ArgumentException("Title required.");
        if (endDate <= startDate) throw new ArgumentException("EndDate must be after StartDate.");

        CampaignId = campaignId;
        AdvertiserId = advertiserId;
        Title = title;
        Description = description;
        AdType = adType;
        Placement = placement;
        StartDate = startDate;
        EndDate = endDate;
        PaymentType = paymentType;
        CategoryId = categoryId;
        SubCategoryId = subCategoryId;
        ServiceId = serviceId;
        CityId = cityId;
        Bid = bid;
        Priority = priority;
        Status = AdvertisementStatus.PendingApproval;
        Impressions = 0;
        Clicks = 0;
    }

    // ── Actions ───────────────────────────────────────────────────────────
    public void Approve(string? notes = null)
    {
        IsApproved = true;
        Status = AdvertisementStatus.Active;
        AdminNotes = notes;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reject(string reason)
    {
        IsApproved = false;
        Status = AdvertisementStatus.Rejected;
        AdminNotes = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetImage(string imagePath)
    {
        ImagePath = imagePath;
        UpdatedAt = DateTime.UtcNow;
    }

    public void RecordImpression() { Impressions++; }
    public void RecordClick() { Clicks++; }

    public void UpdateAdScore(double providerRating, double locationScore)
    {
        // AdScore = Bid*0.5 + Rating*0.2 + CTR*0.2 + LocationScore*0.1
        AdScore = Math.Round((double)Bid * 0.5
                           + providerRating     * 0.2
                           + CTR                * 0.2
                           + locationScore      * 0.1
                           + Priority, 2);
        UpdatedAt = DateTime.UtcNow;
    }

    public void Extend(int days)
    {
        EndDate = EndDate.AddDays(days);
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsActive()
    {
        var now = DateTime.UtcNow;
        return IsApproved && !IsDeleted
               && Status == AdvertisementStatus.Active
               && now >= StartDate && now <= EndDate;
    }
}

public static class AdvertisementType
{
    public const string Sponsored = "Sponsored"; // Inside search results – labeled
    public const string Banner    = "Banner";    // Full-width banner on home/category
    public const string Native    = "Native";    // After every 3 results – blends in
}

public static class AdPlacement
{
    public const string Home     = "Home";
    public const string Search   = "Search";
    public const string Category = "Category";
    public const string Sidebar  = "Sidebar";
}

public static class AdvertisementStatus
{
    public const string PendingApproval = "PendingApproval";
    public const string Active          = "Active";
    public const string Paused          = "Paused";
    public const string Rejected        = "Rejected";
    public const string Expired         = "Expired";
}
