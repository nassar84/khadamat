using System;

namespace Khadamat.Domain.Entities;

/// <summary>Points balance for a provider – earned via referrals, reviews, completed orders.</summary>
public class ProviderPoints : BaseEntity
{
    public string ProviderId { get; private set; } = string.Empty; // FK → ApplicationUser.Id
    public int TotalPoints { get; private set; }
    public int UsedPoints { get; private set; }
    public int Balance => TotalPoints - UsedPoints;

    protected ProviderPoints() { }

    public ProviderPoints(string providerId)
    {
        if (string.IsNullOrWhiteSpace(providerId)) throw new ArgumentException("ProviderId required.");
        ProviderId = providerId;
    }

    public void Award(int points)
    {
        if (points <= 0) throw new ArgumentException("Points must be positive.");
        TotalPoints += points;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deduct(int points)
    {
        if (points > Balance) throw new InvalidOperationException("Insufficient points balance.");
        UsedPoints += points;
        UpdatedAt = DateTime.UtcNow;
    }
}

/// <summary>History of points converted into advertisement days.</summary>
public class RewardConversion : BaseEntity
{
    public string ProviderId { get; private set; } = string.Empty;
    public int PointsDeducted { get; private set; }
    public string RewardType { get; private set; } = string.Empty;  // e.g. "7 Days Free Ad"
    public int DaysGranted { get; private set; }
    public int? AdvertisementId { get; private set; }              // Which ad was extended
    public DateTime ConvertedAt { get; private set; }

    protected RewardConversion() { }

    public RewardConversion(string providerId, int pointsDeducted, string rewardType,
        int daysGranted, int? advertisementId = null)
    {
        ProviderId = providerId;
        PointsDeducted = pointsDeducted;
        RewardType = rewardType;
        DaysGranted = daysGranted;
        AdvertisementId = advertisementId;
        ConvertedAt = DateTime.UtcNow;
    }
}

/// <summary>Lookup table: how many points are earned per action type.</summary>
public class PointRewardRule : BaseEntity
{
    public string ActionType { get; private set; } = string.Empty;  // Referral, Review, OrderCompleted
    public int PointsAwarded { get; private set; }
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;

    protected PointRewardRule() { }

    public PointRewardRule(string actionType, int pointsAwarded, string? description = null)
    {
        ActionType = actionType;
        PointsAwarded = pointsAwarded;
        Description = description;
    }
}

public static class PointActionType
{
    public const string Referral       = "Referral";
    public const string Review         = "Review";
    public const string OrderCompleted = "OrderCompleted";
    public const string Share          = "Share";
}
