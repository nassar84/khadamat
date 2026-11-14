using System;
using Khadamat.Domain.Common;

namespace Khadamat.Domain.Entities;

/// <summary>
/// Configurable reminder template and scheduling rule for provider engagement.
/// Supports WhatsApp, Push Notifications, and In-App Notifications.
/// </summary>
public class ProviderEngagementReminder : BaseEntity
{
    /// <summary>
    /// Type of reminder:
    /// CompleteBasicData, AddProfileOrLogo, AddWorkImages, CompleteDescription,
    /// CompleteLocation, UpdateContacts, UpdateProfile, WelcomeAfterRegistration,
    /// EncourageEngagement, Custom
    /// </summary>
    public string ReminderType { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Default channel: "All", "WhatsApp", "PushOnly", "InAppOnly", "Both"
    /// </summary>
    public string TargetChannel { get; set; } = "All";

    /// <summary>
    /// Full template for WhatsApp messages
    /// </summary>
    public string WhatsAppTemplate { get; set; } = string.Empty;

    /// <summary>
    /// Short notification title for Push and In-App
    /// </summary>
    public string PushTitleTemplate { get; set; } = string.Empty;

    /// <summary>
    /// Notification body for Push and In-App
    /// </summary>
    public string PushBodyTemplate { get; set; } = string.Empty;

    /// <summary>
    /// Target internal deep link route e.g. "/service/{ServiceId}", "/provider/my-services"
    /// </summary>
    public string DeepLinkRoute { get; set; } = "/service/{ServiceId}";

    /// <summary>
    /// Scheduling interval: Days after service registration to trigger this reminder
    /// (e.g. 0 for immediate welcome, 3 for 3 days after, 7 for 7 days after)
    /// </summary>
    public int DaysAfterRegistration { get; set; } = 3;

    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } = 0;
}
