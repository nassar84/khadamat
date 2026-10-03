using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Khadamat.Application.Features.Services.Queries;
using System.Text;
using System.Web;
using System.Linq;
using Microsoft.AspNetCore.Http;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

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
    private readonly IWebHostEnvironment _env;

    public ShareController(IMediator mediator, IConfiguration configuration, IWebHostEnvironment env)
    {
        _mediator = mediator;
        _configuration = configuration;
        _env = env;
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

        // Determine base URL dynamically from current incoming request
        var scheme = Request.Headers["X-Forwarded-Proto"].FirstOrDefault() ?? Request.Scheme;
        var host = Request.Headers["X-Forwarded-Host"].FirstOrDefault() ?? Request.Host.ToString();
        string baseUrl = $"{scheme}://{host}".TrimEnd('/');

        if (baseUrl.Contains("localhost") || baseUrl.Contains("127.0.0.1") || baseUrl.Contains("::1"))
        {
            var configUrl = _configuration["ApiSettings:WebAppBaseUrl"];
            if (!string.IsNullOrEmpty(configUrl) && !configUrl.Contains("localhost"))
            {
                baseUrl = configUrl.TrimEnd('/');
            }
            else
            {
                var allowedOrigins = _configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
                var publicOrigin = allowedOrigins?.FirstOrDefault(o => o.StartsWith("https://") && !o.Contains("localhost"));
                baseUrl = !string.IsNullOrEmpty(publicOrigin) ? publicOrigin.TrimEnd('/') : "https://khadamawy.eis-dev.com";
            }
        }
        else if (baseUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
        {
            baseUrl = "https://" + baseUrl.Substring(7);
        }

        // Build the canonical SPA URL that users land on after clicking the shared link
        var serviceUrl = $"{baseUrl}/service/{id}";
        var shareUrl = $"{baseUrl}/share/service/{id}";

        // Check if a pre-rendered premium card image exists for this service
        string imageUrl = $"{baseUrl}/images/logo.png"; // safe default — always assigned
        var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var cardRelativePath = $"images/share_cards/card_{id}.png";
        var cardPhysicalPath = Path.Combine(webRoot, cardRelativePath);

        if (System.IO.File.Exists(cardPhysicalPath))
        {
            var lastWrite = System.IO.File.GetLastWriteTimeUtc(cardPhysicalPath).Ticks;
            imageUrl = $"{baseUrl}/{cardRelativePath}?v={lastWrite}";
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
                        var catPath = Path.Combine(webRoot, "images", "categories", $"c_{service.CategoryId}_{service.SubCategoryId}.png");
                        if (System.IO.File.Exists(catPath))
                        {
                            imageUrl = $"{baseUrl}/images/categories/c_{service.CategoryId}_{service.SubCategoryId}.png";
                            foundCategoryImg = true;
                        }
                    }
                    if (!foundCategoryImg && service.CategoryId.HasValue && service.MainCategoryId > 0)
                    {
                        var catPath = Path.Combine(webRoot, "images", "categories", $"c_{service.MainCategoryId}_{service.CategoryId}.png");
                        if (System.IO.File.Exists(catPath))
                        {
                            imageUrl = $"{baseUrl}/images/categories/c_{service.MainCategoryId}_{service.CategoryId}.png";
                            foundCategoryImg = true;
                        }
                    }
                    if (!foundCategoryImg)
                    {
                        // Use branded logo for best social preview when no service image exists
                        var logoPath = Path.Combine(webRoot, "images", "logo.png");
                        imageUrl = System.IO.File.Exists(logoPath)
                            ? $"{baseUrl}/images/logo.png"
                            : $"{baseUrl}/images/defaults/default_service.png";
                    }
                }
            }
        }

        if (imageUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
        {
            imageUrl = "https://" + imageUrl.Substring(7);
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
            var waUrl = $"https://wa.me/{digits}?text=" + HttpUtility.UrlEncode($"مرحباً، بخصوص الخدمة المعروضة على منصة خدماوي: {service.Title}");
            contactButtonsHtml.AppendLine($"      <a href=\"{waUrl}\" target=\"_blank\" class=\"btn-contact btn-whatsapp\"><span class=\"btn-icon\"><svg width=\"16\" height=\"16\" viewBox=\"0 0 448 512\" fill=\"white\" style=\"vertical-align:middle;display:inline-block;\"><path d=\"M380.9 97.1C339 55.1 283.2 32 223.9 32c-122.4 0-222 99.6-222 222 0 39.1 10.2 77.3 29.6 111L0 480l117.7-30.9c32.4 17.7 68.9 27 106.1 27h.1c122.3 0 224.1-99.6 224.1-222 0-59.3-25.2-115-67.1-157zm-157 341.6c-33.2 0-65.7-8.9-94-25.7l-6.7-4-69.8 18.3L72 359.2l-4.4-7c-18.5-29.4-28.2-63.3-28.2-98.2 0-101.7 82.8-184.5 184.6-184.5 49.3 0 95.6 19.2 130.4 54.1 34.8 34.9 56.2 81.2 56.1 130.5 0 101.8-84.9 184.6-186.6 184.6zm101.2-138.2c-5.5-2.8-32.8-16.2-37.9-18-5.1-1.9-8.8-2.8-12.5 2.8-3.7 5.6-14.3 18-17.6 21.8-3.2 3.7-6.5 4.2-12 1.4-32.6-16.3-54-29.1-75.5-66-5.7-9.8 5.7-9.1 16.3-30.3 1.8-3.7.9-6.9-.5-9.7-1.4-2.8-12.5-30.1-17.1-41.2-4.5-10.8-9.1-9.3-12.5-9.5-3.2-.2-6.9-.2-10.6-.2-3.7 0-9.7 1.4-14.8 6.9-5.1 5.6-19.4 19-19.4 46.3 0 27.3 19.9 53.7 22.6 57.4 2.8 3.7 39.1 59.7 94.8 83.8 35.2 15.2 49 16.5 66.6 13.9 10.7-1.6 32.8-13.4 37.4-26.4 4.6-13 4.6-24.1 3.2-26.4-1.3-2.5-5-3.9-10.5-6.6z\"/></svg></span> واتساب: {HttpUtility.HtmlEncode(service.WhatsApp)}</a>");
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
        var ogDesc = !string.IsNullOrEmpty(shortDesc)
            ? shortDesc
            : $"{service.CategoryName} في {service.GovernorateName}";
        // Add location + price context
        var contextSuffix = $" | 📍 {service.GovernorateName}، {service.CityName} | 💰 {priceText}";
        // Add a compelling CTA with app definition and invitation
        var cta = " — 📲 منصة وتطبيق خدماوي: سوق الخدمات والأعمال الأول في مصر. تصفح تفاصيل الخدمة وحمل التطبيق مجاناً لتواصل مباشر وآلاف الخدمات القريبة!";
        var combined = ogDesc + contextSuffix + cta;
        if (combined.Length > 300) combined = combined[..297] + "...";
        var safeOgDesc = HttpUtility.HtmlEncode(combined);

        // Page title (also used as og:title)
        var pageTitle = HttpUtility.HtmlEncode($"{service.Title} • {service.CategoryName} في {service.CityName}");

        // App download URLs
        var appStoreUrl = $"{baseUrl}/downloads/khadamat.apk";

        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html lang=\"ar\" dir=\"rtl\">");
        html.AppendLine("<head>");
        html.AppendLine("  <meta charset=\"UTF-8\" />");
        html.AppendLine("  <meta name=\"build-version\" content=\"v2.0.0-fix-20260824\" />");
        html.AppendLine($"  <title>{pageTitle}</title>");

        var cardExists = System.IO.File.Exists(cardPhysicalPath);
        var finalOgImage = cardExists ? imageUrl : $"{baseUrl}/share/service/{id}/og-image";
        var ogImageType = cardExists ? "image/png" : "image/jpeg";

        // === Standard Open Graph tags (Facebook, LinkedIn, Discord, WhatsApp, Telegram) ===
        html.AppendLine("  <meta property=\"og:type\"        content=\"website\" />");
        html.AppendLine($"  <meta property=\"og:url\"         content=\"{shareUrl}\" />");
        html.AppendLine($"  <meta property=\"og:title\"       content=\"{pageTitle}\" />");
        html.AppendLine($"  <meta property=\"og:description\" content=\"{safeOgDesc}\" />");
        html.AppendLine($"  <meta property=\"og:image\"       content=\"{finalOgImage}\" />");
        html.AppendLine($"  <meta property=\"og:image:secure_url\" content=\"{finalOgImage}\" />");
        html.AppendLine($"  <meta property=\"og:image:type\"   content=\"{ogImageType}\" />");
        html.AppendLine("  <meta property=\"og:image:width\"  content=\"1200\" />");
        html.AppendLine("  <meta property=\"og:image:height\" content=\"630\" />");
        html.AppendLine($"  <meta property=\"og:image:alt\"    content=\"{pageTitle}\" />");
        html.AppendLine("  <meta property=\"og:locale\"      content=\"ar_AR\" />");
        html.AppendLine("  <meta property=\"og:site_name\"   content=\"خدماوي\" />");
        html.AppendLine($"  <link rel=\"image_src\"           href=\"{finalOgImage}\" />");
        html.AppendLine($"  <meta itemprop=\"image\"          content=\"{finalOgImage}\" />");

        // === Facebook specific ===
        html.AppendLine("  <meta property=\"fb:app_id\"      content=\"1546767603438579\" />");

        // === Twitter Card tags ===
        html.AppendLine("  <meta name=\"twitter:card\"        content=\"summary_large_image\" />");
        html.AppendLine($"  <meta name=\"twitter:title\"       content=\"{pageTitle}\" />");
        html.AppendLine($"  <meta name=\"twitter:description\" content=\"{safeOgDesc}\" />");
        html.AppendLine($"  <meta name=\"twitter:image\"       content=\"{finalOgImage}\" />");

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
        html.AppendLine("      border-right: 3px solid #6366f1;");
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
        html.AppendLine("      background: linear-gradient(135deg, #4f46e5 0%, #7c3aed 100%);");
        html.AppendLine("      border-radius: 20px;");
        html.AppendLine("      padding: 22px;");
        html.AppendLine("      color: white;");
        html.AppendLine("      margin-top: 25px;");
        html.AppendLine("      margin-bottom: 15px;");
        html.AppendLine("      box-shadow: 0 10px 20px rgba(79, 70, 229, 0.15);");
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
        html.AppendLine("    .btn-promo-web { background: white; color: #4f46e5; }");
        html.AppendLine("    .btn-promo-app { background: rgba(255,255,255,0.2); color: white; border: 1px solid rgba(255,255,255,0.3); }");
        html.AppendLine("    .btn-promo:hover { transform: scale(1.03); }");
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
        html.AppendLine("      border-top: 2px solid #6366f1;");
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

        // ── Card Body ──
        html.AppendLine("    <div class=\"card-body\">");

        // Redirect Notice
        html.AppendLine("    <div class=\"redirect-notice\">");
        html.AppendLine("      <div class=\"spinner\"></div>");
        html.AppendLine("      <span>جاري توجيهك إلى صفحة الخدمة بالموقع...</span>");
        html.AppendLine("    </div>");

        // Title & Rating
        if (service.Rating > 0)
        {
            html.AppendLine("        <div style=\"margin-bottom: 8px;\">");
            html.AppendLine($"          <span class=\"rating-badge\">⭐ {service.Rating:0.0}</span>");
            html.AppendLine("        </div>");
        }
        html.AppendLine($"        <h1 class=\"service-title\">{safeTitle}</h1>");

        // Badges Row
        var detailedLocation = !string.IsNullOrEmpty(service.Address)
            ? $"{service.GovernorateName} - {service.CityName} - {service.Address}"
            : $"{service.GovernorateName} - {service.CityName}";
        var safeDetailedLocation = HttpUtility.HtmlEncode(detailedLocation);

        html.AppendLine("    <div class=\"badge-row\">");
        html.AppendLine($"      <span class=\"badge-item badge-category\">🗂️ {categoryPath}</span>");
        html.AppendLine($"      <span class=\"badge-item badge-location\">📍 {safeDetailedLocation}</span>");
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
        html.AppendLine($"        <a href=\"{appStoreUrl}\" class=\"btn-promo btn-promo-app\">📲 تحميل تطبيق خدماوي (APK)</a>");
        html.AppendLine("      </div>");
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
            var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
            var folderPath = Path.Combine(webRoot, "images", "share_cards");

            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var filePath = Path.Combine(folderPath, $"card_{id}.png");

            using (var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
            {
                await file.CopyToAsync(stream);
            }

            // Remove any cached fallback OG image
            var cachedOgPath = Path.Combine(folderPath, $"og_contained_{id}.jpg");
            if (System.IO.File.Exists(cachedOgPath))
            {
                try { System.IO.File.Delete(cachedOgPath); } catch { }
            }

            return Ok(new { success = true, message = "Card image uploaded successfully" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ShareController] Error saving card image: {ex.Message}");
            return StatusCode(500, new { success = false, message = "Error saving card image" });
        }
    }

    /// <summary>
    /// Returns the best available share image URL for a service.
    /// Called by GenerateShareCardAsync in Blazor UI and mobile app.
    /// POST /share/service/{id}/generate-card
    /// </summary>
    [HttpPost("service/{id:int}/generate-card")]
    public async Task<IActionResult> GenerateCard(int id)
    {
        var service = await _mediator.Send(new GetServiceByIdQuery(id));
        if (service == null)
            return NotFound(new { success = false });

        var scheme  = Request.Headers["X-Forwarded-Proto"].FirstOrDefault() ?? Request.Scheme;
        var host    = Request.Headers["X-Forwarded-Host"].FirstOrDefault()  ?? Request.Host.ToString();
        string baseUrl = $"{scheme}://{host}".TrimEnd('/');
        if (baseUrl.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            baseUrl = "https://" + baseUrl[7..];
        if (baseUrl.Contains("localhost") || baseUrl.Contains("127.0.0.1"))
            baseUrl = _configuration["ApiSettings:WebAppBaseUrl"]?.TrimEnd('/') ?? "https://khadamawy.eis-dev.com";

        var webRoot      = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
        var cardRelPath  = $"images/share_cards/card_{id}.png";
        var cardFullPath = Path.Combine(webRoot, cardRelPath);

        // 1. Pre-generated card exists → return it
        if (System.IO.File.Exists(cardFullPath))
        {
            var ts = System.IO.File.GetLastWriteTimeUtc(cardFullPath).Ticks;
            return Ok(new { success = true, cardExists = true, imageUrl = $"{baseUrl}/{cardRelPath}?v={ts}" });
        }

        // 2. Card does NOT exist yet → return success: false so client captures and uploads it
        return Ok(new { success = false, cardExists = false, imageUrl = (string?)null, message = "Card not generated yet" });
    }

    /// <summary>
    /// Returns an optimized 1200x630 Open Graph image for social media crawlers (Facebook, WhatsApp, Twitter).
    /// The service image is 100% contained within the frame with a blurred background, preventing any edge cropping.
    /// </summary>
    [HttpGet("service/{id:int}/og-image")]
    public async Task<IActionResult> GetOgImage(int id)
    {
        var webRoot = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");

        // 1. If an HTML-rendered high-res card exists, serve it
        var cardPath = Path.Combine(webRoot, "images", "share_cards", $"card_{id}.png");
        if (System.IO.File.Exists(cardPath))
        {
            return PhysicalFile(cardPath, "image/png");
        }

        // 2. If a cached contained OG image exists, serve it
        var shareCardsDir = Path.Combine(webRoot, "images", "share_cards");
        if (!Directory.Exists(shareCardsDir)) Directory.CreateDirectory(shareCardsDir);

        var cachedOgPath = Path.Combine(shareCardsDir, $"og_contained_{id}.jpg");
        if (System.IO.File.Exists(cachedOgPath))
        {
            return PhysicalFile(cachedOgPath, "image/jpeg");
        }

        // 3. Find the service image on disk
        var service = await _mediator.Send(new GetServiceByIdQuery(id));
        if (service == null)
        {
            var logoPath = Path.Combine(webRoot, "images", "logo.png");
            if (System.IO.File.Exists(logoPath)) return PhysicalFile(logoPath, "image/png");
            return NotFound();
        }

        string? sourceFile = null;
        var firstImg = service.Images?.FirstOrDefault();
        if (!string.IsNullOrEmpty(firstImg) && !firstImg.Contains("/gen/") && !firstImg.Contains("/placeholders/"))
        {
            var cleanName = firstImg.TrimStart('/').Replace("images/services/", "").Replace("images/", "");
            var testPath = Path.Combine(webRoot, "images", "services", cleanName);
            if (System.IO.File.Exists(testPath)) sourceFile = testPath;
        }

        if (sourceFile == null && !string.IsNullOrEmpty(service.SubCategoryImageUrl))
        {
            var cleanName = service.SubCategoryImageUrl.TrimStart('/').Replace("images/subcategories/", "");
            var testPath = Path.Combine(webRoot, "images", "subcategories", cleanName);
            if (System.IO.File.Exists(testPath)) sourceFile = testPath;
        }

        if (sourceFile == null && !string.IsNullOrEmpty(service.CategoryImageUrl))
        {
            var cleanName = service.CategoryImageUrl.TrimStart('/').Replace("images/categories/", "");
            var testPath = Path.Combine(webRoot, "images", "categories", cleanName);
            if (System.IO.File.Exists(testPath)) sourceFile = testPath;
        }

        if (sourceFile == null)
        {
            var logoCandidate = Path.Combine(webRoot, "images", "logo.png");
            if (System.IO.File.Exists(logoCandidate)) sourceFile = logoCandidate;
        }

        if (sourceFile == null || !System.IO.File.Exists(sourceFile))
        {
            return NotFound();
        }

        // 4. Generate contained 1200x630 image with ImageSharp
        try
        {
            using var srcImg = await SixLabors.ImageSharp.Image.LoadAsync(sourceFile);

            const int targetW = 1200;
            const int targetH = 630;

            using var canvas = new SixLabors.ImageSharp.Image<SixLabors.ImageSharp.PixelFormats.Rgba32>(targetW, targetH, new SixLabors.ImageSharp.PixelFormats.Rgba32(15, 23, 42, 255));

            // Background layer: blurred & dimmed version of the source image
            using (var bgImg = srcImg.Clone(ctx =>
            {
                ctx.Resize(new ResizeOptions
                {
                    Size = new SixLabors.ImageSharp.Size(targetW, targetH),
                    Mode = ResizeMode.Crop
                });
                ctx.GaussianBlur(25f);
                ctx.Brightness(0.45f);
            }))
            {
                canvas.Mutate(ctx => ctx.DrawImage(bgImg, new SixLabors.ImageSharp.Point(0, 0), 0.7f));
            }

            // Foreground layer: Service image scaled with ResizeMode.Max so 100% is visible
            using (var fgImg = srcImg.Clone(ctx =>
            {
                ctx.Resize(new ResizeOptions
                {
                    Size = new SixLabors.ImageSharp.Size(targetW - 60, targetH - 60),
                    Mode = ResizeMode.Max
                });
            }))
            {
                int posX = (targetW - fgImg.Width) / 2;
                int posY = (targetH - fgImg.Height) / 2;
                canvas.Mutate(ctx => ctx.DrawImage(fgImg, new SixLabors.ImageSharp.Point(posX, posY), 1f));
            }

            await canvas.SaveAsJpegAsync(cachedOgPath);
            return PhysicalFile(cachedOgPath, "image/jpeg");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ShareController] Error generating contained OG image: {ex.Message}");
            return PhysicalFile(sourceFile, "image/jpeg");
        }
    }
}
