using System;
using Khadamat.Domain.Common;

namespace Khadamat.Domain.Entities;

/// <summary>
/// Defines a local city marketing campaign (e.g. سرس الليان launch campaign).
/// Allows rolling out city-specific messaging, offers, and community links dynamically.
/// </summary>
public class MarketingCampaign : BaseEntity
{
    public string CampaignName { get; set; } = string.Empty;
    public int? CityId { get; set; }
    public string CityName { get; set; } = string.Empty;
    public int? GovernorateId { get; set; }
    public string GovernorateName { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
    public bool IsDefault { get; set; } = false; // Fallback template when no city campaign is active

    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }

    /// <summary>
    /// Marketing slogan e.g. "خدماوي.. خدمات بلدنا في مكان واحد ❤️"
    /// </summary>
    public string Slogan { get; set; } = "خدماوي.. خدمات بلدنا في مكان واحد ❤️";

    /// <summary>
    /// Local marketing message template with dynamic placeholders:
    /// {PersonalizedGreeting}, {ProviderName}, {ProfessionalTitle}, {ServiceName},
    /// {CityName}, {GovernorateName}, {ServiceUrl}, {AppDownloadUrl}, {FacebookPageUrl}
    /// </summary>
    public string MessageTemplate { get; set; } = string.Empty;

    /// <summary>
    /// Subscription offer duration in months (e.g. 3 months free)
    /// </summary>
    public int SubscriptionOfferMonths { get; set; } = 3;
    public string SubscriptionOfferText { get; set; } = "اشتراك مجاني لأول 3 شهور! ❤️";

    public string? AppDownloadUrl { get; set; }
    public string FacebookCommunityUrl { get; set; } = "https://www.facebook.com/profile.php?id=61595461124579";

    /// <summary>
    /// Campaign stage: "Launch" (e.g. سرس الليان), "Expansion" (e.g. شبين الكوم), "General"
    /// </summary>
    public string TargetStage { get; set; } = "Launch";

    public virtual City? City { get; set; }
    public virtual Governorate? Governorate { get; set; }
}
