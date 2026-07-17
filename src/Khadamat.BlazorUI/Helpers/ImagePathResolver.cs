namespace Khadamat.BlazorUI.Helpers;

/// <summary>
/// ============================================================
///  IMAGE PATH RESOLVER — المرجع الرسمي لمسارات الصور
/// ============================================================
///
/// === مبدأ العمل ===
///   قاعدة البيانات تخزّن اسم الملف فقط (بدون مسار).
///   هذا الـ Resolver يحوّل اسم الملف إلى مسار كامل قابل
///   للعرض في <img src="...">.
///
/// === هيكل المجلدات (wwwroot/images/) ===
///
///   images/
///   ├── maincategories/   → صور الفئات الرئيسية (MainCategory.ImageUrl)
///   ├── categories/       → صور الفئات (Category.ImageUrl)
///   ├── subcategories/    → صور الفئات الفرعية (SubCategory.ImageUrl)
///   ├── services/         → صور الخدمات (Service.ImageUrl)
///   ├── ads/              → صور الإعلانات (Ad.ImageUrl / Advertisement.ImageUrl)
///   ├── marketplace/      → صور منتجات المتجر (MarketplaceItem.ImageUrl / MarketplaceImage.ImageUrl)
///   ├── users/            → صور المستخدمين (ApplicationUser.ProfileImageUrl)
///   ├── placeholders/     → صور بديلة ثابتة (لا ترفع من المستخدم)
///   └── defaults/         → صور default لكل نوع
///
/// === نظام التسمية (Naming Convention) ===
///   للخدمات: s_{categoryId}_{serviceId}{ext}
///   مثال: s_2_8.jpg  ← حيث 2 هو رقم الفئة و 8 هو رقم الخدمة
///   لباقي الأنواع: {timestamp_ms}_{guid_compact}{ext}
///   يُولَّد الاسم النهائي للخدمات في ServicesController.cs بعد الحفظ.
///
/// === ما يُخزَّن في قاعدة البيانات ===
///   اسم الملف فقط ← "s_2_8.jpg"
///   وليس المسار الكامل ← ❌ لا يُخزَّن "/images/services/s_2_8.jpg"
///
/// === الاستخدام في الواجهة ===
///   <img src="@ImagePathResolver.Service(service.ImageUrl)" />
///   <img src="@ImagePathResolver.MainCategory(mc.ImageUrl)" />
///   <img src="@ImagePathResolver.User(user.ProfileImageUrl)" />
///
/// === API endpoint للرفع ===
///   POST /v1/upload?type=services
///   POST /v1/upload?type=maincategories
///   POST /v1/upload?type=categories
///   POST /v1/upload?type=subcategories
///   POST /v1/upload?type=ads
///   POST /v1/upload?type=marketplace
///   POST /v1/upload?type=users
///
/// ============================================================
/// </summary>
public static class ImagePathResolver
{
    // ============================================================
    //  Folder constants — يطابق هيكل wwwroot/images/
    // ============================================================
    public const string MainCategories = "images/maincategories";
    public const string Categories     = "images/categories";
    public const string SubCategories  = "images/subcategories";
    public const string Services       = "images/services";
    public const string Ads            = "images/ads";
    public const string Marketplace    = "images/marketplace";
    public const string Users          = "images/users";
    public const string Placeholders   = "images/placeholders";
    public const string Defaults       = "images/defaults";

    // ============================================================
    //  Default fallback filenames (موجودة في images/defaults/)
    // ============================================================
    public const string DefaultImage    = "default.png";
    public const string DefaultService  = "default_service.png";
    public const string DefaultUser     = "default-user.png";
    public const string DefaultAd       = "default-ad.png";
    public const string DefaultProduct  = "default-product.png";
    public const string NoImage         = "no-image.png";

    // ============================================================
    //  Core Resolver
    // ============================================================
    /// <summary>
    /// يحوّل اسم الملف المخزَّن في قاعدة البيانات إلى مسار عرض كامل.
    ///
    /// السلوك:
    ///   - null / فارغ   → folder/defaultImage
    ///   - http/data:     → يُعاد كما هو (رابط خارجي)
    ///   - images/...     → يُعاد كما هو (مسار مكتمل)
    ///   - /images/...    → يُزال الـ / الأول فقط
    ///   - filename.jpg   → folder/filename.jpg   ← الحالة الطبيعية
    /// </summary>
    public static string Resolve(string? imageName, string folder, string defaultImage = DefaultImage)
    {
        if (string.IsNullOrWhiteSpace(imageName))
            return $"{folder}/{defaultImage}";

        var name = imageName.Replace("\\", "/").Trim();

        // رابط خارجي أو data URI → لا نغير شيئاً
        if (name.StartsWith("http", StringComparison.OrdinalIgnoreCase) ||
            name.StartsWith("data:", StringComparison.OrdinalIgnoreCase) ||
            name.StartsWith("//"))
            return name;

        // مسار مكتمل بالفعل
        if (name.StartsWith("images/", StringComparison.OrdinalIgnoreCase))
            return name;

        // يبدأ بـ / → نزيل الـ slash الأول
        if (name.StartsWith("/"))
            return name.TrimStart('/');

        // اسم ملف فقط (الحالة الطبيعية من قاعدة البيانات)
        return $"{folder}/{name}";
    }

    // ============================================================
    //  Entity-specific helpers — استخدم هذه مباشرة في الـ Razor
    // ============================================================

    /// <summary>صور الفئات الرئيسية → images/maincategories/{name}</summary>
    public static string MainCategory(string? name) => Resolve(name, MainCategories, DefaultImage);

    /// <summary>صور الفئات → images/categories/{name}</summary>
    public static string Category(string? name)
    {
        // Seed images were converted from JPG to PNG (160x120)
        if (!string.IsNullOrEmpty(name) && name.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
            name = System.IO.Path.ChangeExtension(name, ".png");
        return Resolve(name, Categories, DefaultImage);
    }

    /// <summary>صور الفئات الفرعية → images/subcategories/{name}</summary>
    public static string SubCategory(string? name)
    {
        // Seed images were converted from JPG to PNG (160x120)
        if (!string.IsNullOrEmpty(name) && name.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase))
            name = System.IO.Path.ChangeExtension(name, ".png");
        return Resolve(name, SubCategories, DefaultImage);
    }

    /// <summary>صور الخدمات → images/services/{name}</summary>
    public static string Service(string? name) => Resolve(name, Services, DefaultService);

    /// <summary>صور الإعلانات → images/ads/{name}</summary>
    public static string Ad(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return $"{Defaults}/{DefaultAd}";
        return Resolve(name, Ads, DefaultAd);
    }

    /// <summary>صور منتجات المتجر → images/marketplace/{name}</summary>
    public static string MarketplaceItem(string? name) => Resolve(name, Marketplace, DefaultProduct);

    /// <summary>صور المستخدمين → images/users/{name}</summary>
    public static string User(string? name) => Resolve(name, Users, DefaultUser);

    /// <summary>صور placeholder → images/placeholders/{name}</summary>
    public static string Placeholder(string? name) => Resolve(name, Placeholders, NoImage);

    // ============================================================
    //  Upload type → folder mapping (للاستخدام في ApiClient)
    // ============================================================
    /// <summary>
    /// يُعيد نوع الرفع المناسب لكل endpoint.
    /// استخدم هذا مع POST /v1/upload?type={result}
    /// </summary>
    public static string GetUploadType(EntityImageType entityType) => entityType switch
    {
        EntityImageType.MainCategory  => "maincategories",
        EntityImageType.Category      => "categories",
        EntityImageType.SubCategory   => "subcategories",
        EntityImageType.Service       => "services",
        EntityImageType.Ad            => "ads",
        EntityImageType.Marketplace   => "marketplace",
        EntityImageType.User          => "users",
        _                             => "general"
    };

    /// <summary>
    /// Resolve by entity type (for generic UI components).
    /// </summary>
    public static string ResolveByType(string? imageName, EntityImageType entityType) => entityType switch
    {
        EntityImageType.MainCategory  => MainCategory(imageName),
        EntityImageType.Category      => Category(imageName),
        EntityImageType.SubCategory   => SubCategory(imageName),
        EntityImageType.Service       => Service(imageName),
        EntityImageType.Ad            => Ad(imageName),
        EntityImageType.Marketplace   => MarketplaceItem(imageName),
        EntityImageType.User          => User(imageName),
        _                             => Resolve(imageName, "images/general", DefaultImage)
    };

    /// <summary>
    /// Returns the icon path for a category slug.
    /// e.g. GetCategoryIconPath("health") → "images/categories/health.png"
    /// </summary>
    public static string CategoryIcon(string slug) => $"{Categories}/{slug}.png";
}

/// <summary>
/// أنواع الكيانات التي تملك صور — يُستخدم مع ImagePathResolver
/// </summary>
public enum EntityImageType
{
    MainCategory,
    Category,
    SubCategory,
    Service,
    Ad,
    Marketplace,
    User,
    General
}
