using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Khadamat.Application.Features.Services.Queries;
using System.Text;
using System.Web;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace Khadamat.WebAPI.Controllers;

/// <summary>
/// Returns static HTML pages with Open Graph meta tags for social media scrapers.
/// Facebook, WhatsApp, Telegram bots crawl these pages to generate rich link previews.
/// </summary>
[ApiController]
[Route("share")]
public class ShareController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public ShareController(IMediator mediator, IConfiguration configuration, IWebHostEnvironment webHostEnvironment)
    {
        _mediator = mediator;
        _configuration = configuration;
        _webHostEnvironment = webHostEnvironment;
    }

    /// <summary>
    /// Returns an HTML page with Open Graph tags for a service.
    /// Use this URL as the Facebook/Telegram share target instead of the SPA route.
    /// </summary>
    [HttpGet("service/{id:int}")]
    public async Task<IActionResult> ShareService(int id)
    {
        var service = await _mediator.Send(new GetServiceByIdQuery(id));

        if (service == null)
            return NotFound();

        // Determine base URL dynamically, preferring forwarded headers or configuration
        string? baseUrl = _configuration["ApiSettings:WebAppBaseUrl"];
        if (string.IsNullOrEmpty(baseUrl) || baseUrl.Contains("localhost"))
        {
            // Fallback to proxy headers
            var scheme = Request.Headers["X-Forwarded-Proto"].FirstOrDefault() ?? Request.Scheme;
            var host = Request.Headers["X-Forwarded-Host"].FirstOrDefault() ?? Request.Host.ToString();
            baseUrl = $"{scheme}://{host}";
        }
        
        baseUrl = baseUrl.TrimEnd('/');

        // If the URL still points to localhost but we are in production, try to resolve from Cors
        if (baseUrl.Contains("localhost") || baseUrl.Contains("127.0.0.1") || baseUrl.Contains("::1"))
        {
            var allowedOrigins = _configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
            var publicOrigin = allowedOrigins?.FirstOrDefault(o => o.StartsWith("https://") && !o.Contains("localhost"));
            if (!string.IsNullOrEmpty(publicOrigin))
            {
                baseUrl = publicOrigin.TrimEnd('/');
            }
        }

        // Force HTTPS in production
        if (baseUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase) && 
            !baseUrl.Contains("localhost") && !baseUrl.Contains("127.0.0.1") && !baseUrl.Contains("::1"))
        {
            baseUrl = "https://" + baseUrl.Substring(7);
        }

        // Build the canonical SPA URL that users land on after clicking the shared link
        var serviceUrl = $"{baseUrl}/service/{id}";
        var shareUrl = $"{baseUrl}/share/service/{id}";

        // Check if a pre-rendered premium card image exists for this service (prefer JPEG for size/WhatsApp compatibility, fallback to PNG)
        string imageUrl = $"{baseUrl}/images/logo.png"; // safe default — always assigned
        var basePath = _webHostEnvironment.WebRootPath ?? Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot");
        var cardRelativePathJpg = $"images/share_cards/card_{id}.jpg";
        var cardPhysicalPathJpg = Path.Combine(basePath, "images", "share_cards", $"card_{id}.jpg");
        var cardRelativePathPng = $"images/share_cards/card_{id}.png";
        var cardPhysicalPathPng = Path.Combine(basePath, "images", "share_cards", $"card_{id}.png");

        if (System.IO.File.Exists(cardPhysicalPathJpg))
        {
            var lastWrite = System.IO.File.GetLastWriteTimeUtc(cardPhysicalPathJpg).Ticks;
            imageUrl = $"{baseUrl}/{cardRelativePathJpg}?v={lastWrite}";
        }
        else if (System.IO.File.Exists(cardPhysicalPathPng))
        {
            var lastWrite = System.IO.File.GetLastWriteTimeUtc(cardPhysicalPathPng).Ticks;
            imageUrl = $"{baseUrl}/{cardRelativePathPng}?v={lastWrite}";
        }
        else
        {
            // Pick the best image: real uploaded images or category icon
            var firstImg = service.Images?.FirstOrDefault();
            bool isRealImage = !string.IsNullOrEmpty(firstImg) &&
                               !firstImg.Contains("/gen/") && !firstImg.Contains("/placeholders/");

            if (isRealImage)
            {
                if (firstImg!.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                    imageUrl = firstImg;
                else if (firstImg.StartsWith("/images/", StringComparison.OrdinalIgnoreCase))
                    imageUrl = $"{baseUrl}{firstImg}";
                else if (firstImg.StartsWith("images/", StringComparison.OrdinalIgnoreCase))
                    imageUrl = $"{baseUrl}/{firstImg}";
                else // plain filename (e.g. "s_2_8.jpg") — real uploaded service image
                    imageUrl = $"{baseUrl}/images/services/{firstImg}";
            }
            else
            {
                // Prioritize database-stored category imagery
                if (!string.IsNullOrEmpty(service.SubCategoryImageUrl))
                {
                    imageUrl = service.SubCategoryImageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                        ? service.SubCategoryImageUrl
                        : $"{baseUrl}/images/subcategories/{service.SubCategoryImageUrl.TrimStart('/')}";
                }
                else if (!string.IsNullOrEmpty(service.CategoryImageUrl))
                {
                    imageUrl = service.CategoryImageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                        ? service.CategoryImageUrl
                        : $"{baseUrl}/images/categories/{service.CategoryImageUrl.TrimStart('/')}";
                }
                else if (!string.IsNullOrEmpty(service.MainCategoryImageUrl))
                {
                    imageUrl = service.MainCategoryImageUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)
                        ? service.MainCategoryImageUrl
                        : $"{baseUrl}/images/maincategories/{service.MainCategoryImageUrl.TrimStart('/')}";
                }
                else
                {
                    // Try ID-based category image (files exist as c_{id}_{id}.png)
                    // Fallback to logo.png for a branded look instead of generic placeholder
                    bool foundCategoryImg = false;
                    if (service.SubCategoryId.HasValue && service.CategoryId.HasValue)
                    {
                        var catPath = Path.Combine(basePath, "images", "categories", $"c_{service.CategoryId}_{service.SubCategoryId}.png");
                        if (System.IO.File.Exists(catPath))
                        {
                            imageUrl = $"{baseUrl}/images/categories/c_{service.CategoryId}_{service.SubCategoryId}.png";
                            foundCategoryImg = true;
                        }
                    }
                    if (!foundCategoryImg && service.CategoryId.HasValue && service.MainCategoryId > 0)
                    {
                        var catPath = Path.Combine(basePath, "images", "categories", $"c_{service.MainCategoryId}_{service.CategoryId}.png");
                        if (System.IO.File.Exists(catPath))
                        {
                            imageUrl = $"{baseUrl}/images/categories/c_{service.MainCategoryId}_{service.CategoryId}.png";
                            foundCategoryImg = true;
                        }
                    }
                    if (!foundCategoryImg)
                    {
                        // Use branded logo for best social preview when no service image exists
                        var logoPath = Path.Combine(basePath, "images", "logo.png");
                        imageUrl = System.IO.File.Exists(logoPath)
                            ? $"{baseUrl}/images/logo.png"
                            : $"{baseUrl}/images/defaults/default_service.png";
                    }
                }
            }
        }

        var safeTitle = HttpUtility.HtmlEncode(service.Title);
        var fullCategoryPath = !string.IsNullOrEmpty(service.SubCategoryName)
            ? $"{service.MainCategoryName} > {service.CategoryName} > {service.SubCategoryName}"
            : $"{service.MainCategoryName} > {service.CategoryName}";
        var categoryPath = HttpUtility.HtmlEncode(fullCategoryPath);
        var location = HttpUtility.HtmlEncode($"{service.GovernorateName} - {service.CityName}");
        var priceText = service.Price.HasValue ? $"{service.Price:N0} ج.م" : "مجاناً";
        var safeAddress = !string.IsNullOrEmpty(service.Address) ? HttpUtility.HtmlEncode(service.Address) : "تواصل لمعرفة التفاصيل";
        var safeWorkDays = !string.IsNullOrEmpty(service.WorkDays) ? HttpUtility.HtmlEncode(service.WorkDays) : "طوال أيام الأسبوع";
        var safeWorkHours = !string.IsNullOrEmpty(service.WorkHours) ? HttpUtility.HtmlEncode(service.WorkHours) : "مرن / تواصل للحجز";
        var safeProviderName = !string.IsNullOrEmpty(service.ProviderName) ? HttpUtility.HtmlEncode(service.ProviderName) : "مزود خدمة خدماوي";
        var safeDescFull = HttpUtility.HtmlEncode(service.Description ?? "");

        // Build contact buttons
        var contactButtonsHtml = new StringBuilder();
        if (!string.IsNullOrEmpty(service.WhatsApp))
        {
            var digits = new string(service.WhatsApp.Where(char.IsDigit).ToArray());
            if (digits.StartsWith("01") && digits.Length == 11) digits = "2" + digits;
            var waUrl = $"https://wa.me/{digits}?text=" + HttpUtility.UrlEncode($"مرحباً {service.ProviderName}، بخصوص الخدمة المعروضة: {service.Title}");
            contactButtonsHtml.AppendLine($"      <a href=\"{waUrl}\" target=\"_blank\" class=\"btn-contact btn-whatsapp\"><span class=\"btn-icon\">💬</span> واتساب: {HttpUtility.HtmlEncode(service.WhatsApp)}</a>");
        }
        if (!string.IsNullOrEmpty(service.Phone1))
        {
            contactButtonsHtml.AppendLine($"      <a href=\"tel:{service.Phone1}\" class=\"btn-contact btn-phone\"><span class=\"btn-icon\">📞</span> اتصل: {HttpUtility.HtmlEncode(service.Phone1)}</a>");
        }
        if (!string.IsNullOrEmpty(service.Phone2))
        {
            contactButtonsHtml.AppendLine($"      <a href=\"tel:{service.Phone2}\" class=\"btn-contact btn-phone\"><span class=\"btn-icon\">📞</span> اتصل (إضافي): {HttpUtility.HtmlEncode(service.Phone2)}</a>");
        }
        if (!string.IsNullOrEmpty(service.Telegram))
        {
            var tgUser = service.Telegram.Replace("@", "").Trim();
            contactButtonsHtml.AppendLine($"      <a href=\"https://t.me/{tgUser}\" target=\"_blank\" class=\"btn-contact btn-telegram\"><span class=\"btn-icon\">✈️</span> تليجرام</a>");
        }
        if (!string.IsNullOrEmpty(service.Facebook))
        {
            var fbUrl = service.Facebook.StartsWith("http") ? service.Facebook : $"https://facebook.com/{service.Facebook}";
            contactButtonsHtml.AppendLine($"      <a href=\"{fbUrl}\" target=\"_blank\" class=\"btn-contact btn-social\"><span class=\"btn-icon\">📘</span> فيسبوك</a>");
        }

        // Contact info summary
        var contactLines = new List<string>();
        if (!string.IsNullOrEmpty(service.Phone1)) contactLines.Add($"📞 {service.Phone1}");
        if (!string.IsNullOrEmpty(service.Phone2)) contactLines.Add($"📞 {service.Phone2}");
        if (!string.IsNullOrEmpty(service.WhatsApp)) contactLines.Add($"💬 {service.WhatsApp}");
        var contactStr = string.Join(" | ", contactLines);

        // Short description (trim for OG tag — Facebook ~300 chars)
        var shortDesc = service.Description ?? "";
        if (shortDesc.Length > 120) shortDesc = shortDesc[..117] + "...";
        var safeDesc = HttpUtility.HtmlEncode(shortDesc);

        // OG description — Must be PROMOTIONAL (not raw data) for social sharing appeal
        // WhatsApp/Facebook show this text under the title — make it enticing!
        var cleanTitle = service.Title.Replace("\"", "").Replace("'", "");
        var ogDesc = $"تم مشاركة هذه الخدمة من تطبيق وموقع خدماوي 📲 | {cleanTitle} | 📍 {location} | 💰 {priceText}. {(shortDesc.Length > 100 ? shortDesc[..97] + "..." : shortDesc)} — حمل تطبيق خدماوي مجاناً الآن لتصفح آلاف الخدمات والتواصل مع مقدميها مباشرة وبدون عمولات!";
        if (ogDesc.Length > 280) ogDesc = ogDesc[..277] + "...";
        var safeOgDesc = HttpUtility.HtmlEncode(ogDesc);

        // Page title (also used as og:title)
        var pageTitle = HttpUtility.HtmlEncode($"{service.Title} • {service.CategoryName} في {service.CityName}");

        // App download URLs
        var appStoreUrl = $"{baseUrl}/downloads/khadamat.apk";

        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html lang=\"ar\" dir=\"rtl\">");
        html.AppendLine("<head>");
        html.AppendLine("  <meta charset=\"UTF-8\" />");
        html.AppendLine("  <meta name=\"build-version\" content=\"v2.0.0-fix-20260626\" />");
        html.AppendLine($"  <title>{pageTitle}</title>");

        // === Standard Open Graph tags (Facebook, LinkedIn, Discord) ===
        html.AppendLine("  <meta property=\"og:type\"        content=\"website\" />");
        html.AppendLine($"  <meta property=\"og:url\"         content=\"{shareUrl}\" />");
        html.AppendLine($"  <meta property=\"og:title\"       content=\"{pageTitle}\" />");
        html.AppendLine($"  <meta property=\"og:description\" content=\"{safeOgDesc}\" />");
        html.AppendLine($"  <meta property=\"og:image\"       content=\"{imageUrl}\" />");
        html.AppendLine($"  <meta property=\"og:image:secure_url\" content=\"{imageUrl}\" />");
        html.AppendLine("  <meta property=\"og:image:width\"  content=\"1200\" />");
        html.AppendLine("  <meta property=\"og:image:height\" content=\"630\" />");
        html.AppendLine("  <meta property=\"og:locale\"      content=\"ar_EG\" />");
        html.AppendLine("  <meta property=\"og:site_name\"   content=\"خدماوي\" />");

        // === Facebook specific ===
        html.AppendLine("  <meta property=\"fb:app_id\"      content=\"\" />");

        // === Twitter Card tags ===
        html.AppendLine("  <meta name=\"twitter:card\"        content=\"summary_large_image\" />");
        html.AppendLine($"  <meta name=\"twitter:title\"       content=\"{pageTitle}\" />");
        html.AppendLine($"  <meta name=\"twitter:description\" content=\"{safeOgDesc}\" />");
        html.AppendLine($"  <meta name=\"twitter:image\"       content=\"{imageUrl}\" />");

        // === SEO ===
        html.AppendLine($"  <meta name=\"description\" content=\"{safeOgDesc}\" />");
        html.AppendLine($"  <link rel=\"canonical\" href=\"{shareUrl}\" />");

        // === JSON-LD Structured Data ===
        var jsonParts = new List<string>
        {
            "\"@context\": \"https://schema.org\"",
            "\"@type\": \"Service\"",
            $"\"name\": \"{HttpUtility.HtmlEncode(service.Title)}\"",
            $"\"description\": \"{HttpUtility.HtmlEncode(shortDesc)}\"",
            $"\"provider\": {{ \"@type\": \"LocalBusiness\", \"name\": \"{HttpUtility.HtmlEncode(service.ProviderName ?? service.Title)}\" }}",
            $"\"areaServed\": \"{location}\"",
            $"\"image\": \"{imageUrl}\"",
            $"\"url\": \"{serviceUrl}\""
        };
        if (service.Price.HasValue)
            jsonParts.Add($"\"offers\": {{ \"@type\": \"Offer\", \"price\": \"{service.Price.Value:F2}\", \"priceCurrency\": \"EGP\" }}");
        if (!string.IsNullOrEmpty(service.Phone1))
            jsonParts.Add($"\"telephone\": \"{service.Phone1}\"");
        html.AppendLine("<script type=\"application/ld+json\">");
        html.AppendLine("{" + string.Join(",", jsonParts.Select(p => "\n  " + p)) + "\n");
        html.AppendLine("}");
        html.AppendLine("</script>");

        // Google Fonts & Styling
        html.AppendLine("  <link rel=\"preconnect\" href=\"https://fonts.googleapis.com\">");
        html.AppendLine("  <link rel=\"preconnect\" href=\"https://fonts.gstatic.com\" crossorigin>");
        html.AppendLine("  <link href=\"https://fonts.googleapis.com/css2?family=Tajawal:wght@400;500;700;800;900&display=swap\" rel=\"stylesheet\">");
        
        html.AppendLine("  <style>");
        html.AppendLine("    * { box-sizing: border-box; }");
        html.AppendLine("    body {");
        html.AppendLine("      font-family: 'Tajawal', sans-serif;");
        html.AppendLine("      background: linear-gradient(135deg, #0f172a 0%, #1e293b 60%, #1d6070 100%);");
        html.AppendLine("      min-height: 100vh;");
        html.AppendLine("      margin: 0;");
        html.AppendLine("      display: flex;");
        html.AppendLine("      align-items: center;");
        html.AppendLine("      justify-content: center;");
        html.AppendLine("      padding: 20px;");
        html.AppendLine("      direction: rtl;");
        html.AppendLine("    }");
        html.AppendLine("    .card-container {");
        html.AppendLine("      max-width: 580px;");
        html.AppendLine("      width: 100%;");
        html.AppendLine("      background: #ffffff;");
        html.AppendLine("      border-radius: 24px;");
        html.AppendLine("      box-shadow: 0 32px 64px rgba(0,0,0,0.35);");
        html.AppendLine("      overflow: hidden;");
        html.AppendLine("    }");
        html.AppendLine("    /* ── Hero Image Banner ── */");
        html.AppendLine("    .hero-banner {");
        html.AppendLine("      width: 100%;");
        html.AppendLine("      height: 220px;");
        html.AppendLine("      position: relative;");
        html.AppendLine("      overflow: hidden;");
        html.AppendLine("      background: #0f172a;");
        html.AppendLine("    }");
        html.AppendLine("    .hero-banner .bg-blur {");
        html.AppendLine("      position: absolute; inset: 0;");
        html.AppendLine("      width: 100%; height: 100%;");
        html.AppendLine("      object-fit: cover;");
        html.AppendLine("      filter: blur(16px);");
        html.AppendLine("      opacity: 0.45;");
        html.AppendLine("      transform: scale(1.1);");
        html.AppendLine("      z-index: 1;");
        html.AppendLine("    }");
        html.AppendLine("    .hero-banner .fg-img {");
        html.AppendLine("      position: absolute; inset: 0;");
        html.AppendLine("      width: 100%; height: 100%;");
        html.AppendLine("      object-fit: contain;");
        html.AppendLine("      z-index: 2;");
        html.AppendLine("    }");
        html.AppendLine("    .hero-banner .logo-fallback {");
        html.AppendLine("      position: absolute; inset: 0;");
        html.AppendLine("      width: 100%; height: 100%;");
        html.AppendLine("      object-fit: contain;");
        html.AppendLine("      padding: 30px;");
        html.AppendLine("      z-index: 2;");
        html.AppendLine("      opacity: 0.92;");
        html.AppendLine("    }");
        html.AppendLine("    .hero-banner .banner-gradient {");
        html.AppendLine("      position: absolute; bottom: 0; left: 0; right: 0;");
        html.AppendLine("      height: 80px;");
        html.AppendLine("      background: linear-gradient(transparent, rgba(0,0,0,0.65));");
        html.AppendLine("      z-index: 3;");
        html.AppendLine("    }");
        html.AppendLine("    .hero-banner .price-badge {");
        html.AppendLine("      position: absolute; bottom: 12px; right: 16px; z-index: 4;");
        html.AppendLine("      background: #10b981; color: white;");
        html.AppendLine("      padding: 4px 14px; border-radius: 50px;");
        html.AppendLine("      font-size: 0.85rem; font-weight: 800;");
        html.AppendLine("      box-shadow: 0 2px 8px rgba(0,0,0,0.25);");
        html.AppendLine("    }");
        html.AppendLine("    .hero-banner .brand-badge {");
        html.AppendLine("      position: absolute; top: 12px; right: 14px; z-index: 4;");
        html.AppendLine("      background: rgba(255,255,255,0.15);");
        html.AppendLine("      backdrop-filter: blur(8px);");
        html.AppendLine("      border: 1px solid rgba(255,255,255,0.25);");
        html.AppendLine("      color: white; padding: 4px 12px; border-radius: 50px;");
        html.AppendLine("      font-size: 0.78rem; font-weight: 700;");
        html.AppendLine("    }");
        html.AppendLine("    /* ── Card Body ── */");
        html.AppendLine("    .card-body { padding: 24px; }");
        html.AppendLine("    .header-details { flex-grow: 1; }");
        html.AppendLine("    .service-title {");
        html.AppendLine("      font-size: 1.35rem;");
        html.AppendLine("      color: #0f172a;");
        html.AppendLine("      margin: 0 0 6px 0;");
        html.AppendLine("      line-height: 1.4;");
        html.AppendLine("      font-weight: 800;");
        html.AppendLine("    }");
        html.AppendLine("    .provider-badge {");
        html.AppendLine("      display: inline-flex;");
        html.AppendLine("      align-items: center;");
        html.AppendLine("      gap: 6px;");
        html.AppendLine("      font-size: 0.85rem;");
        html.AppendLine("      color: #64748b;");
        html.AppendLine("      margin-bottom: 8px;");
        html.AppendLine("    }");
        html.AppendLine("    .rating-badge {");
        html.AppendLine("      background: #fef08a;");
        html.AppendLine("      color: #854d0e;");
        html.AppendLine("      padding: 2px 8px;");
        html.AppendLine("      border-radius: 8px;");
        html.AppendLine("      font-weight: bold;");
        html.AppendLine("      font-size: 0.8rem;");
        html.AppendLine("    }");
        html.AppendLine("    .badge-row {");
        html.AppendLine("      display: flex;");
        html.AppendLine("      flex-wrap: wrap;");
        html.AppendLine("      gap: 10px;");
        html.AppendLine("      margin-bottom: 20px;");
        html.AppendLine("    }");
        html.AppendLine("    .badge-item {");
        html.AppendLine("      padding: 6px 14px;");
        html.AppendLine("      border-radius: 50px;");
        html.AppendLine("      font-size: 0.82rem;");
        html.AppendLine("      font-weight: 700;");
        html.AppendLine("    }");
        html.AppendLine("    .badge-category {");
        html.AppendLine("      background: #eff6ff;");
        html.AppendLine("      color: #2563eb;");
        html.AppendLine("    }");
        html.AppendLine("    .badge-location {");
        html.AppendLine("      background: #fff1f2;");
        html.AppendLine("      color: #be123c;");
        html.AppendLine("    }");
        html.AppendLine("    .section-title {");
        html.AppendLine("      font-size: 0.95rem;");
        html.AppendLine("      color: #334155;");
        html.AppendLine("      font-weight: 700;");
        html.AppendLine("      margin: 20px 0 10px 0;");
        html.AppendLine("      border-right: 3px solid #1d6070;");
        html.AppendLine("      padding-right: 8px;");
        html.AppendLine("    }");
        html.AppendLine("    .desc-box {");
        html.AppendLine("      background: #f8fafc;");
        html.AppendLine("      border-radius: 16px;");
        html.AppendLine("      padding: 16px;");
        html.AppendLine("      color: #475569;");
        html.AppendLine("      font-size: 0.9rem;");
        html.AppendLine("      line-height: 1.7;");
        html.AppendLine("      margin-bottom: 20px;");
        html.AppendLine("      border: 1px solid #f1f5f9;");
        html.AppendLine("      white-space: pre-line;");
        html.AppendLine("    }");
        html.AppendLine("    .info-grid {");
        html.AppendLine("      display: grid;");
        html.AppendLine("      grid-template-columns: 1fr 1fr;");
        html.AppendLine("      gap: 15px;");
        html.AppendLine("      margin-bottom: 20px;");
        html.AppendLine("    }");
        html.AppendLine("    .info-card {");
        html.AppendLine("      background: #f8fafc;");
        html.AppendLine("      border-radius: 12px;");
        html.AppendLine("      padding: 12px;");
        html.AppendLine("      border: 1px solid #f1f5f9;");
        html.AppendLine("      display: flex;");
        html.AppendLine("      align-items: center;");
        html.AppendLine("      gap: 10px;");
        html.AppendLine("    }");
        html.AppendLine("    .info-icon {");
        html.AppendLine("      font-size: 1.2rem;");
        html.AppendLine("    }");
        html.AppendLine("    .info-label {");
        html.AppendLine("      font-size: 0.75rem;");
        html.AppendLine("      color: #94a3b8;");
        html.AppendLine("      margin-bottom: 2px;");
        html.AppendLine("    }");
        html.AppendLine("    .info-value {");
        html.AppendLine("      font-size: 0.85rem;");
        html.AppendLine("      color: #334155;");
        html.AppendLine("      font-weight: 600;");
        html.AppendLine("    }");
        html.AppendLine("    .contact-buttons {");
        html.AppendLine("      display: flex;");
        html.AppendLine("      flex-wrap: wrap;");
        html.AppendLine("      gap: 12px;");
        html.AppendLine("      margin-bottom: 20px;");
        html.AppendLine("    }");
        html.AppendLine("    .btn-contact {");
        html.AppendLine("      flex: 1;");
        html.AppendLine("      min-width: 140px;");
        html.AppendLine("      display: inline-flex;");
        html.AppendLine("      align-items: center;");
        html.AppendLine("      justify-content: center;");
        html.AppendLine("      gap: 8px;");
        html.AppendLine("      padding: 12px 16px;");
        html.AppendLine("      border-radius: 12px;");
        html.AppendLine("      text-decoration: none;");
        html.AppendLine("      font-weight: bold;");
        html.AppendLine("      font-size: 0.85rem;");
        html.AppendLine("      transition: all 0.2s;");
        html.AppendLine("      color: white;");
        html.AppendLine("    }");
        html.AppendLine("    .btn-icon {");
        html.AppendLine("      font-size: 1rem;");
        html.AppendLine("    }");
        html.AppendLine("    .btn-whatsapp { background: #25d366; }");
        html.AppendLine("    .btn-phone { background: #3b82f6; }");
        html.AppendLine("    .btn-telegram { background: #0088cc; }");
        html.AppendLine("    .btn-social { background: #475569; }");
        html.AppendLine("    .btn-contact:hover {");
        html.AppendLine("      transform: translateY(-2px);");
        html.AppendLine("      box-shadow: 0 4px 12px rgba(0,0,0,0.1);");
        html.AppendLine("    }");
        html.AppendLine("    .promo-box {");
        html.AppendLine("      background: linear-gradient(135deg, #1d6070 0%, #2a7f8f 60%, #f47c30 100%);");
        html.AppendLine("      border-radius: 20px;");
        html.AppendLine("      padding: 22px;");
        html.AppendLine("      color: white;");
        html.AppendLine("      margin-top: 25px;");
        html.AppendLine("      margin-bottom: 15px;");
        html.AppendLine("      box-shadow: 0 10px 20px rgba(29, 96, 112, 0.15);");
        html.AppendLine("    }");
        html.AppendLine("    .promo-title {");
        html.AppendLine("      font-size: 1.1rem;");
        html.AppendLine("      font-weight: 800;");
        html.AppendLine("      margin: 0 0 8px 0;");
        html.AppendLine("    }");
        html.AppendLine("    .promo-text {");
        html.AppendLine("      font-size: 0.85rem;");
        html.AppendLine("      line-height: 1.6;");
        html.AppendLine("      margin: 0 0 15px 0;");
        html.AppendLine("      color: rgba(255,255,255,0.9);");
        html.AppendLine("    }");
        html.AppendLine("    .promo-actions {");
        html.AppendLine("      display: flex;");
        html.AppendLine("      gap: 12px;");
        html.AppendLine("      flex-wrap: wrap;");
        html.AppendLine("    }");
        html.AppendLine("    .btn-promo {");
        html.AppendLine("      flex: 1;");
        html.AppendLine("      min-width: 140px;");
        html.AppendLine("      padding: 10px 18px;");
        html.AppendLine("      border-radius: 10px;");
        html.AppendLine("      text-decoration: none;");
        html.AppendLine("      font-weight: bold;");
        html.AppendLine("      font-size: 0.85rem;");
        html.AppendLine("      text-align: center;");
        html.AppendLine("      transition: all 0.2s;");
        html.AppendLine("    }");
        html.AppendLine("    .btn-promo-web { background: white; color: #1d6070; }");
        html.AppendLine("    .btn-promo-app { background: rgba(255,255,255,0.2); color: white; border: 1px solid rgba(255,255,255,0.3); }");
        html.AppendLine("    .btn-promo:hover { transform: scale(1.03); }");
        html.AppendLine("    /* ── Hero CTA Buttons ── */");
        html.AppendLine("    .hero-cta {");
        html.AppendLine("      display: flex;");
        html.AppendLine("      flex-direction: column;");
        html.AppendLine("      gap: 12px;");
        html.AppendLine("      padding: 20px 20px 10px 20px;");
        html.AppendLine("      background: #f8fafc;");
        html.AppendLine("      border-bottom: 1px solid #e2e8f0;");
        html.AppendLine("    }");
        html.AppendLine("    .btn-cta {");
        html.AppendLine("      display: flex;");
        html.AppendLine("      align-items: center;");
        html.AppendLine("      justify-content: center;");
        html.AppendLine("      gap: 10px;");
        html.AppendLine("      padding: 14px 20px;");
        html.AppendLine("      border-radius: 14px;");
        html.AppendLine("      text-decoration: none;");
        html.AppendLine("      font-family: 'Tajawal', sans-serif;");
        html.AppendLine("      font-weight: 800;");
        html.AppendLine("      font-size: 1rem;");
        html.AppendLine("      transition: all 0.25s ease;");
        html.AppendLine("      border: none;");
        html.AppendLine("      cursor: pointer;");
        html.AppendLine("    }");
        html.AppendLine("    .btn-cta-service {");
        html.AppendLine("      background: linear-gradient(135deg, #1d6070 0%, #2a9d8f 100%);");
        html.AppendLine("      color: white;");
        html.AppendLine("      box-shadow: 0 4px 15px rgba(29, 96, 112, 0.4);");
        html.AppendLine("    }");
        html.AppendLine("    .btn-cta-service:hover {");
        html.AppendLine("      transform: translateY(-2px);");
        html.AppendLine("      box-shadow: 0 8px 20px rgba(29, 96, 112, 0.5);");
        html.AppendLine("    }");
        html.AppendLine("    .btn-cta-download {");
        html.AppendLine("      background: linear-gradient(135deg, #f47c30 0%, #e05a1f 100%);");
        html.AppendLine("      color: white;");
        html.AppendLine("      box-shadow: 0 4px 15px rgba(244, 124, 48, 0.4);");
        html.AppendLine("    }");
        html.AppendLine("    .btn-cta-download:hover {");
        html.AppendLine("      transform: translateY(-2px);");
        html.AppendLine("      box-shadow: 0 8px 20px rgba(244, 124, 48, 0.5);");
        html.AppendLine("    }");
        html.AppendLine("    .btn-cta-icon {");
        html.AppendLine("      font-size: 1.3rem;");
        html.AppendLine("      flex-shrink: 0;");
        html.AppendLine("    }");
        html.AppendLine("    .btn-cta-text { line-height: 1.3; text-align: right; }");
        html.AppendLine("    .btn-cta-label { display: block; font-size: 0.75rem; font-weight: 500; opacity: 0.85; }");
        html.AppendLine("    .btn-cta-title { display: block; font-size: 1rem; font-weight: 800; }");
        html.AppendLine("    .footer-text {");
        html.AppendLine("      font-size: 0.75rem;");
        html.AppendLine("      color: #94a3b8;");
        html.AppendLine("      text-align: center;");
        html.AppendLine("      margin: 0;");
        html.AppendLine("    }");
        html.AppendLine("    .redirect-notice {");
        html.AppendLine("      font-size: 0.85rem;");
        html.AppendLine("      color: #64748b;");
        html.AppendLine("      text-align: center;");
        html.AppendLine("      margin-bottom: 20px;");
        html.AppendLine("      display: flex;");
        html.AppendLine("      align-items: center;");
        html.AppendLine("      justify-content: center;");
        html.AppendLine("      gap: 8px;");
        html.AppendLine("    }");
        html.AppendLine("    .spinner {");
        html.AppendLine("      width: 16px;");
        html.AppendLine("      height: 16px;");
        html.AppendLine("      border: 2px solid #cbd5e1;");
        html.AppendLine("      border-top: 2px solid #1d6070;");
        html.AppendLine("      border-radius: 50%;");
        html.AppendLine("      animation: spin 0.8s linear infinite;");
        html.AppendLine("    }");
        html.AppendLine("    @keyframes spin {");
        html.AppendLine("      0% { transform: rotate(0deg); }");
        html.AppendLine("      100% { transform: rotate(360deg); }");
        html.AppendLine("    }");
        html.AppendLine("    @media (max-width: 500px) {");
        html.AppendLine("      .info-grid { grid-template-columns: 1fr; }");
        html.AppendLine("      .header-flex { flex-direction: column; text-align: center; }");
        html.AppendLine("      .service-thumb { margin: 0 auto; }");
        html.AppendLine("    }");
        html.AppendLine("  </style>");
        html.AppendLine("</head>");
        
        html.AppendLine("<body>");
        html.AppendLine("  <div class=\"card-container\">");

        // ── Hero Banner (full-width image at top) ──
        // Detect whether this is a real service image or logo fallback
        bool isLogoFallback = imageUrl.EndsWith("/images/logo.png") || imageUrl.EndsWith("/images/defaults/default_service.png");
        html.AppendLine("    <div class=\"hero-banner\">");
        if (!isLogoFallback)
        {
            // Blurred background + sharp foreground for real images
            html.AppendLine($"      <img class=\"bg-blur\" src=\"{imageUrl}\" alt=\"\" />");
            html.AppendLine($"      <img class=\"fg-img\" src=\"{imageUrl}\" alt=\"{safeTitle}\" />");
        }
        else
        {
            // Logo centered on dark gradient background
            html.AppendLine($"      <img class=\"logo-fallback\" src=\"{imageUrl}\" alt=\"خدماوي\" />");
        }
        html.AppendLine("      <div class=\"banner-gradient\"></div>");
        html.AppendLine($"      <div class=\"price-badge\">💰 {priceText}</div>");
        html.AppendLine("      <div class=\"brand-badge\">📲 خدماوي</div>");
        html.AppendLine("    </div>");

        // ── Two CTA Buttons directly below the hero image ──
        html.AppendLine("    <div class=\"hero-cta\">");
        html.AppendLine($"      <a href=\"{serviceUrl}\" class=\"btn-cta btn-cta-service\">");
        html.AppendLine("        <span class=\"btn-cta-icon\">🔗</span>");
        html.AppendLine("        <span class=\"btn-cta-text\">");
        html.AppendLine("          <span class=\"btn-cta-label\">انقر هنا لعرض تفاصيل الخدمة</span>");
        html.AppendLine("          <span class=\"btn-cta-title\">زيارة صفحة الخدمة</span>");
        html.AppendLine("        </span>");
        html.AppendLine("      </a>");
        html.AppendLine($"      <a href=\"{appStoreUrl}\" class=\"btn-cta btn-cta-download\">");
        html.AppendLine("        <span class=\"btn-cta-icon\">📲</span>");
        html.AppendLine("        <span class=\"btn-cta-text\">");
        html.AppendLine("          <span class=\"btn-cta-label\">تحميل مجاني مباشر من الموقع</span>");
        html.AppendLine("          <span class=\"btn-cta-title\">تحميل تطبيق خدماوي</span>");
        html.AppendLine("        </span>");
        html.AppendLine("      </a>");
        html.AppendLine("    </div>");

        // ── Card Body ──
        html.AppendLine("    <div class=\"card-body\">");

        // Title + Provider
        html.AppendLine("        <div class=\"provider-badge\">");
        html.AppendLine($"          👤 <span>{safeProviderName}</span>");
        if (service.Rating > 0)
        {
            html.AppendLine($"          <span class=\"rating-badge\">⭐ {service.Rating:0.0}</span>");
        }
        html.AppendLine("        </div>");
        html.AppendLine($"        <h1 class=\"service-title\">{safeTitle}</h1>");

        // Badges Row
        html.AppendLine("    <div class=\"badge-row\">");
        html.AppendLine($"      <span class=\"badge-item badge-category\">🗂️ {categoryPath}</span>");
        html.AppendLine($"      <span class=\"badge-item badge-location\">📍 {location}</span>");
        html.AppendLine("    </div>");

        // Description
        html.AppendLine("    <div class=\"section-title\">الوصف</div>");
        html.AppendLine($"    <div class=\"desc-box\">{safeDescFull}</div>");

        // Info Grid
        html.AppendLine("    <div class=\"section-title\">معلومات الخدمة</div>");
        html.AppendLine("    <div class=\"info-grid\">");
        
        // Location
        html.AppendLine("      <div class=\"info-card\">");
        html.AppendLine("        <span class=\"info-icon\">📍</span>");
        html.AppendLine("        <div>");
        html.AppendLine("          <div class=\"info-label\">المنطقة</div>");
        html.AppendLine($"          <div class=\"info-value\">{location}</div>");
        html.AppendLine("        </div>");
        html.AppendLine("      </div>");

        // Address
        html.AppendLine("      <div class=\"info-card\">");
        html.AppendLine("        <span class=\"info-icon\">🏠</span>");
        html.AppendLine("        <div>");
        html.AppendLine("          <div class=\"info-label\">العنوان بالتفصيل</div>");
        html.AppendLine($"          <div class=\"info-value\">{safeAddress}</div>");
        html.AppendLine("        </div>");
        html.AppendLine("      </div>");

        // Work Days
        html.AppendLine("      <div class=\"info-card\">");
        html.AppendLine("        <span class=\"info-icon\">📅</span>");
        html.AppendLine("        <div>");
        html.AppendLine("          <div class=\"info-label\">أيام العمل</div>");
        html.AppendLine($"          <div class=\"info-value\">{safeWorkDays}</div>");
        html.AppendLine("        </div>");
        html.AppendLine("      </div>");

        // Work Hours
        html.AppendLine("      <div class=\"info-card\">");
        html.AppendLine("        <span class=\"info-icon\">⏰</span>");
        html.AppendLine("        <div>");
        html.AppendLine("          <div class=\"info-label\">ساعات العمل</div>");
        html.AppendLine($"          <div class=\"info-value\">{safeWorkHours}</div>");
        html.AppendLine("        </div>");
        html.AppendLine("      </div>");

        html.AppendLine("    </div>");

        // Contacts Section
        html.AppendLine("    <div class=\"section-title\">تواصل مباشر</div>");
        html.AppendLine("    <div class=\"contact-buttons\">");
        html.AppendLine(contactButtonsHtml.ToString());
        html.AppendLine("    </div>");

        // App Promo Box
        html.AppendLine("    <div class=\"promo-box\">");
        html.AppendLine("      <p class=\"promo-title\">📲 حمل تطبيق خدماوي مجاناً</p>");
        html.AppendLine("      <p class=\"promo-text\">منصة خدماوي هي سوق الخدمات والأعمال الأول في مصر. تصفح آلاف الخدمات والمنتجات القريبة منك، وتواصل مباشرة مع الحرفيين ومقدمي الخدمات بكل سهولة وأمان!</p>");
        html.AppendLine("    </div>");

        // Footer
        html.AppendLine("    <p class=\"footer-text\">خدماوي — سوق الخدمات الأول في مصر</p>");
        html.AppendLine("    </div>"); // end card-body

        html.AppendLine("  </div>");

        // JavaScript Redirect (with 2s delay)
        html.AppendLine("  <script>");
        html.AppendLine("    var ua = navigator.userAgent.toLowerCase();");
        html.AppendLine("    var isBot = /bot|crawler|spider|scraper|facebookexternalhit|facebot|twitterbot|whatsapp|telegram/i.test(ua);");
        html.AppendLine("    if (!isBot) {");
        html.AppendLine("      setTimeout(function() { window.location.href = '" + serviceUrl + "'; }, 2000);");
        html.AppendLine("    }");
        html.AppendLine("  </script>");

        html.AppendLine("</body>");
        html.AppendLine("</html>");

        return Content(html.ToString(), "text/html; charset=utf-8");
    }

    /// <summary>
    /// Uploads a generated card image for a service.
    /// This is called from the client side after rendering the card using html2canvas.
    /// POST /share/service/{id}/image
    /// </summary>
    [HttpPost("service/{id:int}/image")]
    public async Task<IActionResult> UploadCardImage(int id, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { success = false, message = "No file uploaded" });

        if (file.Length > 5 * 1024 * 1024) // 5 MB limit
            return BadRequest(new { success = false, message = "File too large" });

        var ext = Path.GetExtension(file.FileName);
        if (!string.Equals(ext, ".png", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(ext, ".jpg", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(ext, ".jpeg", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { success = false, message = "Invalid file type. Only PNG/JPG allowed." });
        }

        try
        {
            var basePath = _webHostEnvironment.WebRootPath ?? Path.Combine(_webHostEnvironment.ContentRootPath, "wwwroot");
            var folderPath = Path.Combine(basePath, "images", "share_cards");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, $"card_{id}.png");

            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
            {
                await file.CopyToAsync(stream);
            }

            return Ok(new { success = true, message = "Card image uploaded successfully" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ShareController] Error saving card image: {ex.Message}");
            return StatusCode(500, new { success = false, message = "Error saving card image" });
        }
    }
}
