using System;
using System.Threading.Tasks;
using Khadamat.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Khadamat.WebAPI.Controllers;

/// <summary>
/// Phase 7: REST API for Advertisement System
/// Endpoints for Mobile and Web integration.
/// </summary>
[ApiController]
[Route("v1/[controller]")]
public class AdvertisementsController : ControllerBase
{
    private readonly IAdvertisementService _adService;
    private readonly IReferralService _referralService;

    public AdvertisementsController(IAdvertisementService adService, IReferralService referralService)
    {
        _adService = adService;
        _referralService = referralService;
    }

    // ── Public Endpoints (No Auth Required) ───────────────────────────────

    /// <summary>Home page banners. Can be filtered by City.</summary>
    [HttpGet("banners")]
    public async Task<IActionResult> GetHomeBanners([FromQuery] int? cityId = null)
    {
        var banners = await _adService.GetHomeBannersAsync(cityId);
        return Ok(banners);
    }

    /// <summary>Category page ads.</summary>
    [HttpGet("category/{categoryId}")]
    public async Task<IActionResult> GetCategoryAds(int categoryId, [FromQuery] int? cityId = null)
    {
        var ads = await _adService.GetCategoryAdsAsync(categoryId, cityId);
        return Ok(ads);
    }

    // ── Analytics Tracking ─────────────────────────────────────────────────

    /// <summary>Record an ad impression (view). Called by Mobile/Web on ad display.</summary>
    [HttpPost("{adId}/impression")]
    public async Task<IActionResult> RecordImpression(int adId,
        [FromQuery] string platform = "Web",
        [FromQuery] string? pageContext = null)
    {
        var userId = User.Identity?.IsAuthenticated == true ? User.FindFirst("sub")?.Value : null;
        await _adService.RecordImpressionAsync(adId, platform, userId, pageContext);
        return NoContent();
    }

    /// <summary>Record an ad click. Called by Mobile/Web on ad tap/click.</summary>
    [HttpPost("{adId}/click")]
    public async Task<IActionResult> RecordClick(int adId,
        [FromQuery] string platform = "Web")
    {
        var userId = User.Identity?.IsAuthenticated == true ? User.FindFirst("sub")?.Value : null;
        var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        await _adService.RecordClickAsync(adId, platform, userId, ip);
        return NoContent();
    }

    // ── Admin Endpoints ────────────────────────────────────────────────────

    /// <summary>Approve a pending advertisement.</summary>
    [HttpPost("{adId}/approve")]
    [Authorize(Roles = "SystemAdmin,SuperAdmin")]
    public async Task<IActionResult> ApproveAd(int adId, [FromBody] ApproveAdRequest request)
    {
        await _adService.ApproveAdvertisementAsync(adId, request.Notes ?? "");
        return Ok(new { message = "Advertisement approved." });
    }

    /// <summary>Reject an advertisement with a reason.</summary>
    [HttpPost("{adId}/reject")]
    [Authorize(Roles = "SystemAdmin,SuperAdmin")]
    public async Task<IActionResult> RejectAd(int adId, [FromBody] RejectAdRequest request)
    {
        await _adService.RejectAdvertisementAsync(adId, request.Reason);
        return Ok(new { message = "Advertisement rejected." });
    }

    /// <summary>Trigger AdScore recalculation for all active ads (background task).</summary>
    [HttpPost("recalculate-scores")]
    [Authorize(Roles = "SystemAdmin,SuperAdmin")]
    public async Task<IActionResult> RecalculateScores()
    {
        await _adService.RecalculateAdScoresAsync();
        return Ok(new { message = "Ad scores recalculated." });
    }

    // ── Campaigns ──────────────────────────────────────────────────────────

    /// <summary>Create a new campaign.</summary>
    [HttpPost("campaigns")]
    [Authorize]
    public async Task<IActionResult> CreateCampaign([FromBody] CreateCampaignRequest req)
    {
        var advertiserId = User.FindFirst("sub")?.Value ?? "";
        var campaign = await _adService.CreateCampaignAsync(
            advertiserId, req.Name, req.Budget,
            req.StartDate, req.EndDate, req.PaymentType, req.PackageId);
        return CreatedAtAction(null, new { id = campaign.Id }, campaign);
    }

    // ── Referral & Points ──────────────────────────────────────────────────

    /// <summary>Get or generate the current user's referral code.</summary>
    [HttpGet("referral/my-code")]
    [Authorize]
    public async Task<IActionResult> GetMyReferralCode()
    {
        var providerId = User.FindFirst("sub")?.Value ?? "";
        var code = await _referralService.GetOrCreateReferralCodeAsync(providerId);
        return Ok(new { code = code.Code, totalInvites = code.TotalInvites, successful = code.SuccessfulInvites });
    }

    /// <summary>Get current points balance.</summary>
    [HttpGet("points/balance")]
    [Authorize]
    public async Task<IActionResult> GetPointsBalance()
    {
        var providerId = User.FindFirst("sub")?.Value ?? "";
        var balance = await _referralService.GetProviderPointsBalanceAsync(providerId);
        return Ok(new { balance });
    }

    /// <summary>Convert points to ad extension days.</summary>
    [HttpPost("points/convert")]
    [Authorize]
    public async Task<IActionResult> ConvertPoints([FromBody] ConvertPointsRequest req)
    {
        var providerId = User.FindFirst("sub")?.Value ?? "";
        var result = await _referralService.ConvertPointsToAdDaysAsync(providerId, req.AdvertisementId, req.PointsToSpend);
        if (!result) return BadRequest(new { message = "Insufficient points or invalid ad." });
        return Ok(new { message = "Points converted to ad days successfully." });
    }
    // ── Economy: Promotions & Trials (Phase 8) ─────────────────────────────
    
    [HttpGet("promotions")]
    [Authorize]
    public async Task<IActionResult> GetActivePromotions()
    {
        var promos = await _adService.GetActivePromotionsAsync();
        return Ok(promos);
    }

    [HttpPost("apply-promo")]
    [Authorize]
    public async Task<IActionResult> ApplyPromoCode([FromBody] ApplyPromoRequest req)
    {
        var providerId = User.FindFirst("sub")?.Value ?? "";
        var success = await _adService.ApplyPromoCodeAsync(providerId, req.AdId, req.Code);
        if (!success) return BadRequest(new { message = "Invalid or expired promo code." });
        return Ok(new { message = "Promo code applied successfully!" });
    }

    [HttpPost("admin/grant-trial")]
    [Authorize(Roles = "SystemAdmin,SuperAdmin")]
    public async Task<IActionResult> GrantTrialAd([FromBody] GrantTrialRequest req)
    {
        var trial = await _adService.ActivateTrialAsync(req.ProviderId, req.DurationDays);
        return Ok(new { message = $"Trial ad granted successfully for {req.DurationDays} days.", trial });
    }

    // ── Economy: Analytics (Phase 11) ──────────────────────────────────────

    [HttpGet("admin/analytics")]
    [Authorize(Roles = "SystemAdmin,SuperAdmin")]
    public async Task<IActionResult> GetGlobalAnalytics()
    {
        var analytics = await _adService.GetGlobalAnalyticsAsync();
        return Ok(analytics);
    }

    [HttpGet("my-analytics")]
    [Authorize]
    public async Task<IActionResult> GetMyAnalytics()
    {
        var providerId = User.FindFirst("sub")?.Value ?? "";
        var analytics = await _adService.GetAdvertiserAnalyticsAsync(providerId);
        return Ok(analytics);
    }
    [HttpPost("record-share")]
    [Authorize]
    public async Task<IActionResult> RecordShare([FromBody] RecordShareRequest req)
    {
        var userId = User.FindFirst("sub")?.Value ?? "";
        var success = await _referralService.RecordShareAsync(userId, req.ItemType, req.ItemId);
        return Ok(new { success });
    }
}

// ── Request DTOs ─────────────────────────────────────────────────────────────

public record ApproveAdRequest(string? Notes);
public record RejectAdRequest(string Reason);
public record CreateCampaignRequest(
    string Name, decimal Budget,
    DateTime StartDate, DateTime EndDate,
    string PaymentType, int? PackageId);
public record ConvertPointsRequest(int AdvertisementId, int PointsToSpend);

public record ApplyPromoRequest(int AdId, string Code);
public record GrantTrialRequest(string ProviderId, int DurationDays);
public record RecordShareRequest(string ItemType, int ItemId);
