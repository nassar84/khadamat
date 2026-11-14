using System;

namespace Khadamat.Domain.Entities;

/// <summary>
/// سجل المنشورات المنشورة أو المجدولة للنشر على صفحة فيسبوك الرسمية
/// </summary>
public class FacebookPublishLog : BaseEntity
{
    public int? ServiceId { get; set; }
    public virtual Service? Service { get; set; }

    /// <summary>
    /// نوع المنشور: ServiceCard (كارت خدمة), PhotoPost (صورة), TextPost (نص فقط)
    /// </summary>
    public string PostType { get; set; } = "ServiceCard";

    public string? FacebookPageId { get; set; }

    /// <summary>
    /// معرف المنشور على فيسبوك بعد نجاح النشر (Post ID or Photo ID)
    /// </summary>
    public string? FacebookPostId { get; set; }

    /// <summary>
    /// رابط المنشور على صفحة فيسبوك
    /// </summary>
    public string? PostUrl { get; set; }

    /// <summary>
    /// نص المنشور المعتمد
    /// </summary>
    public string PostText { get; set; } = string.Empty;

    /// <summary>
    /// رابط الصورة المرفقة إن وجدت
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// حالة النشر: Draft (مسودة), PendingReview (بانتظار المراجعة), InProgress (جاري النشر), Published (تم النشر), Failed (فشل), Cancelled (ملغي)
    /// </summary>
    public string Status { get; set; } = "PendingReview";

    public DateTime? AttemptedAt { get; set; }
    public DateTime? PublishedAt { get; set; }

    public int RetryCount { get; set; } = 0;
    public string? ErrorMessage { get; set; }

    public string? AdminUserId { get; set; }
    public string? AdminUserName { get; set; }
    public string? AdminNotes { get; set; }
}
