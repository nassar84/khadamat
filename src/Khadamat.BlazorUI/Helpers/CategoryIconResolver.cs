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
        { "معامل", "labs" },
        { "مراكز طبية", "hospitals" },
        { "علاج طبيعي", "health" },
        { "علاج طبيعى", "health" },
        { "مراكز اشعة", "radiology" },
        // Extended medical subcategory names (matching DB exact names)
        { "جلدية وتناسلية", "dermatology" },
        { "نساء وتوليد", "clinics" },
        { "مسالك بولية", "hospitals" },
        { "جراحة", "hospitals" },
        { "جراحة عامة", "hospitals" },
        { "جراحة عظام", "orthopedics" },
        { "جراحة تجميلية", "clinics" },
        { "نفسية", "clinics" },
        { "طوارق", "emergency" },
        { "قلب واوعية دموية", "clinics" },
        { "صدر وجهاز تنفسي", "clinics" },
        { "كبد وجهاز هضمي", "clinics" },
        { "جهاز هضمي", "clinics" },
        { "غدد صماء", "clinics" },
        { "اورام", "hospitals" },
        { "اطفال وحديثي الولادة", "pediatrics" },
        { "امراض نساء", "clinics" },
        { "مركز نساء وتوليد", "clinics" },

        { "دروس خصوصية", "tutoring" },
        { "كورسات", "courses" },
        { "حضانات", "nurseries" },
        { "مراكز تدريب", "training_centers" },
        { "محفظين قرآن", "education" },
        { "تحفيظ قران", "education" },
        { "مدارس", "education" },

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
    /// Slug icons (generic) come from images/categories/.
    /// Prioritizes database-stored images (original, parent, grandparent),
    /// falling back to the slug map, and then to a default icon.
    /// </summary>
    public static string GetIconUrl(
        string categoryName, 
        string? originalImageUrl = null, 
        string? entityFolder = null, 
        string? parentCategoryName = null,
        string? parentImageUrl = null,
        string? parentEntityFolder = null,
        string? grandparentImageUrl = null,
        string? grandparentEntityFolder = null)
    {
        const string slugFolder = "images/categories";

        // 1. Check database images first (original, parent, then grandparent)
        if (!string.IsNullOrEmpty(originalImageUrl))
            return ImagePathResolver.Resolve(originalImageUrl, entityFolder ?? slugFolder);

        if (!string.IsNullOrEmpty(parentImageUrl))
            return ImagePathResolver.Resolve(parentImageUrl, parentEntityFolder ?? slugFolder);

        if (!string.IsNullOrEmpty(grandparentImageUrl))
            return ImagePathResolver.Resolve(grandparentImageUrl, grandparentEntityFolder ?? slugFolder);

        // 2. Try to resolve a slug icon from the generic slug folder based on name
        if (!string.IsNullOrEmpty(categoryName))
        {
            string key = categoryName.Trim();
            if (_slugMap.TryGetValue(key, out string? slug))
                return $"{slugFolder}/{slug}.png";

            // Hierarchical fallback: try parent
            if (!string.IsNullOrEmpty(parentCategoryName))
            {
                string parentKey = parentCategoryName.Trim();
                if (_slugMap.TryGetValue(parentKey, out string? parentSlug))
                    return $"{slugFolder}/{parentSlug}.png";
            }
        }

        // 3. Ultimate fallback
        return $"{slugFolder}/other_services.png";
    }

    /// <summary>Resolve with entity folder set to images/maincategories</summary>
    public static string GetMainCategoryIcon(string categoryName, string? originalImageUrl = null, string? parentCategoryName = null)
        => GetIconUrl(categoryName, originalImageUrl, ImagePathResolver.MainCategories, parentCategoryName);

    /// <summary>Resolve with entity folder set to images/categories</summary>
    public static string GetCategoryIcon(string categoryName, string? originalImageUrl = null, string? parentCategoryName = null, string? parentImageUrl = null)
        => GetIconUrl(categoryName, originalImageUrl, ImagePathResolver.Categories, parentCategoryName, parentImageUrl, ImagePathResolver.MainCategories);

    /// <summary>Resolve with entity folder set to images/subcategories</summary>
    public static string GetSubCategoryIcon(string categoryName, string? originalImageUrl = null, string? parentCategoryName = null, string? parentImageUrl = null, string? grandparentImageUrl = null)
        => GetIconUrl(categoryName, originalImageUrl, ImagePathResolver.SubCategories, parentCategoryName, parentImageUrl, ImagePathResolver.Categories, grandparentImageUrl, ImagePathResolver.MainCategories);
}
