using System;

namespace Khadamat.Domain.Entities;

/// <summary>
/// إعدادات وحالة تكامل صفحة فيسبوك الرسمية لتطبيق خدماوي
/// </summary>
public class FacebookIntegrationConfig : BaseEntity
{
    public string AppId { get; set; } = "1546767603438579";
    public string PageName { get; set; } = "خدماوى | Khadamawy";
    public string PageUsername { get; set; } = "KhadamawyApp";
    public string PageUrl { get; set; } = "https://www.facebook.com/khadamawyApp/";
    
    /// <summary>
    /// معرف الصفحة الحقيقي المستخرج من Meta Graph API
    /// </summary>
    public string? PageId { get; set; }

    /// <summary>
    /// رمز وصول الصفحة المشفر (Page Access Token)
    /// لا يتم حفظه بنص صريح أبداً في قاعدة البيانات
    /// </summary>
    public string? ProtectedPageAccessToken { get; set; }

    /// <summary>
    /// هل الصفحة مربوطة حالياً وتعمل
    /// </summary>
    public bool IsConnected { get; set; } = false;

    public DateTime? ConnectedAt { get; set; }
    public string? ConnectedByAdminUserId { get; set; }
    public string? ConnectedByAdminName { get; set; }

    /// <summary>
    /// الصلاحيات الممنوحة من المسؤول (e.g. pages_show_list, pages_read_engagement, pages_manage_posts)
    /// </summary>
    public string? PermissionsGranted { get; set; }

    /// <summary>
    /// المهام المتاحة للمسؤول على الصفحة (e.g. CREATE_CONTENT, MANAGE)
    /// </summary>
    public string? PageTasks { get; set; }

    /// <summary>
    /// وضع النشر: ManualOnly (يدوي فقط), ReviewBeforePublish (مراجعة ثم نشر), AutomaticAfterApproval (تلقائي بعد الاعتماد)
    /// </summary>
    public string PublishMode { get; set; } = "ManualOnly";

    /// <summary>
    /// تفعيل النشر التلقائي بعد اعتماد الخدمة (افتراضياً معطل للأمان)
    /// </summary>
    public bool AutoPublishOnApproval { get; set; } = false;

    /// <summary>
    /// نتائج آخر فحص اتصال
    /// </summary>
    public DateTime? LastConnectionCheck { get; set; }
    public bool LastConnectionSuccess { get; set; }
    public string? LastConnectionError { get; set; }
}
