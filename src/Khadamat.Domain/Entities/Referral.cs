using System;
using System.Collections.Generic;

namespace Khadamat.Domain.Entities;

/// <summary>Unique referral code per provider (e.g. KHD-1258).</summary>
public class ReferralCode : BaseEntity
{
    public string ProviderId { get; private set; } = string.Empty; // FK → ApplicationUser.Id
    public string Code { get; private set; } = string.Empty;       // e.g. KHD-1258
    public int TotalInvites { get; private set; }
    public int SuccessfulInvites { get; private set; }
    public bool IsActive { get; private set; } = true;

    public virtual ICollection<Referral> Referrals { get; private set; } = new List<Referral>();

    protected ReferralCode() { }

    public ReferralCode(string providerId, string code)
    {
        if (string.IsNullOrWhiteSpace(providerId)) throw new ArgumentException("ProviderId required.");
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Referral code required.");

        ProviderId = providerId;
        Code = code.ToUpper().Trim();
        IsActive = true;
    }

    public void IncrementSuccessful()
    {
        SuccessfulInvites++;
        UpdatedAt = DateTime.UtcNow;
    }
    public void IncrementTotal() { TotalInvites++; UpdatedAt = DateTime.UtcNow; }
}

/// <summary>A single referral event (one user invited by another via code).</summary>
public class Referral : BaseEntity
{
    public int ReferralCodeId { get; private set; }
    public string InviteeId { get; private set; } = string.Empty; // FK → ApplicationUser.Id (the new user)
    public string Status { get; private set; } = ReferralStatus.Pending;
    public DateTime RegisteredAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public int PointsAwarded { get; private set; }

    public virtual ReferralCode ReferralCode { get; private set; } = null!;

    protected Referral() { }

    public Referral(int referralCodeId, string inviteeId)
    {
        ReferralCodeId = referralCodeId;
        InviteeId = inviteeId;
        Status = ReferralStatus.Pending;
        RegisteredAt = DateTime.UtcNow;
    }

    public void Complete(int pointsToAward)
    {
        Status = ReferralStatus.Successful;
        CompletedAt = DateTime.UtcNow;
        PointsAwarded = pointsToAward;
        UpdatedAt = DateTime.UtcNow;
    }
}

public static class ReferralStatus
{
    public const string Pending    = "Pending";
    public const string Successful = "Successful";
    public const string Expired    = "Expired";
}
