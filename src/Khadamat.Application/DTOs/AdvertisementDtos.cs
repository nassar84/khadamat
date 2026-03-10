using System;
using System.Collections.Generic;

namespace Khadamat.Application.DTOs;

public record ApproveAdRequestDto(string? Notes);
public record RejectAdRequestDto(string Reason);
public record CreateCampaignRequestDto(
    string Name, decimal Budget,
    DateTime StartDate, DateTime EndDate,
    string PaymentType, int? PackageId);
public record ConvertPointsRequestDto(int AdvertisementId, int PointsToSpend);

public class ReferralCodeDto
{
    public string Code { get; set; } = string.Empty;
    public int TotalInvites { get; set; }
    public int Successful { get; set; }
}

public class PointsBalanceDto
{
    public int Balance { get; set; }
}

// ── Economy: Promos & Trials ──────────────────────────────────────────────────
public record ApplyPromoRequestDto(int AdId, string Code);

public class PromoCodeDto
{
    public string Code { get; set; } = string.Empty;
    public int FreeDays { get; set; }
    public decimal DiscountPercentage { get; set; }
    public string? Description { get; set; }
    public DateTime ExpirationDate { get; set; }
}

// ── Economy: Analytics ────────────────────────────────────────────────────────
public class AdAnalyticsDto
{
    public int TotalAds { get; set; }
    public int ActiveAds { get; set; }
    public int TotalClicks { get; set; }
    public int TotalImpressions { get; set; }
    public decimal TotalRevenue { get; set; }
    public double AverageCTR { get; set; }
    public double OverallCTR { get; set; }
    public List<TopPerformingAdDto> TopPerformingAds { get; set; } = new();
}

public class TopPerformingAdDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public int Clicks { get; set; }
    public int Impressions { get; set; }
    public double CTR { get; set; }
}
