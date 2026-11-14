using System;
using System.Collections.Generic;

namespace Khadamat.Application.DTOs;

public class EngagementKpiDto
{
    public int TotalServices { get; set; }
    public int IncompleteServices { get; set; }
    public int CompleteServices { get; set; }
    public int NeedsImages { get; set; }
    public int NeedsDescription { get; set; }
    public int NeedsLocation { get; set; }
    public int NeedsContacts { get; set; }
    public int ConfirmedWhatsAppSent { get; set; }
    public int PushNotificationsSent { get; set; }
    public int InAppNotificationsSent { get; set; }
    public int CompletedAfterEngagement { get; set; }
    public double AverageCompletenessPercentage { get; set; }
}

public class ServiceEngagementSummaryDto
{
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string? ProviderUserId { get; set; }
    public int ProviderProfileId { get; set; }
    public string? Phone1 { get; set; }
    public string? WhatsApp { get; set; }
    public string FormattedPhone { get; set; } = string.Empty;
    public int? CityId { get; set; }
    public string CityName { get; set; } = string.Empty;
    public int? GovernorateId { get; set; }
    public string GovernorateName { get; set; } = string.Empty;
    public string? CategoryName { get; set; }
    public string? Specialization { get; set; }
    public string DetectedTitle { get; set; } = "أستاذ";
    
    public int CompletenessPercentage { get; set; }
    public List<string> MissingFields { get; set; } = new();
    public bool HasMainImage { get; set; }
    public int WorkImagesCount { get; set; }
    public bool HasValidLocation { get; set; }
    public bool HasValidDescription { get; set; }
    public bool HasValidContacts { get; set; }
    
    // Distinction metadata
    public string DistinctionReason { get; set; } = "None";
    public string DistinctionReasonTitle { get; set; } = "خدمة عادية";
    public bool IsDistinctionEnabled { get; set; } = true;
    public double AverageRating { get; set; } = 0;
    public int RatingsCount { get; set; } = 0;
    public bool HasRealReviews => RatingsCount > 0 && AverageRating >= 4.0;
    
    public DateTime? LastContactDate { get; set; }
    public int ContactAttemptsCount { get; set; }
    public string LastContactStatus { get; set; } = "لم يتم التواصل";
    public string? LastContactChannel { get; set; }
    public bool DoNotContact { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ServiceCompletenessResultDto
{
    public int Percentage { get; set; }
    public bool IsComplete => Percentage >= 90;
    public List<string> MissingFields { get; set; } = new();
    public Dictionary<string, bool> Checklist { get; set; } = new();
}

public class PreparedEngagementMessageDto
{
    public int ServiceId { get; set; }
    public string RecipientName { get; set; } = string.Empty;
    public string RecipientPhone { get; set; } = string.Empty;
    public string ProfessionalTitle { get; set; } = string.Empty;
    public string PersonalizedGreeting { get; set; } = string.Empty;
    public string CityName { get; set; } = string.Empty;
    public string GovernorateName { get; set; } = string.Empty;
    public int? CampaignId { get; set; }
    public string CampaignName { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string FullMessageText { get; set; } = string.Empty;
    public string WhatsAppClickUrl { get; set; } = string.Empty;
    public string InAppTitle { get; set; } = string.Empty;
    public string InAppBody { get; set; } = string.Empty;
    public string DeepLinkRoute { get; set; } = string.Empty;
    public bool HasActivePushDevice { get; set; }
    public int CompletenessPercentage { get; set; }
    public List<string> MissingFields { get; set; } = new();
    public bool DoNotContact { get; set; }

    // Distinction Paragraph Properties
    public string DistinctionReason { get; set; } = "None";
    public string DistinctionReasonTitle { get; set; } = "خدمة عادية";
    public bool IsDistinctionEnabled { get; set; } = true;
    public string DistinctionParagraph { get; set; } = string.Empty;
    public int RatingsCount { get; set; } = 0;
    public double AverageRating { get; set; } = 0;
    public bool HasRealReviews { get; set; } = false;
    public bool HasPositiveReviews { get => HasRealReviews || RatingsCount > 0; set => HasRealReviews = value; }
    public int PositiveReviewsCount { get => RatingsCount; set => RatingsCount = value; }
}

public class SendEngagementRequestDto
{
    public int ServiceId { get; set; }
    public string Channel { get; set; } = "WhatsApp"; // "WhatsApp", "PushNotification", "InAppNotification"
    public string ReminderType { get; set; } = "WelcomeAfterRegistration";
    public string? ProfessionalTitle { get; set; }
    public string? CityName { get; set; }
    public int? CampaignId { get; set; }
    public string? CustomTitle { get; set; }
    public string? CustomMessage { get; set; }
    public string? AdminNotes { get; set; }

    // Distinction Override
    public string? DistinctionReason { get; set; }
    public bool? IsDistinctionEnabled { get; set; }
    public bool IsDistinctionIncluded { get => IsDistinctionEnabled ?? false; set => IsDistinctionEnabled = value; }
    public string? DistinctionNotes { get; set; }
}

public class EngagementLogDto
{
    public int Id { get; set; }
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string RecipientName { get; set; } = string.Empty;
    public string RecipientPhone { get; set; } = string.Empty;
    public string ProfessionalTitle { get; set; } = string.Empty;
    public string CityName { get; set; } = string.Empty;
    public string Channel { get; set; } = string.Empty;
    public string ReminderType { get; set; } = string.Empty;
    public string ReminderTypeTitle { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string MessageText { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string StatusArabic { get; set; } = string.Empty;
    public string? AdminUserName { get; set; }
    public DateTime PreparedAt { get; set; }
    public DateTime? SentConfirmedAt { get; set; }
    public string? DistinctionReason { get; set; }
    public bool IsDistinctionIncluded { get; set; }
    public string? AdminNotes { get; set; }
}

public class MarketingCampaignDto
{
    public int Id { get; set; }
    public string CampaignName { get; set; } = string.Empty;
    public int? CityId { get; set; }
    public string CityName { get; set; } = string.Empty;
    public int? GovernorateId { get; set; }
    public string GovernorateName { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsDefault { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Slogan { get; set; } = string.Empty;
    public string MessageTemplate { get; set; } = string.Empty;
    public int SubscriptionOfferMonths { get; set; } = 3;
    public string SubscriptionOfferText { get; set; } = string.Empty;
    public string? AppDownloadUrl { get; set; }
    public string FacebookCommunityUrl { get; set; } = string.Empty;
    public string TargetStage { get; set; } = "Launch";
}

public class ProfessionalTitleRuleDto
{
    public int Id { get; set; }
    public string ProfessionKeyword { get; set; } = string.Empty;
    public string MaleTitle { get; set; } = string.Empty;
    public string FemaleTitle { get; set; } = string.Empty;
    public string DefaultTitle { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int Priority { get; set; }
    public string? Notes { get; set; }
}

public class ProviderEngagementReminderDto
{
    public int Id { get; set; }
    public string ReminderType { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string TargetChannel { get; set; } = "All";
    public string WhatsAppTemplate { get; set; } = string.Empty;
    public string PushTitleTemplate { get; set; } = string.Empty;
    public string PushBodyTemplate { get; set; } = string.Empty;
    public string DeepLinkRoute { get; set; } = string.Empty;
    public int DaysAfterRegistration { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
}

public class ServiceDistinctionConfigDto
{
    public int Id { get; set; }
    public bool IsEnabledGlobally { get; set; } = true;
    public string HeaderText { get; set; } = "🌟 *خبر مميز ليك!*";
    public string AdminSelectionText { get; set; } = "تم اختيار خدمتك وإضافتها ضمن الخدمات المميزة على تطبيق خدماوي";
    public string CommunityNominationText { get; set; } = "تم ترشيح خدمتك من أعضاء مجتمع خدماوي لتكون ضمن الخدمات المميزة";
    public string PositiveReviewsText { get; set; } = "خدمتك من الخدمات التي حصلت على تقييمات إيجابية من مستخدمي خدماوي";
    public string SpecialPromotionText { get; set; } = "تم اختيار خدمتك ضمن الاختيارات الترويجية المميزة على خدماوي";
    public string GeneralWelcomeText { get; set; } = "سعداء بانضمامك لمنصة خدماوي لتوصيل خدماتك لأهل بلدك ومساعدتك في الوصول لعملاء جدد في منطقتك! 🚀";
    public string FollowUpText { get; set; } = "إحنا بنهتم نعرّف أهل بلدك بأصحاب الخدمات المميزة، ونساعد شغلك يوصل للناس اللي بتدور على الخدمة اللي بتقدمها.\n\nوجودك معانا خطوة جديدة علشان شغلك ينتشر ويكون أسهل للعملاء إنهم يلاقوك ويتواصلوا معاك! 🚀";
}
