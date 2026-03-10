using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Khadamat.Application.Interfaces;
using Khadamat.Domain.Entities;
using Khadamat.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Khadamat.Infrastructure.Features;

/// <summary>
/// Core advertisement engine implementing:
/// - Phase 3: Ad injection into search results (native after every 3 results)
/// - Phase 4: Smart AdScore ranking
/// - Phase 7: API data for Home/Category/Search
/// - Phase 8: Trial/Free ad activation
/// </summary>
public class AdvertisementService : IAdvertisementService
{
    private readonly KhadamatDbContext _db;

    public AdvertisementService(KhadamatDbContext db)
    {
        _db = db;
    }

    // ────────────────────────────────────────────────────────────────────────
    // Campaigns
    // ────────────────────────────────────────────────────────────────────────

    public async Task<AdCampaign> CreateCampaignAsync(
        string advertiserId, string name, decimal budget,
        DateTime startDate, DateTime endDate,
        string paymentType, int? packageId = null)
    {
        var campaign = new AdCampaign(advertiserId, name, budget, startDate, endDate, paymentType, packageId);
        _db.AdCampaigns.Add(campaign);
        await _db.SaveChangesAsync();
        return campaign;
    }

    public async Task ActivateCampaignAsync(int campaignId)
    {
        var campaign = await _db.AdCampaigns.FindAsync(campaignId)
            ?? throw new InvalidOperationException($"Campaign {campaignId} not found.");
        campaign.Activate();
        await _db.SaveChangesAsync();
    }

    public async Task ExtendCampaignAsync(int campaignId, int days, string reason, string extendedBy)
    {
        var campaign = await _db.AdCampaigns.FindAsync(campaignId)
            ?? throw new InvalidOperationException($"Campaign {campaignId} not found.");
        campaign.Extend(days);
        await _db.SaveChangesAsync();
    }

    // ────────────────────────────────────────────────────────────────────────
    // Advertisements
    // ────────────────────────────────────────────────────────────────────────

    public async Task<Advertisement> CreateAdvertisementAsync(
        int campaignId, string advertiserId,
        string title, string? description,
        string adType, string placement,
        DateTime startDate, DateTime endDate,
        string paymentType, int? categoryId, int? cityId,
        decimal bid, int priority)
    {
        var ad = new Advertisement(campaignId, advertiserId, title, description,
            adType, placement, startDate, endDate, paymentType, 
            categoryId: categoryId, cityId: cityId, bid: bid, priority: priority);
        _db.Advertisements.Add(ad);
        await _db.SaveChangesAsync();
        return ad;
    }

    public async Task ApproveAdvertisementAsync(int adId, string adminNotes = "")
    {
        var ad = await _db.Advertisements.FindAsync(adId)
            ?? throw new InvalidOperationException($"Advertisement {adId} not found.");
        ad.Approve(adminNotes);
        await _db.SaveChangesAsync();
    }

    public async Task RejectAdvertisementAsync(int adId, string reason)
    {
        var ad = await _db.Advertisements.FindAsync(adId)
            ?? throw new InvalidOperationException($"Advertisement {adId} not found.");
        ad.Reject(reason);
        await _db.SaveChangesAsync();
    }

    // ────────────────────────────────────────────────────────────────────────
    // Phase 7: Public API queries
    // ────────────────────────────────────────────────────────────────────────

    public async Task<List<Advertisement>> GetHomeBannersAsync(int? cityId = null)
    {
        var now = DateTime.UtcNow;
        return await _db.Advertisements
            .Where(a => a.IsApproved
                     && a.Placement == AdPlacement.Home
                     && a.StartDate <= now
                     && a.EndDate >= now
                     && (cityId == null || a.CityId == null || a.CityId == cityId))
            .OrderByDescending(a => a.AdScore)
            .ThenBy(a => a.DisplayOrder)
            .Take(5)
            .ToListAsync();
    }

    public async Task<List<Advertisement>> GetCategoryAdsAsync(int categoryId, int? cityId = null)
    {
        var now = DateTime.UtcNow;
        return await _db.Advertisements
            .Where(a => a.IsApproved
                     && a.Placement == AdPlacement.Category
                     && a.StartDate <= now
                     && a.EndDate >= now
                     && (a.CategoryId == null || a.CategoryId == categoryId)
                     && (cityId == null || a.CityId == null || a.CityId == cityId))
            .OrderByDescending(a => a.AdScore)
            .Take(3)
            .ToListAsync();
    }

    // ────────────────────────────────────────────────────────────────────────
    // Phase 3: Native Ad Injection into search results
    // Native ad inserted after every 3 actual results
    // ────────────────────────────────────────────────────────────────────────

    public async Task<List<object>> GetSearchResultsWithAdsAsync(
        List<object> searchResults, int? categoryId, int? cityId)
    {
        const int InjectEvery = 3; // insert ad after every N results
        var now = DateTime.UtcNow;

        // How many ad slots do we need?
        int adSlotsNeeded = searchResults.Count / InjectEvery;
        if (adSlotsNeeded == 0) return searchResults; // too few results, no injection

        var ads = await _db.Advertisements
            .Where(a => a.IsApproved
                     && (a.Placement == AdPlacement.Search || a.AdType == AdvertisementType.Sponsored)
                     && a.StartDate <= now
                     && a.EndDate >= now
                     && (categoryId == null || a.CategoryId == null || a.CategoryId == categoryId)
                     && (cityId == null || a.CityId == null || a.CityId == cityId))
            .OrderByDescending(a => a.AdScore)
            .Take(adSlotsNeeded)
            .ToListAsync();

        var merged = new List<object>();
        int adIndex = 0;

        for (int i = 0; i < searchResults.Count; i++)
        {
            merged.Add(searchResults[i]);

            // After every N results, inject an ad
            if ((i + 1) % InjectEvery == 0 && adIndex < ads.Count)
            {
                merged.Add(new { IsAd = true, Ad = ads[adIndex] });
                adIndex++;
            }
        }

        return merged;
    }

    // ────────────────────────────────────────────────────────────────────────
    // Analytics tracking
    // ────────────────────────────────────────────────────────────────────────

    public async Task RecordImpressionAsync(int adId, string platform, string? userId, string? pageContext)
    {
        var ad = await _db.Advertisements.FindAsync(adId);
        if (ad == null) return;

        ad.RecordImpression();
        _db.AdImpressions.Add(new AdImpression(adId, platform, userId, pageContext));
        await _db.SaveChangesAsync();
    }

    public async Task RecordClickAsync(int adId, string platform, string? userId, string? ipAddress)
    {
        var ad = await _db.Advertisements.FindAsync(adId);
        if (ad == null) return;

        ad.RecordClick();
        _db.AdClicks.Add(new AdClick(adId, platform, userId, ipAddress));
        await _db.SaveChangesAsync();
    }

    public async Task<AdStatistic?> GetDailyStatisticAsync(int adId, DateTime date)
        => await _db.AdStatistics.FirstOrDefaultAsync(s => s.AdvertisementId == adId && s.Date == date.Date);

    // ────────────────────────────────────────────────────────────────────────
    // Phase 4: Smart AdScore recalculation
    // AdScore = Bid*0.5 + Rating*0.2 + CTR*0.2 + LocationScore*0.1
    // ────────────────────────────────────────────────────────────────────────

    public async Task RecalculateAdScoresAsync()
    {
        var activeAds = await _db.Advertisements
            .Where(a => a.IsApproved && !a.IsDeleted)
            .ToListAsync();

        foreach (var ad in activeAds)
        {
            // Fetch provider rating if available
            double providerRating = 0;
            if (!string.IsNullOrEmpty(ad.AdvertiserId))
            {
                var profile = await _db.ProviderProfiles
                    .FirstOrDefaultAsync(p => p.UserId == ad.AdvertiserId);
                if (profile != null)
                    providerRating = 5.0; // Placeholder for actual rating calculation
            }

            // Location score: 1.0 if ad targets a city, 0.5 generic
            double locationScore = ad.CityId.HasValue ? 1.0 : 0.5;

            ad.UpdateAdScore(providerRating, locationScore);
        }

        await _db.SaveChangesAsync();
    }

    // ────────────────────────────────────────────────────────────────────────
    // Phase 8: Economy: Trials & Promos
    // ────────────────────────────────────────────────────────────────────────

    public async Task<TrialAdvertisement> ActivateTrialAsync(string providerId, int durationDays)
    {
        var trial = new TrialAdvertisement(providerId, durationDays);
        _db.TrialAdvertisements.Add(trial);
        await _db.SaveChangesAsync();
        return trial;
    }

    public async Task<bool> ApplyPromoCodeAsync(string providerId, int adId, string promoCode)
    {
        var offer = await _db.PromotionalOffers
            .FirstOrDefaultAsync(p => p.Code == promoCode && p.IsActive);
        
        if (offer == null || !offer.CanUse()) return false;

        var ad = await _db.Advertisements.FindAsync(adId);
        if (ad == null || ad.AdvertiserId != providerId) return false;

        // Apply Free Days
        if (offer.FreeDays > 0)
        {
            ad.Extend(offer.FreeDays);
            var extension = new AdExtension(adId, offer.FreeDays, "PromoCode", providerId, $"Used code: {promoCode}");
            _db.AdExtensions.Add(extension);
        }

        offer.Use();
        await _db.SaveChangesAsync();

        return true;
    }

    public async Task<List<PromotionalOffer>> GetActivePromotionsAsync()
    {
        var now = DateTime.UtcNow;
        return await _db.PromotionalOffers
            .Where(p => p.IsActive && p.ExpirationDate >= now && p.UsedCount < p.MaxUsages)
            .ToListAsync();
    }

    // ────────────────────────────────────────────────────────────────────────
    // Phase 11: Economy: Analytics & Reports
    // ────────────────────────────────────────────────────────────────────────

    public async Task<object> GetGlobalAnalyticsAsync()
    {
        var totalAds = await _db.Advertisements.CountAsync(a => !a.IsDeleted);
        var activeAds = await _db.Advertisements.CountAsync(a => !a.IsDeleted && a.IsApproved && a.EndDate >= DateTime.UtcNow);
        var totalClicks = await _db.AdClicks.CountAsync();
        var totalImpressions = await _db.AdImpressions.CountAsync();
        
        var revenue = await _db.AdCampaigns
             .Where(c => c.Budget > 0 && c.PackageId != null)
             .SumAsync(c => c.Budget);

        return new
        {
            TotalAds = totalAds,
            ActiveAds = activeAds,
            TotalClicks = totalClicks,
            TotalImpressions = totalImpressions,
            TotalRevenue = revenue,
            AverageCTR = totalImpressions > 0 ? Math.Round((double)totalClicks / totalImpressions * 100, 2) : 0
        };
    }

    public async Task<object> GetAdvertiserAnalyticsAsync(string advertiserId)
    {
        var ads = await _db.Advertisements
            .Where(a => a.AdvertiserId == advertiserId && !a.IsDeleted)
            .ToListAsync();

        var totalClicks = ads.Sum(a => a.Clicks);
        var totalImpressions = ads.Sum(a => a.Impressions);

        var topAds = ads.OrderByDescending(a => a.Clicks).Take(5).Select(a => new
        {
            a.Id,
            a.Title,
            a.Clicks,
            a.Impressions,
            CTR = Math.Round(a.CTR * 100, 2)
        });

        return new
        {
            TotalAds = ads.Count,
            TotalClicks = totalClicks,
            TotalImpressions = totalImpressions,
            OverallCTR = totalImpressions > 0 ? Math.Round((double)totalClicks / totalImpressions * 100, 2) : 0,
            TopPerformingAds = topAds
        };
    }
}
