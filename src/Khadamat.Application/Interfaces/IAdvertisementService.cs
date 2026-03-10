using System.Collections.Generic;
using System.Threading.Tasks;
using Khadamat.Domain.Entities;

namespace Khadamat.Application.Interfaces;

public interface IAdvertisementService
{
    // ── Campaigns ──────────────────────────────────────────────────────────
    Task<AdCampaign> CreateCampaignAsync(string advertiserId, string name, decimal budget,
        System.DateTime startDate, System.DateTime endDate,
        string paymentType, int? packageId = null);

    Task ActivateCampaignAsync(int campaignId);
    Task ExtendCampaignAsync(int campaignId, int days, string reason, string extendedBy);

    // ── Advertisements ─────────────────────────────────────────────────────
    Task<Advertisement> CreateAdvertisementAsync(int campaignId, string advertiserId,
        string title, string? description, string adType, string placement,
        System.DateTime startDate, System.DateTime endDate,
        string paymentType, int? categoryId, int? cityId, decimal bid, int priority);

    Task ApproveAdvertisementAsync(int adId, string adminNotes = "");
    Task RejectAdvertisementAsync(int adId, string reason);

    // ── Public Queries (Phase 3 & 7) ───────────────────────────────────────
    Task<List<Advertisement>> GetHomeBannersAsync(int? cityId = null);
    Task<List<Advertisement>> GetCategoryAdsAsync(int categoryId, int? cityId = null);
    Task<List<object>> GetSearchResultsWithAdsAsync(List<object> searchResults, int? categoryId, int? cityId);

    // ── Analytics ──────────────────────────────────────────────────────────
    Task RecordImpressionAsync(int adId, string platform, string? userId, string? pageContext);
    Task RecordClickAsync(int adId, string platform, string? userId, string? ipAddress);
    Task<AdStatistic?> GetDailyStatisticAsync(int adId, System.DateTime date);

    // ── Score Recalculation (Phase 4) ──────────────────────────────────────
    Task RecalculateAdScoresAsync();

    // ── Economy: Trials & Promos (Phase 8) ─────────────────────────────────
    Task<TrialAdvertisement> ActivateTrialAsync(string providerId, int durationDays);
    Task<bool> ApplyPromoCodeAsync(string providerId, int adId, string promoCode);
    Task<List<PromotionalOffer>> GetActivePromotionsAsync();

    // ── Economy: Analytics (Phase 11) ──────────────────────────────────────
    Task<object> GetGlobalAnalyticsAsync();
    Task<object> GetAdvertiserAnalyticsAsync(string advertiserId);
}

public interface IReferralService
{
    Task<ReferralCode> GetOrCreateReferralCodeAsync(string providerId);
    Task<bool> ProcessReferralAsync(string referralCode, string inviteeId);
    Task<int> GetProviderPointsBalanceAsync(string providerId);
    Task<bool> ConvertPointsToAdDaysAsync(string providerId, int advertisementId, int pointsToSpend);
    Task<bool> RecordShareAsync(string userId, string itemType, int itemId);
}
