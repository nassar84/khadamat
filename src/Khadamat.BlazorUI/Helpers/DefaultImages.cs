namespace Khadamat.BlazorUI.Helpers;

public static class DefaultImages
{
    // User Avatars
    public const string MaleAvatar = "https://ui-avatars.com/api/?name=User&background=4F46E5&color=fff&size=200&bold=true&format=svg";
    public const string FemaleAvatar = "https://ui-avatars.com/api/?name=User&background=EC4899&color=fff&size=200&bold=true&format=svg";
    public const string DefaultAvatar = "https://ui-avatars.com/api/?name=User&background=6B7280&color=fff&size=200&bold=true&format=svg";
    
    // Services
    public const string DefaultService = "https://images.unsplash.com/photo-1581578731548-c64695cc6952?w=800&h=600&fit=crop&q=80";
    public const string PlumbingService = "https://images.unsplash.com/photo-1607472586893-edb57bdc0e39?w=800&h=600&fit=crop&q=80";
    public const string ElectricianService = "https://images.unsplash.com/photo-1621905251189-08b45d6a269e?w=800&h=600&fit=crop&q=80";
    public const string CarpentryService = "https://images.unsplash.com/photo-1504148455328-c376907d081c?w=800&h=600&fit=crop&q=80";
    public const string PaintingService = "https://images.unsplash.com/photo-1589939705384-5185137a7f0f?w=800&h=600&fit=crop&q=80";
    public const string CleaningService = "https://images.unsplash.com/photo-1581578731548-c64695cc6952?w=800&h=600&fit=crop&q=80";
    
    // Categories
    public const string DefaultCategory = "https://images.unsplash.com/photo-1486406146926-c627a92ad1ab?w=400&h=300&fit=crop&q=80";
    public const string HomeServicesCategory = "https://images.unsplash.com/photo-1560518883-ce09059eeffa?w=400&h=300&fit=crop&q=80";
    public const string ConstructionCategory = "https://images.unsplash.com/photo-1504307651254-35680f356dfd?w=400&h=300&fit=crop&q=80";
    public const string TechnologyCategory = "https://images.unsplash.com/photo-1518770660439-4636190af475?w=400&h=300&fit=crop&q=80";
    public const string EducationCategory = "https://images.unsplash.com/photo-1503676260728-1c00da094a0b?w=400&h=300&fit=crop&q=80";
    public const string HealthCategory = "https://images.unsplash.com/photo-1505751172876-fa1923c5c528?w=400&h=300&fit=crop&q=80";
    
    // Provider Profile
    public const string DefaultProviderBanner = "https://images.unsplash.com/photo-1497366216548-37526070297c?w=1200&h=400&fit=crop&q=80";
    
    // Placeholder for no image
    public const string NoImagePlaceholder = "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='400' height='300' viewBox='0 0 400 300'%3E%3Crect fill='%23f3f4f6' width='400' height='300'/%3E%3Ctext fill='%239ca3af' font-family='sans-serif' font-size='24' dy='10.5' font-weight='bold' x='50%25' y='50%25' text-anchor='middle'%3Eلا توجد صورة%3C/text%3E%3C/svg%3E";

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

        return $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(initials)}&background={bgColor}&color=fff&size=200&bold=true&format=svg";
    }

    /// <summary>
    /// Get service image based on category or subcategory - INTELLIGENT MATCHING
    /// </summary>
    public static string GetServiceImage(string? categoryName = null, string? existingUrl = null)
    {
        if (!string.IsNullOrEmpty(existingUrl))
            return existingUrl;

        if (string.IsNullOrEmpty(categoryName))
            return DefaultService;

        var lowerName = categoryName.ToLower();

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
        if (!string.IsNullOrEmpty(existingUrl))
            return existingUrl;

        if (string.IsNullOrEmpty(categoryName))
            return DefaultCategory;

        var lowerName = categoryName.ToLower();

        // Home Services - خدمات منزلية
        if (lowerName.Contains("منزل") || lowerName.Contains("home") || 
            lowerName.Contains("بيت") || lowerName.Contains("house"))
            return HomeServicesCategory;

        // Construction - بناء وتشييد
        if (lowerName.Contains("بناء") || lowerName.Contains("construction") || 
            lowerName.Contains("تشييد") || lowerName.Contains("مقاول") || 
            lowerName.Contains("عمار") || lowerName.Contains("building"))
            return ConstructionCategory;

        // Technology - تقنية
        if (lowerName.Contains("تقني") || lowerName.Contains("tech") || 
            lowerName.Contains("معلومات") || lowerName.Contains("it") || 
            lowerName.Contains("كمبيوتر") || lowerName.Contains("برمج"))
            return TechnologyCategory;

        // Education - تعليم
        if (lowerName.Contains("تعليم") || lowerName.Contains("education") || 
            lowerName.Contains("دراس") || lowerName.Contains("تدريب") || 
            lowerName.Contains("كورس") || lowerName.Contains("training"))
            return EducationCategory;

        // Health - صحة
        if (lowerName.Contains("صحة") || lowerName.Contains("health") || 
            lowerName.Contains("طب") || lowerName.Contains("medical") || 
            lowerName.Contains("علاج") || lowerName.Contains("رياضة"))
            return HealthCategory;

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
        
        return $"https://ui-avatars.com/api/?name={Uri.EscapeDataString(initials)}&background={color}&color=fff&size=200&bold=true&format=svg";
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
