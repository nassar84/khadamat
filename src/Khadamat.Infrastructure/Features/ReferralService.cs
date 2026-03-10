using System;
using System.Threading.Tasks;
using Khadamat.Application.Interfaces;
using Khadamat.Domain.Entities;
using Khadamat.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Khadamat.Infrastructure.Features;

/// <summary>
/// Handles the referral growth engine (Phase 5 & 10):
/// - Generate referral codes (e.g. KHD-1258)
/// - Process invites and award points
/// - Convert points to ad extension days
/// </summary>
public class ReferralService : IReferralService
{
    private readonly KhadamatDbContext _db;

    public ReferralService(KhadamatDbContext db)
    {
        _db = db;
    }

    public async Task<ReferralCode> GetOrCreateReferralCodeAsync(string providerId)
    {
        var existing = await _db.ReferralCodes
            .FirstOrDefaultAsync(r => r.ProviderId == providerId && r.IsActive);

        if (existing != null) return existing;

        // Generate a unique code: KHD-XXXX
        string code;
        do
        {
            code = $"KHD-{new Random().Next(1000, 9999)}";
        } while (await _db.ReferralCodes.AnyAsync(r => r.Code == code));

        var referralCode = new ReferralCode(providerId, code);
        _db.ReferralCodes.Add(referralCode);
        await _db.SaveChangesAsync();
        return referralCode;
    }

    public async Task<bool> ProcessReferralAsync(string referralCode, string inviteeId)
    {
        var code = await _db.ReferralCodes
            .FirstOrDefaultAsync(r => r.Code == referralCode.ToUpper() && r.IsActive);

        if (code == null) return false;

        // Prevent self-referral
        if (code.ProviderId == inviteeId) return false;

        // Prevent duplicate referrals
        var alreadyReferred = await _db.Referrals
            .AnyAsync(r => r.ReferralCodeId == code.Id && r.InviteeId == inviteeId);
        if (alreadyReferred) return false;

        var referral = new Referral(code.Id, inviteeId);
        _db.Referrals.Add(referral);

        // Award points to the inviter based on reward rules
        var rule = await _db.PointRewardRules
            .FirstOrDefaultAsync(r => r.ActionType == PointActionType.Referral && r.IsActive);
        int pointsToAward = rule?.PointsAwarded ?? 50; // Default 50 points per referral

        referral.Complete(pointsToAward);
        code.IncrementSuccessful();
        code.IncrementTotal();

        // Add or update provider points
        var providerPoints = await _db.ProviderPoints
            .FirstOrDefaultAsync(p => p.ProviderId == code.ProviderId);

        if (providerPoints == null)
        {
            providerPoints = new ProviderPoints(code.ProviderId);
            _db.ProviderPoints.Add(providerPoints);
        }

        providerPoints.Award(pointsToAward);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<int> GetProviderPointsBalanceAsync(string providerId)
    {
        var points = await _db.ProviderPoints
            .FirstOrDefaultAsync(p => p.ProviderId == providerId);
        return points?.Balance ?? 0;
    }

    public async Task<bool> ConvertPointsToAdDaysAsync(string providerId, int advertisementId, int pointsToSpend)
    {
        // Validate rule: e.g. 100 points = 7 days
        // Using a simple ratio: 100 points = 7 days
        if (pointsToSpend < 100) return false;
        int daysGranted = (pointsToSpend / 100) * 7;

        var providerPoints = await _db.ProviderPoints
            .FirstOrDefaultAsync(p => p.ProviderId == providerId);
        if (providerPoints == null || providerPoints.Balance < pointsToSpend) return false;

        var ad = await _db.Advertisements.FindAsync(advertisementId);
        if (ad == null) return false;

        // Deduct points
        providerPoints.Deduct(pointsToSpend);

        // Extend the ad
        ad.Extend(daysGranted);

        // Log the extension
        _db.AdExtensions.Add(new AdExtension(
            advertisementId, daysGranted,
            reason: "PointsConversion",
            extendedBy: providerId,
            notes: $"{pointsToSpend} points → {daysGranted} days"));

        // Log the conversion
        _db.RewardConversions.Add(new RewardConversion(
            providerId, pointsToSpend,
            rewardType: $"{daysGranted} Days Free Ad",
            daysGranted: daysGranted,
            advertisementId: advertisementId));

        await _db.SaveChangesAsync();
        return true;
    }
    public async Task<bool> RecordShareAsync(string userId, string itemType, int itemId)
    {
        // Award points for sharing
        var rule = await _db.PointRewardRules
            .FirstOrDefaultAsync(r => r.ActionType == PointActionType.Share && r.IsActive);
        
        int pointsToAward = rule?.PointsAwarded ?? 5; // Default 5 points per share

        var providerPoints = await _db.ProviderPoints
            .FirstOrDefaultAsync(p => p.ProviderId == userId);

        if (providerPoints == null)
        {
            providerPoints = new ProviderPoints(userId);
            _db.ProviderPoints.Add(providerPoints);
        }

        providerPoints.Award(pointsToAward);
        await _db.SaveChangesAsync();
        return true;
    }
}
