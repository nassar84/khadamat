using System;
using Khadamat.Domain.Common;

namespace Khadamat.Domain.Entities;

/// <summary>
/// Historical audit log for provider engagement and outreach across
/// WhatsApp (manual review + confirmation), Push Notifications, and In-App Alerts.
/// </summary>
public class ProviderEngagementLog : BaseEntity
{
    public int ServiceId { get; set; }
    public string? ProviderUserId { get; set; }
    public int? ProviderProfileId { get; set; }

    /// <summary>
    /// Channel used: "WhatsApp", "PushNotification", "InAppNotification"
    /// </summary>
    public string Channel { get; set; } = "WhatsApp";

    /// <summary>
    /// Type of reminder: CompleteBasicData, AddProfileOrLogo, AddWorkImages,
    /// CompleteDescription, CompleteLocation, UpdateContacts, UpdateProfile,
    /// WelcomeAfterRegistration, EncourageEngagement, Custom
    /// </summary>
    public string ReminderType { get; set; } = "WelcomeAfterRegistration";

    public string RecipientName { get; set; } = string.Empty;
    public string RecipientPhone { get; set; } = string.Empty;
    public string ProfessionalTitle { get; set; } = string.Empty;
    public string CityName { get; set; } = string.Empty;

    public int? CampaignId { get; set; }
    public string? CampaignName { get; set; }

    public string Title { get; set; } = string.Empty;
    public string MessageText { get; set; } = string.Empty;

    /// <summary>
    /// Status:
    /// "Prepared", "WhatsAppOpened", "SendConfirmed",
    /// "PushSent", "PushFailed", "InAppCreated",
    /// "ProviderReplied", "ImagesReceived", "DataCompleted", "DoNotContact"
    /// </summary>
    public string Status { get; set; } = "Prepared";

    public string? AdminUserId { get; set; }
    public string? AdminUserName { get; set; }

    public DateTime PreparedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SentConfirmedAt { get; set; }

    public string? DistinctionReason { get; set; }
    public bool IsDistinctionIncluded { get; set; } = false;

    public string? AdminNotes { get; set; }

    public virtual Service? Service { get; set; }
    public virtual MarketingCampaign? Campaign { get; set; }
}
