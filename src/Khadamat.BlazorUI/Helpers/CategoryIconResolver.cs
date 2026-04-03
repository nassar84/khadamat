using System.Collections.Generic;

namespace Khadamat.BlazorUI.Helpers;

public static class CategoryIconResolver
{
    private static readonly Dictionary<string, string> _slugMap = new()
    {
        { "صحة", "health" },
        { "تعليم", "education" },
        { "متاجر", "stores" },
        { "ماكولات ومشروبات", "food" },
        { "مكاتب", "offices" },
        { "حرفيون", "crafts" },
        { "تسوق اون لين", "online_shopping" },
        { "مواصلات", "transportation" },
        { "صيانة سيارات", "auto_repair" },
        { "خدمات حكومية", "gov_services" },
        { "جمعيات واعمال خيرية", "charities" },
        { "تمويل وبنوك", "finance" },
        { "متجر السلع", "marketplace" },
        { "خدمات اخرى", "other_services" },
        
        { "عيادات", "clinics" },
        { "اسنان", "dentist" },
        { "اطفال", "pediatrics" },
        { "عيون", "ophthalmology" },
        { "جلدية", "dermatology" },
        { "باطنة", "internal_medicine" },
        { "عظام", "orthopedics" },
        { "مخ واعصاب", "neurology" },
        { "انف واذن وحنجرة", "ent" },
        { "صيدليات", "pharmacies" },
        { "مستشفيات", "hospitals" },
        { "معامل تحاليل", "labs" },
        { "مراكز اشعة", "radiology" },

        { "دروس خصوصية", "tutoring" },
        { "كورسات", "courses" },
        { "حضانات", "nurseries" },
        { "مراكز تدريب", "training_centers" },

        { "سباكة", "plumbing" },
        { "كهرباء", "electricity" },
        { "نجارة", "carpentry" },
        { "نقاشة ودهانات", "painting" },
        { "صيانة اجهزة منزلية", "appliances_repair" },

        { "توصيل طلبات", "delivery" },
        { "نقل اثاث", "furniture_moving" },
        { "تاكسي ورحلات", "taxi" },

        { "الأثاث", "furniture" },
        { "الأجهزة الإلكترونية", "electronics" },
        { "الأجهزة المنزلية", "home_appliances" },
        { "السيارات والمركبات", "vehicles" },
        { "الحيوانات الأليفة", "pets" },
        { "الملابس والأزياء", "clothing" },
        { "ألعاب وأطفال", "toys" },
        { "أدوات رياضية", "sports" },
        { "كتب وأدوات تعليمية", "books" },
        { "أدوات منزلية", "housewares" },
        { "أدوات ومعدات", "tools" },
        { "أشياء متنوعة", "miscellaneous" },

        // Optimized mappings (reusing existing assets where specific ones are unavailable)
        { "مركز جراحة", "hospitals" }, // Reuse hospital icon
        { "مدرسة", "education" },     // Reuse education icon
        { "جامعة", "education" },      // Reuse education icon
        { "الأثاث", "housewares" },    // Reuse housewares icon
        { "الأجهزة الإلكترونية", "online_shopping" }, // Reuse online shopping icon

        // Newly downloaded Emoji Mappings
        // Health
        { "صيدلية 24 ساعة", "pharmacy_24" },
        { "صيدلية منزلية", "home_pharmacy" },
        { "مستلزمات طبية", "medical_supplies" },
        { "مستشفى عام", "general_hospital" },
        { "طوارئ", "emergency" },
        { "تحاليل دم", "blood_test" },
        { "تحاليل شاملة", "comprehensive_test" },
        { "مسحات", "swab_test" },
        { "اشعة مقطعية", "ct_scan" },
        { "سونار", "ultrasound" },
        { "رنين مغناطيسي", "mri" },

        // Education
        { "ابتدائي", "primary_school" },
        { "اعدادي", "middle_school" },
        { "ثانوي", "high_school" },
        { "لغات", "languages" },
        { "مواد علمية", "science" },
        { "مواد ادبية", "literature" },
        { "برمجة", "programming" },
        { "لغة انجليزية", "english" },
        { "لغة المانية", "german" },
        { "فوتوشوب", "graphic_design" },
        { "جرافيك ديزاين", "graphic_design" },
        { "تسويق الكتروني", "digital_marketing" },
        { "حضانة لغات", "nursery_languages" },
        { "تنمية مهارات", "skills_development" },
        { "تخاطب", "speech_therapy" },
        { "تنمية بشرية", "human_development" },
        { "ادارة اعمال", "business_admin" },
        { "محاسبة", "accounting" },

        // Craftsmen
        { "تأسيس سباكة", "plumbing_ops" },
        { "صيانة اعطال", "plumbing_ops" },
        { "تركيب ادوات صحية", "sanitary_ware" },
        { "تسليك بلاعات", "drain_cleaning" },
        { "تأسيس كهرباء", "electrical_setup" },
        { "تركيب نجفات", "chandeliers" },
        { "صيانة اجهزة", "electrical_setup" },
        { "لوحات توزيع", "distribution_boards" },
        { "تصنيع اثاث", "furniture_making" },
        { "تصليح ابواب", "door_repair" },
        { "تجديد مطابخ", "furniture_making" },
        { "فك وتركيب", "dismantle_assemble" },
        { "دهانات داخلية", "interior_painting" },
        { "ديكورات جبس", "wallpaper_decor" },
        { "ورق حائط", "wallpaper_decor" },
        { "ثلاجات", "fridges" },
        { "غسالات", "washers" },
        { "تكييفات", "acs" },
        { "بوتاجازات", "appliance_repair_ops" },

        // Transportation
        { "دليفري مطاعم", "restaurant_delivery" },
        { "شحن محافظات", "shipping" },
        { "توصيل هدايا", "gift_delivery" },
        { "ونش رفع", "lifting_crane" },
        { "فك وتركيب اثاث", "lifting_crane" },
        { "سيارات نقل", "transport_vehicles" },
        { "مشاوير خاصة", "private_rides" },
        { "توصيل مطار", "airport_transfer" },
        { "رحلات سياحية", "tours" },

        // Marketplace (Specific Items)
        { "غرف نوم", "bedrooms" },
        { "غرف معيشة", "living_rooms" },
        { "سفرة وطاولات", "dining_tables" },
        { "كراسي ومكاتب", "chairs_desks" },
        { "أثاث مكتبي", "chairs_desks" },
        { "أثاث أطفال", "kids_furniture" },
        { "أثاث خارجي", "chairs_desks" },
        { "أثاث مستعمل", "chairs_desks" },
        { "أثاث جديد", "chairs_desks" },

        { "موبايلات", "mobiles" },
        { "تابلت", "mobiles" },
        { "لابتوب", "computers" },
        { "كمبيوتر مكتبي", "computers" },
        { "شاشات", "monitors" },
        { "طابعات", "printers" },
        { "كاميرات", "cameras" },
        { "سماعات", "headphones" },
        { "أجهزة ألعاب", "gaming_consoles" },

        { "ميكروويف", "appliance_repair_ops" },
        { "مراوح", "acs" },
        { "سخانات", "appliance_repair_ops" },
        { "أجهزة مطبخ صغيرة", "appliance_repair_ops" },

        { "سيارات", "cars" },
        { "موتوسيكلات", "motorcycles" },
        { "دراجات", "bicycles" },
        { "قطع غيار", "spare_parts" },
        { "إكسسوارات سيارات", "spare_parts" },

        { "كلاب", "dogs" },
        { "قطط", "cats" },
        { "طيور", "birds" },
        { "أسماك", "fish" },
        { "أدوات الحيوانات", "dogs" },
        { "طعام الحيوانات", "dogs" },

        { "ملابس رجالي", "mens_clothing" },
        { "ملابس نسائي", "womens_clothing" },
        { "ملابس أطفال", "womens_clothing" },
        { "أحذية", "shoes" },
        { "شنط", "bags" },
        { "إكسسوارات", "watches" },
        { "ساعات", "watches" },

        { "ألعاب أطفال", "kids_toys" },
        { "عربيات أطفال", "strollers" },
        { "سرير أطفال", "kids_furniture" },
        { "أدوات تعليمية", "kids_toys" },

        { "أجهزة رياضية منزلية", "dumbbells" },
        { "أثقال", "dumbbells" },
        { "أدوات جيم", "dumbbells" },
        { "دراجات رياضية", "sports_bikes" },
        { "ملابس رياضية", "womens_clothing" },

        { "كتب مدرسية", "school_books" },
        { "كتب جامعية", "school_books" },
        { "روايات", "school_books" },
        { "أدوات مكتبية", "stationery" },

        { "أدوات مطبخ", "appliance_repair_ops" },
        { "أدوات ديكور", "wallpaper_decor" },
        { "سجاد", "wallpaper_decor" },
        { "ستائر", "wallpaper_decor" },
        { "إضاءة", "chandeliers" },

        { "أدوات كهربائية", "distribution_boards" },
        { "أدوات يدوية", "dismantle_assemble" },
        { "معدات صناعية", "lifting_crane" },
        { "أخرى", "miscellaneous" }
    };

    /// <summary>
    /// Returns the local path to the colorful icon for a given category name.
    /// Falls back to the parent's icon, original image URL, or a default icon if neither is available.
    /// </summary>
    public static string GetIconUrl(string categoryName, string? originalImageUrl = null, string? fallbackPrefix = null, string? parentCategoryName = null)
    {
        if (string.IsNullOrEmpty(categoryName))
        {
            if (string.IsNullOrEmpty(originalImageUrl)) return "images/categories/gen/other_services.png";
            return ConstructFallbackUrl(originalImageUrl, fallbackPrefix);
        }

        string key = categoryName.Trim();
        if (_slugMap.TryGetValue(key, out string? slug))
        {
            return $"images/categories/gen/{slug}.png";
        }

        // 🟢 Hierarchical Fallback: If child fails, try parent
        if (!string.IsNullOrEmpty(parentCategoryName))
        {
            string parentKey = parentCategoryName.Trim();
            if (_slugMap.TryGetValue(parentKey, out string? parentSlug))
            {
                return $"images/categories/gen/{parentSlug}.png";
            }
        }

        // If no match in slugMap, fallback to originalImageUrl or a default generic icon
        if (string.IsNullOrEmpty(originalImageUrl))
        {
            return "images/categories/gen/other_services.png";
        }
        
        return ConstructFallbackUrl(originalImageUrl, fallbackPrefix);
    }

    private static string ConstructFallbackUrl(string? originalImageUrl, string? fallbackPrefix)
    {
        if (string.IsNullOrEmpty(originalImageUrl)) return "images/categories/default.jpg";
        
        // Handle absolute URLs
        if (originalImageUrl.StartsWith("http") || originalImageUrl.StartsWith("//"))
        {
            return originalImageUrl;
        }

        // Normalize: Remove leading slash if any
        string fileName = originalImageUrl.TrimStart('/');

        // Apply prefix if it's just a filename
        if (!string.IsNullOrEmpty(fallbackPrefix) && !fileName.Contains("/"))
        {
            return $"{fallbackPrefix}/{fileName}";
        }

        return fileName;
    }
}
