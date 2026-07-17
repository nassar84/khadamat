using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Khadamat.WebAPI.Controllers;

/// <summary>
/// Handles image/file uploads for all entity types.
///
/// === FOLDER STRUCTURE (wwwroot/images/) ===
///   maincategories/  → صور الفئات الرئيسية
///   categories/      → صور الفئات الفرعية (المستوى الثاني)
///   subcategories/   → صور التصنيفات الفرعية (المستوى الثالث)
///   services/        → صور الخدمات
///   ads/             → صور الإعلانات
///   marketplace/     → صور المتجر
///   users/           → صور المستخدمين (الصورة الشخصية)
///   placeholders/    → صور افتراضية ثابتة
///   defaults/        → صور default للأنواع المختلفة
///
/// === NAMING CONVENTION ===
///   {timestamp_ms}_{guid_n}{ext}
///   مثال: 1749754465123_a3f2b1c4d5e6f7a8b9c0d1e2f3a4b5c6.jpg
///
/// === DATABASE STORAGE ===
///   يُخزَّن في قاعدة البيانات اسم الملف فقط (بدون المجلد):
///   مثال: "1749754465123_a3f2b1c4d5e6f7a8b9c0d1e2f3a4b5c6.jpg"
///
/// === PATH RESOLUTION (BlazorUI / API) ===
///   ImagePathResolver.Service("filename.jpg")      → "images/services/filename.jpg"
///   ImagePathResolver.MainCategory("filename.jpg") → "images/maincategories/filename.jpg"
///   ImagePathResolver.Category("filename.jpg")     → "images/categories/filename.jpg"
///   ImagePathResolver.SubCategory("filename.jpg")  → "images/subcategories/filename.jpg"
///   ImagePathResolver.Ad("filename.jpg")           → "images/ads/filename.jpg"
///   ImagePathResolver.MarketplaceItem("filename.jpg") → "images/marketplace/filename.jpg"
///   ImagePathResolver.User("filename.jpg")         → "images/users/filename.jpg"
/// </summary>
[ApiController]
[Route("v1/upload")]
[Authorize]
public class UploadController : ControllerBase
{
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".jpg", ".jpeg", ".png", ".webp", ".gif" };

    /// <summary>
    /// Allowed upload types and their corresponding folder names.
    /// المجلد الفيزيائي = wwwroot/images/{folder}
    /// </summary>
    private static readonly Dictionary<string, string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        { "maincategories",  "maincategories"  },   // فئات رئيسية
        { "categories",      "categories"      },   // فئات فرعية
        { "subcategories",   "subcategories"   },   // تصنيفات فرعية
        { "services",        "services"        },   // خدمات
        { "ads",             "ads"             },   // إعلانات
        { "marketplace",     "marketplace"     },   // متجر
        { "users",           "users"           },   // مستخدمون
        { "general",         "general"         },   // عام
    };

    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10 MB

    /// <summary>
    /// Upload a single image file. Returns the filename ONLY (not the full path).
    ///
    /// POST /v1/upload?type=services
    /// POST /v1/upload?type=maincategories
    /// POST /v1/upload?type=categories
    /// POST /v1/upload?type=subcategories
    /// POST /v1/upload?type=ads
    /// POST /v1/upload?type=marketplace
    /// POST /v1/upload?type=users
    ///
    /// Response:
    /// {
    ///   "success": true,
    ///   "filename": "1749754465123_abc123.jpg",   ← save this in DB
    ///   "url": "/images/services/1749754465123_abc123.jpg",  ← full relative URL
    ///   "type": "services"
    /// }
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> UploadImage(IFormFile file, [FromQuery] string type = "services")
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { success = false, message = "لم يتم إرسال ملف" });

        if (file.Length > MaxFileSizeBytes)
            return BadRequest(new { success = false, message = $"حجم الملف يتجاوز الحد المسموح ({MaxFileSizeBytes / 1024 / 1024} MB)" });

        var ext = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(ext))
            return BadRequest(new { success = false, message = "نوع الملف غير مدعوم. المدعوم: jpg, jpeg, png, webp, gif" });

        // Validate and resolve folder
        if (!AllowedTypes.TryGetValue(type, out var folder))
        {
            return BadRequest(new { success = false, message = $"نوع الرفع غير مدعوم: {type}. المدعوم: {string.Join(", ", AllowedTypes.Keys)}" });
        }

        try
        {
            var basePath = Directory.GetCurrentDirectory();
            var folderPath = Path.Combine(basePath, "wwwroot", "images", folder);

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            // Naming convention: {timestamp_ms}_{guid_compact}{ext}
            var uniqueName = $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{Guid.NewGuid():N}{ext}";
            var filePath = Path.Combine(folderPath, uniqueName);

            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
            {
                await file.CopyToAsync(stream);
            }

            // ✅ Return ONLY the filename → save this in DB
            // ✅ Also return the full relative URL for immediate display
            return Ok(new
            {
                success  = true,
                filename = uniqueName,                          // ← store in DB
                url      = $"/images/{folder}/{uniqueName}",   // ← use for <img src>
                type     = folder,
                message  = "تم رفع الصورة بنجاح"
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[UploadController] Error saving file: {ex.Message}");
            return StatusCode(500, new { success = false, message = "حدث خطأ أثناء رفع الملف" });
        }
    }

    /// <summary>
    /// Delete an uploaded image by filename and type.
    /// DELETE /v1/upload?type=services&filename=1749754465123_abc123.jpg
    /// </summary>
    [HttpDelete]
    [Authorize(Policy = "RequireAdmin")]
    public IActionResult DeleteImage([FromQuery] string type, [FromQuery] string filename)
    {
        if (!AllowedTypes.TryGetValue(type, out var folder))
            return BadRequest(new { success = false, message = "نوع غير صحيح" });

        // Prevent path traversal
        if (filename.Contains("..") || filename.Contains("/") || filename.Contains("\\"))
            return BadRequest(new { success = false, message = "اسم الملف غير صحيح" });

        var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", folder, filename);

        if (!System.IO.File.Exists(filePath))
            return NotFound(new { success = false, message = "الملف غير موجود" });

        System.IO.File.Delete(filePath);
        return Ok(new { success = true, message = "تم حذف الصورة" });
    }
}
