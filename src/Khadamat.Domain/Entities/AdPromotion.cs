using System;

namespace Khadamat.Domain.Entities;

/// <summary>Ad extension log: when an ad duration was manually or automatically extended.</summary>
public class AdExtension : BaseEntity
{
    public int AdvertisementId { get; private set; }
    public int DaysAdded { get; private set; }
    public string Reason { get; private set; } = string.Empty; // Referral, AdminGift, PromoCode, PointsConversion
    public string? Notes { get; private set; }
    public DateTime ExtendedAt { get; private set; }
    public string ExtendedBy { get; private set; } = string.Empty; // UserId or "System"

    public virtual Advertisement Advertisement { get; private set; } = null!;

    protected AdExtension() { }

    public AdExtension(int advertisementId, int daysAdded, string reason, string extendedBy, string? notes = null)
    {
        AdvertisementId = advertisementId;
        DaysAdded = daysAdded;
        Reason = reason;
        ExtendedBy = extendedBy;
        Notes = notes;
        ExtendedAt = DateTime.UtcNow;
    }
}

/// <summary>Promo codes usable by providers for free ad days or discounts.</summary>
public class PromotionalOffer : BaseEntity
{
    public string Code { get; private set; } = string.Empty;
    public int FreeDays { get; private set; }
    public decimal DiscountPercentage { get; private set; }
    public int MaxUsages { get; private set; }
    public int UsedCount { get; private set; }
    public DateTime ExpirationDate { get; private set; }
    public bool IsActive { get; private set; } = true;
    public string? Description { get; private set; }

    protected PromotionalOffer() { }

    public PromotionalOffer(string code, int freeDays, decimal discountPercentage,
        int maxUsages, DateTime expirationDate, string? description = null)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Promo code is required.");

        Code = code.ToUpper().Trim();
        FreeDays = freeDays;
        DiscountPercentage = discountPercentage;
        MaxUsages = maxUsages;
        ExpirationDate = expirationDate;
        Description = description;
        IsActive = true;
        UsedCount = 0;
    }

    public bool CanUse() => IsActive && UsedCount < MaxUsages && DateTime.UtcNow <= ExpirationDate;

    public void Use()
    {
        if (!CanUse()) throw new InvalidOperationException("Promo code is no longer valid.");
        UsedCount++;
        if (UsedCount >= MaxUsages) IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}

/// <summary>Tracks free trial advertising periods given to new providers during launch phase.</summary>
public class TrialAdvertisement : BaseEntity
{
    public string ProviderId { get; private set; } = string.Empty; // FK → ApplicationUser.Id
    public int TrialDurationDays { get; private set; }             // 30, 60, 90
    public int UsedDays { get; private set; }
    public bool IsActive { get; private set; } = true;
    public DateTime TrialStartDate { get; private set; }
    public DateTime TrialEndDate { get; private set; }

    protected TrialAdvertisement() { }

    public TrialAdvertisement(string providerId, int trialDurationDays)
    {
        if (string.IsNullOrWhiteSpace(providerId)) throw new ArgumentException("ProviderId required.");
        if (trialDurationDays <= 0) throw new ArgumentException("Trial duration must be positive.");

        ProviderId = providerId;
        TrialDurationDays = trialDurationDays;
        TrialStartDate = DateTime.UtcNow;
        TrialEndDate = DateTime.UtcNow.AddDays(trialDurationDays);
        IsActive = true;
        UsedDays = 0;
    }

    public int RemainingDays => Math.Max(0, (TrialEndDate - DateTime.UtcNow).Days);

    public void Expire()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}
