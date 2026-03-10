using System;

namespace Khadamat.Domain.Entities;

/// <summary>Track each time an advertisement is shown to a user.</summary>
public class AdImpression : BaseEntity
{
    public int AdvertisementId { get; private set; }
    public string? UserId { get; private set; }           // null = anonymous
    public string Platform { get; private set; } = "Web"; // Web, Mobile
    public DateTime ViewedAt { get; private set; }
    public string? PageContext { get; private set; }      // Home, Search, Category

    public virtual Advertisement Advertisement { get; private set; } = null!;

    protected AdImpression() { }

    public AdImpression(int advertisementId, string platform, string? userId = null, string? pageContext = null)
    {
        AdvertisementId = advertisementId;
        Platform = platform;
        UserId = userId;
        PageContext = pageContext;
        ViewedAt = DateTime.UtcNow;
    }
}

/// <summary>Track each time an advertisement is clicked.</summary>
public class AdClick : BaseEntity
{
    public int AdvertisementId { get; private set; }
    public string? UserId { get; private set; }
    public string Platform { get; private set; } = "Web";
    public string? IpAddress { get; private set; }
    public DateTime ClickedAt { get; private set; }

    public virtual Advertisement Advertisement { get; private set; } = null!;

    protected AdClick() { }

    public AdClick(int advertisementId, string platform, string? userId = null, string? ipAddress = null)
    {
        AdvertisementId = advertisementId;
        Platform = platform;
        UserId = userId;
        IpAddress = ipAddress;
        ClickedAt = DateTime.UtcNow;
    }
}

/// <summary>Aggregated daily statistics per advertisement.</summary>
public class AdStatistic : BaseEntity
{
    public int AdvertisementId { get; private set; }
    public DateTime Date { get; private set; }           // Day of the stat
    public int DailyImpressions { get; private set; }
    public int DailyClicks { get; private set; }
    public double DailyCTR => DailyImpressions > 0 ? (double)DailyClicks / DailyImpressions : 0;
    public double AdScore { get; private set; }          // Snapshot of AdScore that day

    public virtual Advertisement Advertisement { get; private set; } = null!;

    protected AdStatistic() { }

    public AdStatistic(int advertisementId, DateTime date)
    {
        AdvertisementId = advertisementId;
        Date = date.Date;
    }

    public void Tick(int impressions, int clicks, double adScore)
    {
        DailyImpressions = impressions;
        DailyClicks = clicks;
        AdScore = adScore;
        UpdatedAt = DateTime.UtcNow;
    }
}
