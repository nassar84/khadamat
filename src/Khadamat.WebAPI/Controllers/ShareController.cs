using MediatR;
using Microsoft.AspNetCore.Mvc;
using Khadamat.Application.Features.Services.Queries;
using System.Text;
using System.Web;

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

    public ShareController(IMediator mediator)
    {
        _mediator = mediator;
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

        // Build the canonical SPA URL that users land on after clicking the shared link
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var serviceUrl = $"{baseUrl}/service/{id}";

        // Pick the best image: first uploaded image, otherwise category image, otherwise default
        string imageUrl;
        if (service.Images?.FirstOrDefault() is { } img && !string.IsNullOrEmpty(img))
        {
            imageUrl = img.StartsWith("http") ? img : $"{baseUrl}/{img.TrimStart('/')}";
        }
        else
        {
            string categoryName = !string.IsNullOrEmpty(service.SubCategoryName) ? service.SubCategoryName :
                                 (!string.IsNullOrEmpty(service.CategoryName) ? service.CategoryName : service.MainCategoryName);
            
            // Replicate CategoryIconResolver slug map for API self-containment
            var slugMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "صحة", "health" }, { "تعليم", "education" }, { "متاجر", "stores" },
                { "ماكولات ومشروبات", "food" }, { "مكاتب", "offices" }, { "حرفيون", "crafts" },
                { "تسوق اون لين", "online_shopping" }, { "مواصلات", "transportation" }, { "صيانة سيارات", "auto_repair" },
                { "خدمات حكومية", "gov_services" }, { "جمعيات واعمال خيرية", "charities" }, { "تمويل وبنوك", "finance" },
                { "متجر السلع", "marketplace" }, { "خدمات اخرى", "other_services" },
                { "عيادات", "clinics" }, { "اسنان", "dentist" }, { "اطفال", "pediatrics" }, { "عيون", "ophthalmology" },
                { "جلدية", "dermatology" }, { "باطنة", "internal_medicine" }, { "عظام", "orthopedics" },
                { "مخ واعصاب", "neurology" }, { "انف واذن وحنجرة", "ent" }, { "صيدليات", "pharmacies" },
                { "مستشفيات", "hospitals" }, { "معامل تحاليل", "labs" }, { "معامل", "labs" },
                { "مراكز طبية", "hospitals" }, { "علاج طبيعي", "physical_therapy" }, { "علاج طبيعى", "physical_therapy" },
                { "مراكز اشعة", "radiology" }, { "دروس خصوصية", "tutoring" }, { "كورسات", "courses" },
                { "حضانات", "nurseries" }, { "مراكز تدريب", "training_centers" }, { "سباكة", "plumbing" },
                { "كهرباء", "electricity" }, { "نجارة", "carpentry" }, { "نقاشة ودهانات", "painting" },
                { "صيانة اجهزة منزلية", "appliances_repair" }, { "توصيل طلبات", "delivery" }, { "نقل اثاث", "furniture_moving" },
                { "تاكسي ورحلات", "taxi" }
            };

            string slug = "other_services";
            if (!string.IsNullOrEmpty(categoryName) && slugMap.TryGetValue(categoryName.Trim(), out var s))
            {
                slug = s;
            }
            else if (!string.IsNullOrEmpty(service.CategoryName) && slugMap.TryGetValue(service.CategoryName.Trim(), out var s2))
            {
                slug = s2;
            }
            else if (!string.IsNullOrEmpty(service.MainCategoryName) && slugMap.TryGetValue(service.MainCategoryName.Trim(), out var s3))
            {
                slug = s3;
            }

            imageUrl = $"{baseUrl}/images/categories/gen/{slug}.png";
        }

        var title    = HttpUtility.HtmlEncode(service.Title);
        var desc     = HttpUtility.HtmlEncode(
            service.Description?.Length > 200
                ? service.Description[..197] + "..."
                : service.Description ?? "");
        var location = HttpUtility.HtmlEncode($"{service.GovernorateName} - {service.CityName}");
        var category = HttpUtility.HtmlEncode($"{service.MainCategoryName} > {service.CategoryName}");
        var price    = service.Price.HasValue ? $"{service.Price:N0} ج.م" : "";

        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html lang=\"ar\" dir=\"rtl\">");
        html.AppendLine("<head>");
        html.AppendLine("  <meta charset=\"UTF-8\" />");
        html.AppendLine($"  <title>{title} - خدمات</title>");

        // === Standard Open Graph tags (Facebook, LinkedIn, Discord) ===
        html.AppendLine($"  <meta property=\"og:type\"        content=\"website\" />");
        html.AppendLine($"  <meta property=\"og:url\"         content=\"{serviceUrl}\" />");
        html.AppendLine($"  <meta property=\"og:title\"       content=\"{title}\" />");
        html.AppendLine($"  <meta property=\"og:description\" content=\"{desc}\" />");
        html.AppendLine($"  <meta property=\"og:image\"       content=\"{imageUrl}\" />");
        html.AppendLine($"  <meta property=\"og:image:width\"  content=\"1200\" />");
        html.AppendLine($"  <meta property=\"og:image:height\" content=\"630\" />");
        html.AppendLine($"  <meta property=\"og:locale\"      content=\"ar_EG\" />");
        html.AppendLine($"  <meta property=\"og:site_name\"   content=\"خدمات\" />");

        // === Twitter Card tags ===
        html.AppendLine($"  <meta name=\"twitter:card\"        content=\"summary_large_image\" />");
        html.AppendLine($"  <meta name=\"twitter:title\"       content=\"{title}\" />");
        html.AppendLine($"  <meta name=\"twitter:description\" content=\"{desc}\" />");
        html.AppendLine($"  <meta name=\"twitter:image\"       content=\"{imageUrl}\" />");

        // === SEO ===
        html.AppendLine($"  <meta name=\"description\" content=\"{desc}\" />");
        html.AppendLine($"  <link rel=\"canonical\" href=\"{serviceUrl}\" />");

        // Immediately redirect the actual human visitor to the SPA page
        html.AppendLine($"  <meta http-equiv=\"refresh\" content=\"0; url={serviceUrl}\" />");
        html.AppendLine("</head>");
        html.AppendLine("<body>");

        // Visible fallback content (also helps WhatsApp which sometimes reads body text)
        html.AppendLine($"  <h1>{title}</h1>");
        if (!string.IsNullOrEmpty(price))
            html.AppendLine($"  <p>السعر: {price}</p>");
        html.AppendLine($"  <p>{desc}</p>");
        html.AppendLine($"  <p>📍 {location}</p>");
        html.AppendLine($"  <p>🗂️ {category}</p>");
        html.AppendLine($"  <p><a href=\"{serviceUrl}\">عرض التفاصيل الكاملة</a></p>");

        html.AppendLine("</body></html>");

        return Content(html.ToString(), "text/html; charset=utf-8");
    }
}
