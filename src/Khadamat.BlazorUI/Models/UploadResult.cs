namespace Khadamat.BlazorUI.Models;

/// <summary>
/// نتيجة رفع الصورة من API endpoint: POST /v1/upload
///
/// الاستخدام الصحيح:
///   var result = await Api.UploadImageAsync(file, EntityImageType.Service);
///   if (result != null)
///   {
///       service.ImageUrl = result.Filename;  // ← يُخزَّن في قاعدة البيانات
///       // لا تخزن result.Url في قاعدة البيانات
///   }
///
/// في العرض:
///   <img src="@ImagePathResolver.Service(service.ImageUrl)" />
/// </summary>
public class UploadResult
{
    /// <summary>
    /// هل نجح الرفع؟
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// اسم الملف فقط — هذا ما يُخزَّن في قاعدة البيانات.
    /// مثال: "1749754465123_a3f2b1c4d5e6f7a8b9c0.jpg"
    /// </summary>
    public string? Filename { get; set; }

    /// <summary>
    /// المسار النسبي الكامل — يُستخدم في img src مباشرة.
    /// مثال: "/images/services/1749754465123_a3f2b1c4d5e6f7a8b9c0.jpg"
    /// </summary>
    public string? Url { get; set; }

    /// <summary>
    /// نوع الكيان المرفوعة له الصورة (services, users, ads, ...)
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// رسالة الاستجابة من الخادم
    /// </summary>
    public string? Message { get; set; }
}
