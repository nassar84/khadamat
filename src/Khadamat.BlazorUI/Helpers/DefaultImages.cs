using System;
using System.Collections.Generic;
using System.Linq;

namespace Khadamat.BlazorUI.Helpers;

public static class DefaultImages
{
    // User Avatars
    public const string MaleAvatar = "https://ui-avatars.com/api/?name=User&background=4F46E5&color=fff&size=128&bold=true&format=png";
    public const string FemaleAvatar = "https://ui-avatars.com/api/?name=User&background=EC4899&color=fff&size=128&bold=true&format=png";
    public const string DefaultAvatar = "https://ui-avatars.com/api/?name=User&background=6B7280&color=fff&size=128&bold=true&format=png";
    
    // Default service fallback (from ImagePathResolver)
    public static string DefaultService => ImagePathResolver.Service(null);

    /// <summary>
    /// Generates a javascript onerror handler call string that tries to fall back to:
    /// 1. Subcategory image
    /// 2. Category image
    /// 3. Maincategory image
    /// 4. Subcategory icon
    /// 5. Category icon
    /// 6. Maincategory icon
    /// 7. Default service placeholder
    /// </summary>
    public static string GetServiceImageOnError(
        string? subCategoryName = null,
        string? categoryName = null,
        string? mainCategoryName = null,
        string? subCategoryImageUrl = null,
        string? categoryImageUrl = null,
        string? mainCategoryImageUrl = null,
        string? baseUrl = null)
    {
        var fallbacks = new List<string>();

        // Priority 2: SubCategory database-stored image
        if (!string.IsNullOrEmpty(subCategoryImageUrl))
        {
            fallbacks.Add("/" + ImagePathResolver.SubCategory(subCategoryImageUrl).TrimStart('/'));
        }

        // Priority 3: Category database-stored image
        if (!string.IsNullOrEmpty(categoryImageUrl))
        {
            fallbacks.Add("/" + ImagePathResolver.Category(categoryImageUrl).TrimStart('/'));
        }

        // Priority 4: MainCategory database-stored image
        if (!string.IsNullOrEmpty(mainCategoryImageUrl))
        {
            fallbacks.Add("/" + ImagePathResolver.MainCategory(mainCategoryImageUrl).TrimStart('/'));
        }

        // Priority 5: SubCategory icon
        if (!string.IsNullOrEmpty(subCategoryName))
        {
            var subIcon = CategoryIconResolver.GetIconUrl(subCategoryName, null, null, categoryName);
            if (!string.IsNullOrEmpty(subIcon) && !subIcon.EndsWith("other_services.png"))
                fallbacks.Add(subIcon);
        }

        // Priority 6: Category icon
        if (!string.IsNullOrEmpty(categoryName))
        {
            var catIcon = CategoryIconResolver.GetIconUrl(categoryName, null, null, mainCategoryName);
            if (!string.IsNullOrEmpty(catIcon))
                fallbacks.Add(catIcon);
        }

        // Priority 7: MainCategory icon
        if (!string.IsNullOrEmpty(mainCategoryName))
        {
            var mainIcon = CategoryIconResolver.GetIconUrl(mainCategoryName);
            if (!string.IsNullOrEmpty(mainIcon))
                fallbacks.Add(mainIcon);
        }

        // Ultimate fallback
        fallbacks.Add("/images/placeholders/default_service.png");

        // Format each fallback as an absolute url or relative path. 
        // If baseUrl is provided, we can prepend it to relative paths.
        var formatted = new List<string>();
        foreach (var fb in fallbacks)
        {
            if (fb.StartsWith("http", StringComparison.OrdinalIgnoreCase) || fb.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            {
                formatted.Add(fb);
            }
            else
            {
                var relative = "/" + fb.TrimStart('/');
                if (!string.IsNullOrEmpty(baseUrl))
                {
                    formatted.Add($"{baseUrl.TrimEnd('/')}{relative}");
                }
                else
                {
                    formatted.Add(relative);
                }
            }
        }

        var jsonList = string.Join(",", formatted.Select(f => $"'{f.Replace("'", "\\'")}'"));
        return $"window.handleServiceImageError(this, [{jsonList}]);";
    }

    // Services - keyword-matched photographic fallbacks (keep as Unsplash)
    public const string PlumbingService = "https://images.unsplash.com/photo-1607472586893-edb57bdc0e39?w=800&h=600&fit=crop&q=80";
    public const string ElectricianService = "https://images.unsplash.com/photo-1621905251189-08b45d6a269e?w=800&h=600&fit=crop&q=80";
    public const string CarpentryService = "https://images.unsplash.com/photo-1504148455328-c376907d081c?w=800&h=600&fit=crop&q=80";
    public const string PaintingService = "https://images.unsplash.com/photo-1589939705384-5185137a7f0f?w=800&h=600&fit=crop&q=80";
    public const string CleaningService = "https://images.unsplash.com/photo-1581578731548-c64695cc6952?w=800&h=600&fit=crop&q=80";
    
    // Category placeholders
    public const string DefaultCategory = "https://images.unsplash.com/photo-1486406146926-c627a92ad1ab?w=400&h=300&fit=crop&q=80";
    
    // Provider Profile
    public const string DefaultProviderBanner = "https://images.unsplash.com/photo-1497366216548-37526070297c?w=1200&h=400&fit=crop&q=80";

    /// <summary>
    /// Get user avatar based on name and optional gender
    /// </summary>
    public static string GetUserAvatar(string? name = null, string? gender = null, string? existingUrl = null)
    {
        if (!string.IsNullOrEmpty(existingUrl))
            return existingUrl;

        var displayName = string.IsNullOrEmpty(name) ? "User" : name;
        var initials = GetInitials(displayName);
        
        // Determine background color based on gender
        var bgColor = gender?.ToLower() switch
        {
            "male" or "ذكر" => "4F46E5",      // Indigo
            "female" or "أنثى" => "EC4899",   // Pink
            _ => "6B7280"                      // Gray
        };

        return $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(initials)}&background={bgColor}&color=fff&size=128&bold=true&format=png";
    }

    /// <summary>
    /// Get service image based on category or subcategory - INTELLIGENT MATCHING
    /// </summary>
    /// <summary>
    /// Returns TRUE only for images that were actually uploaded by users.
    /// Seed-data filenames like subc_2_5.jpg, c_1_1.jpg are NOT real uploads.
    /// Real uploads: http/https URLs, data: URIs, paths starting with /images/{services,ads,marketplace,users}/, or uploaded filenames.
    /// </summary>
    public static bool IsRealUploadedImage(string? url)
    {
        if (string.IsNullOrEmpty(url)) return false;
        if (url.StartsWith("http", StringComparison.OrdinalIgnoreCase) ||
            url.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            return true;

        var lower = url.ToLowerInvariant();
        
        // If it is a path containing "images/", check if it matches upload folders
        if (lower.Contains("images/"))
        {
            return lower.StartsWith("/images/services/") ||
                   lower.StartsWith("/images/ads/") ||
                   lower.StartsWith("/images/marketplace/") ||
                   lower.StartsWith("/images/users/") ||
                   lower.StartsWith("images/services/") ||
                   lower.StartsWith("images/ads/") ||
                   lower.StartsWith("images/marketplace/") ||
                   lower.StartsWith("images/users/");
        }

        // If it's a filename only:
        // Exclude seed data and defaults
        var filename = System.IO.Path.GetFileName(lower);
        if (filename.StartsWith("subc_", StringComparison.OrdinalIgnoreCase) ||
            filename.StartsWith("c_", StringComparison.OrdinalIgnoreCase) ||
            filename.StartsWith("cat_", StringComparison.OrdinalIgnoreCase) ||
            filename.StartsWith("default", StringComparison.OrdinalIgnoreCase) ||
            filename.StartsWith("defult", StringComparison.OrdinalIgnoreCase) ||
            filename.StartsWith("no-image", StringComparison.OrdinalIgnoreCase))
        {
            // If it starts with subc_ but has a second underscore (e.g. subc_12_34.jpg), it's a renamed service image!
            if (filename.StartsWith("subc_", StringComparison.OrdinalIgnoreCase) && filename.IndexOf('_', 5) > -1)
            {
                return true;
            }
            return false;
        }

        // Real service/user uploads start with "s_" (renamed service) or digits (timestamp_guid)
        return filename.StartsWith("s_", StringComparison.OrdinalIgnoreCase) || 
               (filename.Length > 0 && char.IsDigit(filename[0]));
    }

    /// <summary>
    /// Get service image URL with hierarchical fallback:
    ///   1. Real uploaded service image
    ///   2. Subcategory icon (with category as parent)
    ///   3. Category icon (with main category as parent)
    ///   4. Main category icon
    ///   5. Legacy keyword matching
    ///   6. DefaultService placeholder
    /// </summary>
    public static string GetServiceImage(
        string? existingUrl = null, 
        string? subCategoryName = null, 
        string? categoryName = null, 
        string? mainCategoryName = null,
        string? subCategoryImageUrl = null,
        string? categoryImageUrl = null,
        string? mainCategoryImageUrl = null)
    {
        // Priority 1: Real uploaded service image
        if (IsRealUploadedImage(existingUrl))
        {
            var url = existingUrl!;
            if (url.StartsWith("http", StringComparison.OrdinalIgnoreCase) ||
                url.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
                return url;

            // If it's a filename only (doesn't contain "images/"), resolve to services folder path
            if (!url.Contains("images/"))
            {
                return "/" + ImagePathResolver.Service(url).TrimStart('/');
            }
            return "/" + url.TrimStart('/');
        }

        // Priority 2: SubCategory database-stored image
        if (!string.IsNullOrEmpty(subCategoryImageUrl))
        {
            return "/" + ImagePathResolver.SubCategory(subCategoryImageUrl).TrimStart('/');
        }

        // Priority 3: Category database-stored image
        if (!string.IsNullOrEmpty(categoryImageUrl))
        {
            return "/" + ImagePathResolver.Category(categoryImageUrl).TrimStart('/');
        }

        // Priority 4: MainCategory database-stored image
        if (!string.IsNullOrEmpty(mainCategoryImageUrl))
        {
            return "/" + ImagePathResolver.MainCategory(mainCategoryImageUrl).TrimStart('/');
        }

        // Priority 5: SubCategory icon (with CategoryName as parent fallback)
        if (!string.IsNullOrEmpty(subCategoryName))
        {
            var subIcon = CategoryIconResolver.GetIconUrl(subCategoryName, null, null, categoryName);
            if (!string.IsNullOrEmpty(subIcon) && !subIcon.EndsWith("other_services.png"))
                return subIcon;
        }

        // Priority 6: Category icon (with MainCategoryName as parent fallback)
        if (!string.IsNullOrEmpty(categoryName))
        {
            var catIcon = CategoryIconResolver.GetIconUrl(categoryName, null, null, mainCategoryName);
            if (!string.IsNullOrEmpty(catIcon))
                return catIcon;
        }

        // Priority 7: MainCategory icon
        if (!string.IsNullOrEmpty(mainCategoryName))
        {
            var mainIcon = CategoryIconResolver.GetIconUrl(mainCategoryName);
            if (!string.IsNullOrEmpty(mainIcon))
                return mainIcon;
        }

        // Priority 5: Legacy keyword-based matching
        var matchName = mainCategoryName ?? categoryName ?? subCategoryName;
        if (string.IsNullOrEmpty(matchName))
            return DefaultService;

        var lowerName = matchName.ToLower();

        // Plumbing - سباكة
        if (lowerName.Contains("سباك") || lowerName.Contains("plumb") || 
            lowerName.Contains("صحي") || lowerName.Contains("مياه") || 
            lowerName.Contains("حنفي") || lowerName.Contains("صرف"))
            return PlumbingService;

        // Electrical - كهرباء
        if (lowerName.Contains("كهرب") || lowerName.Contains("electric") || 
            lowerName.Contains("كهرباء") || lowerName.Contains("إضاءة") || 
            lowerName.Contains("أسلاك") || lowerName.Contains("محول"))
            return ElectricianService;

        // Carpentry - نجارة
        if (lowerName.Contains("نجار") || lowerName.Contains("carpent") || 
            lowerName.Contains("خشب") || lowerName.Contains("أثاث") || 
            lowerName.Contains("موبيليا") || lowerName.Contains("wood"))
            return CarpentryService;

        // Painting - دهانات
        if (lowerName.Contains("دهان") || lowerName.Contains("paint") || 
            lowerName.Contains("ديكور") || lowerName.Contains("طلاء") || 
            lowerName.Contains("ألوان") || lowerName.Contains("جبس"))
            return PaintingService;

        // Cleaning - تنظيف
        if (lowerName.Contains("نظاف") || lowerName.Contains("clean") || 
            lowerName.Contains("تنظيف") || lowerName.Contains("غسيل") || 
            lowerName.Contains("تعقيم") || lowerName.Contains("ترتيب"))
            return CleaningService;

        // Construction - بناء
        if (lowerName.Contains("بناء") || lowerName.Contains("construction") || 
            lowerName.Contains("مقاول") || lowerName.Contains("تشييد") || 
            lowerName.Contains("عمار") || lowerName.Contains("بلاط"))
            return "https://images.unsplash.com/photo-1504307651254-35680f356dfd?w=800&h=600&fit=crop&q=80";

        // Technology/IT - تقنية
        if (lowerName.Contains("تقني") || lowerName.Contains("tech") || 
            lowerName.Contains("كمبيوتر") || lowerName.Contains("برمج") || 
            lowerName.Contains("شبكات") || lowerName.Contains("صيانة حاسب"))
            return "https://images.unsplash.com/photo-1518770660439-4636190af475?w=800&h=600&fit=crop&q=80";

        // Auto/Car - سيارات
        if (lowerName.Contains("سيار") || lowerName.Contains("car") || 
            lowerName.Contains("ميكانيك") || lowerName.Contains("auto") || 
            lowerName.Contains("مركب") || lowerName.Contains("vehicle"))
            return "https://images.unsplash.com/photo-1486262715619-67b85e0b08d3?w=800&h=600&fit=crop&q=80";

        // Education - تعليم
        if (lowerName.Contains("تعليم") || lowerName.Contains("education") || 
            lowerName.Contains("دروس") || lowerName.Contains("معلم") || 
            lowerName.Contains("تدريس") || lowerName.Contains("مدرس"))
            return "https://images.unsplash.com/photo-1503676260728-1c00da094a0b?w=800&h=600&fit=crop&q=80";

        // Health/Medical - صحة
        if (lowerName.Contains("صحة") || lowerName.Contains("health") || 
            lowerName.Contains("طب") || lowerName.Contains("medical") || 
            lowerName.Contains("علاج") || lowerName.Contains("دكتور"))
            return "https://images.unsplash.com/photo-1505751172876-fa1923c5c528?w=800&h=600&fit=crop&q=80";

        // Beauty/Salon - تجميل
        if (lowerName.Contains("تجميل") || lowerName.Contains("beauty") || 
            lowerName.Contains("صالون") || lowerName.Contains("حلاق") || 
            lowerName.Contains("كوافير") || lowerName.Contains("مكياج"))
            return "https://images.unsplash.com/photo-1560066984-138dadb4c035?w=800&h=600&fit=crop&q=80";

        // Photography - تصوير
        if (lowerName.Contains("تصوير") || lowerName.Contains("photo") || 
            lowerName.Contains("كاميرا") || lowerName.Contains("فوتو") || 
            lowerName.Contains("مصور") || lowerName.Contains("استوديو"))
            return "https://images.unsplash.com/photo-1554048612-b6a482bc67e5?w=800&h=600&fit=crop&q=80";

        // Gardening - حدائق
        if (lowerName.Contains("حديقة") || lowerName.Contains("garden") || 
            lowerName.Contains("زراع") || lowerName.Contains("نبات") || 
            lowerName.Contains("تنسيق حدائق") || lowerName.Contains("landscape"))
            return "https://images.unsplash.com/photo-1416879595882-3373a0480b5b?w=800&h=600&fit=crop&q=80";

        // Moving/Transport - نقل
        if (lowerName.Contains("نقل") || lowerName.Contains("moving") || 
            lowerName.Contains("شحن") || lowerName.Contains("توصيل") || 
            lowerName.Contains("transport") || lowerName.Contains("delivery"))
            return "https://images.unsplash.com/photo-1586528116311-ad8dd3c8310d?w=800&h=600&fit=crop&q=80";

        // Default
        return DefaultService;
    }

    /// <summary>
    /// Get category image based on category name - INTELLIGENT MATCHING
    /// </summary>
    public static string GetCategoryImage(string? categoryName = null, string? existingUrl = null)
    {
        // Use resolver for DB-stored image filenames (e.g. "c_5.png" → "images/categories/c_5.png")
        if (!string.IsNullOrEmpty(existingUrl))
            return ImagePathResolver.Category(existingUrl);

        // No entity image — try keyword matching
        if (string.IsNullOrEmpty(categoryName))
            return DefaultCategory;

        var lowerName = categoryName.ToLower();

        // Home Services - خدمات منزلية
        if (lowerName.Contains("منزل") || lowerName.Contains("home") || 
            lowerName.Contains("بيت") || lowerName.Contains("house"))
            return "https://images.unsplash.com/photo-1560518883-ce09059eeffa?w=400&h=300&fit=crop&q=80";

        // Construction - بناء وتشييد
        if (lowerName.Contains("بناء") || lowerName.Contains("construction") || 
            lowerName.Contains("تشييد") || lowerName.Contains("مقاول") || 
            lowerName.Contains("عمار") || lowerName.Contains("building"))
            return "https://images.unsplash.com/photo-1504307651254-35680f356dfd?w=400&h=300&fit=crop&q=80";

        // Technology - تقنية
        if (lowerName.Contains("تقني") || lowerName.Contains("tech") || 
            lowerName.Contains("معلومات") || lowerName.Contains("it") || 
            lowerName.Contains("كمبيوتر") || lowerName.Contains("برمج"))
            return "https://images.unsplash.com/photo-1518770660439-4636190af475?w=400&h=300&fit=crop&q=80";

        // Education - تعليم
        if (lowerName.Contains("تعليم") || lowerName.Contains("education") || 
            lowerName.Contains("دراس") || lowerName.Contains("تدريب") || 
            lowerName.Contains("كورس") || lowerName.Contains("training"))
            return "https://images.unsplash.com/photo-1503676260728-1c00da094a0b?w=400&h=300&fit=crop&q=80";

        // Health - صحة
        if (lowerName.Contains("صحة") || lowerName.Contains("health") || 
            lowerName.Contains("طب") || lowerName.Contains("medical") || 
            lowerName.Contains("علاج") || lowerName.Contains("رياضة"))
            return "https://images.unsplash.com/photo-1505751172876-fa1923c5c528?w=400&h=300&fit=crop&q=80";

        // Business - أعمال
        if (lowerName.Contains("أعمال") || lowerName.Contains("business") || 
            lowerName.Contains("تجار") || lowerName.Contains("مكتب") || 
            lowerName.Contains("شركات") || lowerName.Contains("office"))
            return "https://images.unsplash.com/photo-1486406146926-c627a92ad1ab?w=400&h=300&fit=crop&q=80";

        // Food - طعام
        if (lowerName.Contains("طعام") || lowerName.Contains("food") || 
            lowerName.Contains("مطعم") || lowerName.Contains("طبخ") || 
            lowerName.Contains("مأكولات") || lowerName.Contains("restaurant"))
            return "https://images.unsplash.com/photo-1504674900247-0877df9cc836?w=400&h=300&fit=crop&q=80";

        // Events - فعاليات
        if (lowerName.Contains("فعالي") || lowerName.Contains("event") || 
            lowerName.Contains("حفل") || lowerName.Contains("مناسب") || 
            lowerName.Contains("زفاف") || lowerName.Contains("party"))
            return "https://images.unsplash.com/photo-1511795409834-ef04bbd61622?w=400&h=300&fit=crop&q=80";

        return DefaultCategory;
    }

    /// <summary>
    /// Get initials from name (max 2 characters)
    /// </summary>
    private static string GetInitials(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return "U";

        var parts = name.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        if (parts.Length == 1)
            return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();
        
        return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
    }

    /// <summary>
    /// Generate a colored avatar with initials
    /// </summary>
    public static string GenerateColoredAvatar(string name, string? colorHex = null)
    {
        var initials = GetInitials(name);
        var color = colorHex ?? GenerateColorFromName(name);
        
        return $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(initials)}&background={color}&color=fff&size=128&bold=true&format=png";
    }

    /// <summary>
    /// Generate a consistent color based on name hash
    /// </summary>
    private static string GenerateColorFromName(string name)
    {
        var colors = new[]
        {
            "4F46E5", // Indigo
            "EC4899", // Pink
            "10B981", // Green
            "F59E0B", // Amber
            "EF4444", // Red
            "8B5CF6", // Purple
            "06B6D4", // Cyan
            "F97316"  // Orange
        };

        var hash = 0;
        foreach (var c in name)
        {
            hash = ((hash << 5) - hash) + c;
            hash = hash & hash; // Convert to 32bit integer
        }

        return colors[Math.Abs(hash) % colors.Length];
    }
}
