using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

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
    private readonly Khadamat.Application.Interfaces.IImageStorageService _imageStorage;

    public UploadController(Khadamat.Application.Interfaces.IImageStorageService imageStorage)
    {
        _imageStorage = imageStorage;
    }

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
        { "hero",            ""                },   // hero banners (saved directly in images/)
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
            var folderPath = _imageStorage.GetFolderPath(folder);

            var (maxWidth, maxHeight, targetQuality) = folder switch
            {
                "users" => (400, 400, 80),                                             // صور المستخدمين (15-35 KB)
                "maincategories" or "categories" or "subcategories" => (512, 512, 85), // أيقونات الفئات (25-50 KB)
                "hero" => (1920, 1080, 82),                                            // بنرات الصفحة الرئيسية
                _ => (1200, 1200, 80)                                                  // الخدمات والمتجر والإعلانات (70-180 KB)
            };

            var webpName = $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}_{Guid.NewGuid():N}.webp";
            var webpPath = Path.Combine(folderPath, webpName);

            using var inputStream = file.OpenReadStream();
            using var img = await Image.LoadAsync<Rgba32>(inputStream);

            // Resize if dimensions exceed threshold
            if (img.Width > maxWidth || img.Height > maxHeight)
            {
                img.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size = new Size(maxWidth, maxHeight),
                    Mode = ResizeMode.Max,
                    Sampler = KnownResamplers.Lanczos3
                }));
            }

            var webpEncoder = new WebpEncoder
            {
                FileFormat = WebpFileFormatType.Lossy,
                Quality = targetQuality,
                TransparentColorMode = WebpTransparentColorMode.Preserve,
                Method = WebpEncodingMethod.BestQuality
            };

            await using (var outStream = new FileStream(webpPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
            {
                await img.SaveAsync(outStream, webpEncoder);
            }

            // ضمان ألا يتعدى حجم الصورة النهائي 300 كيلوبايت (307,200 بايت)
            const long MaxOutputSizeBytes = 300 * 1024;
            var fileInfo = new FileInfo(webpPath);
            if (fileInfo.Exists && fileInfo.Length > MaxOutputSizeBytes)
            {
                // إعادة ضغط بجودة أقل لضمان عدم تجاوز 300KB
                var compressedEncoder = new WebpEncoder
                {
                    FileFormat = WebpFileFormatType.Lossy,
                    Quality = 65,
                    TransparentColorMode = WebpTransparentColorMode.Preserve,
                    Method = WebpEncodingMethod.BestQuality
                };

                await using (var outStream = new FileStream(webpPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
                {
                    await img.SaveAsync(outStream, compressedEncoder);
                }
            }

            return Ok(new
            {
                success  = true,
                filename = webpName,                          // ← يُخزن في قاعدة البيانات
                url      = $"/images/{folder}/{webpName}",   // ← رابط العرض
                type     = folder,
                sizeKb   = Math.Round(new FileInfo(webpPath).Length / 1024.0, 1),
                message  = "تم رفع ومعالجة الصورة بنجاح بصيغة WebP بحجم مثالي"
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
    /// <summary>
    /// Upload a hero banner image (saved as images/hero_banner.png, hero_banner2.png, etc.).
    /// POST /v1/upload/hero?slot=1  (slot = 1 or 2 or 3)
    /// Returns: { success, filename, url }
    /// </summary>
    [HttpPost("hero")]
    [Authorize(Policy = "RequireAdmin")]
    public async Task<IActionResult> UploadHeroImage(IFormFile file, [FromQuery] int slot = 1)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { success = false, message = "لم يتم إرسال ملف" });

        if (file.Length > MaxFileSizeBytes)
            return BadRequest(new { success = false, message = $"حجم الملف يتجاوز {MaxFileSizeBytes / 1024 / 1024} MB" });

        var ext = Path.GetExtension(file.FileName);
        if (!AllowedExtensions.Contains(ext))
            return BadRequest(new { success = false, message = "نوع الملف غير مدعوم" });

        if (slot < 1 || slot > 5)
            return BadRequest(new { success = false, message = "slot يجب أن يكون بين 1 و 5" });

        try
        {
            var folderPath = _imageStorage.GetFolderPath("");

            // Always save as hero_banner.png or hero_banner2.png etc.
            var filename = slot == 1 ? $"hero_banner{ext}" : $"hero_banner{slot}{ext}";
            var filePath = Path.Combine(folderPath, filename);

            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(new
            {
                success = true,
                filename = filename,
                url = $"/images/{filename}",
                slot = slot,
                message = $"تم رفع صورة Hero #{slot} بنجاح"
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[UploadController] Hero upload error: {ex.Message}");
            return StatusCode(500, new { success = false, message = "حدث خطأ أثناء رفع الصورة" });
        }
    }

    [HttpDelete]
    [Authorize(Policy = "RequireAdmin")]
    public IActionResult DeleteImage([FromQuery] string type, [FromQuery] string filename)
    {
        if (!AllowedTypes.TryGetValue(type, out var folder))
            return BadRequest(new { success = false, message = "نوع غير صحيح" });

        // Prevent path traversal
        if (filename.Contains("..") || filename.Contains("/") || filename.Contains("\\"))
            return BadRequest(new { success = false, message = "اسم الملف غير صحيح" });

        if (!_imageStorage.FileExists(folder, filename))
            return NotFound(new { success = false, message = "الملف غير موجود" });

        _imageStorage.DeleteFile(folder, filename);
        return Ok(new { success = true, message = "تم حذف الصورة" });
    }
}
