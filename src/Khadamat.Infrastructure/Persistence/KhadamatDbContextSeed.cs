using Khadamat.Domain.Entities;
using Khadamat.Domain.Enums;
using Khadamat.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Khadamat.Infrastructure.Persistence;

public static class KhadamatDbContextSeed
{
    public static async Task SeedAsync(KhadamatDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        try 
        {
            await SeedRolesAsync(roleManager);
            await SeedUsersAsync(userManager);

            if (!await context.MainCategories.AnyAsync())
            {
                var mainCategories = new List<MainCategory>
                {
                    new MainCategory { Name = "صحة", Icon = "🏥", Color = "medical", DisplayOrder = 1, ImageUrl = "cat_1.png" },
                    new MainCategory { Name = "تعليم", Icon = "🎓", Color = "education", DisplayOrder = 2, ImageUrl = "cat_2.png" },
                    new MainCategory { Name = "متاجر", Icon = "🏪", Color = "stores", DisplayOrder = 3, ImageUrl = "cat_3.png" },
                    new MainCategory { Name = "ماكولات ومشروبات", Icon = "🍲", Color = "food", DisplayOrder = 4, ImageUrl = "cat_4.png" },
                    new MainCategory { Name = "مكاتب", Icon = "🏢", Color = "offices", DisplayOrder = 5, ImageUrl = "cat_5.png" },
                    new MainCategory { Name = "حرفيون", Icon = "🛠️", Color = "crafts", DisplayOrder = 6, ImageUrl = "cat_8.png" },
                    new MainCategory { Name = "تسوق اون لين", Icon = "🛒", Color = "online", DisplayOrder = 7, ImageUrl = "cat_9.png" },
                    new MainCategory { Name = "مواصلات", Icon = "🚗", Color = "transport", DisplayOrder = 8, ImageUrl = "cat_10.png" },
                    new MainCategory { Name = "صيانة سيارات", Icon = "🔧", Color = "auto", DisplayOrder = 9, ImageUrl = "cat_11.png" },
                    new MainCategory { Name = "خدمات حكومية", Icon = "🏛️", Color = "gov", DisplayOrder = 10, ImageUrl = "cat_12.png" },
                    new MainCategory { Name = "متجر السلع", Icon = "🛍️", Color = "marketplace", DisplayOrder = 11, ImageUrl = "cat_7.png" },
                    new MainCategory { Name = "خدمات اخرى", Icon = "✨", Color = "other", DisplayOrder = 12, ImageUrl = "cat_6.png" }
                };
                await context.MainCategories.AddRangeAsync(mainCategories);
                await context.SaveChangesAsync();
            }
            else 
            {
                // Fix existing categories if they were seeded without images
                var existing = await context.MainCategories.ToListAsync();
                bool changed = false;
                var imageMap = new Dictionary<string, string>
                {
                    { "صحة", "cat_1.png" }, { "تعليم", "cat_2.png" }, { "متاجر", "cat_3.png" },
                    { "ماكولات ومشروبات", "cat_4.png" }, { "مكاتب", "cat_5.png" }, { "حرفيون", "cat_8.png" },
                    { "تسوق اون لين", "cat_9.png" }, { "مواصلات", "cat_10.png" }, { "صيانة سيارات", "cat_11.png" },
                    { "خدمات حكومية", "cat_1.png" }, { "متجر السلع", "cat_7.png" }, { "خدمات اخرى", "cat_6.png" }
                };

                foreach (var cat in existing)
                {
                    if (string.IsNullOrEmpty(cat.ImageUrl) && imageMap.TryGetValue(cat.Name, out var img))
                    {
                        cat.ImageUrl = img;
                        changed = true;
                    }
                }
                if (changed) await context.SaveChangesAsync();
            }

            if (!await context.Categories.AnyAsync())
            {
                await SeedCategoriesAndSubCategoriesAsync(context);
            }

            if (!await context.MarketplaceCategories.AnyAsync())
            {
                await SeedMarketplaceCategoriesAsync(context);
            }

            await SeedLocationsAsync(context);
            await SeedRandomUsersAsync(userManager, context);
            await SeedServicesAsync(context);
            await SeedRatingsAsync(context);
            await SeedMessagesAsync(context);

            if (!await context.Ads.AnyAsync())
            {
                await SeedAdsAsync(context);
            }

            if (!await context.SubscriptionPlans.AnyAsync())
            {
                await SeedSubscriptionPlansAsync(context);
            }

            if (!await context.MarketplaceItems.AnyAsync())
            {
                await SeedMarketplaceItemsAsync(context);
            }

            // ── Advertisement System Data ───────────────────────────────
            if (!await context.AdPackages.AnyAsync())
                await SeedAdPackagesAsync(context);

            if (!await context.PointRewardRules.AnyAsync())
                await SeedPointRewardRulesAsync(context);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SEED ERROR: {ex.Message}");
        }
    }

    private static async Task SeedMarketplaceItemsAsync(KhadamatDbContext context)
    {
        var users = await context.Users.Take(5).ToListAsync();
        var subCats = await context.MarketplaceSubCategories.Take(10).ToListAsync();
        var cities = await context.Cities.Take(2).ToListAsync();

        if (!users.Any() || !subCats.Any()) return;

        var random = new Random();
        for (int i = 1; i <= 10; i++)
        {
            var user = users[random.Next(users.Count)];
            var subCat = subCats[random.Next(subCats.Count)];
            var city = cities.Any() ? cities[random.Next(cities.Count)] : null;

            var item = new MarketplaceItem(
                $"سلعة تجريبية {i}",
                $"وصف السلعة التجريبية رقم {i}. هذه السلعة مخصصة للاختبار فقط.",
                random.Next(100, 5000),
                user.Id,
                subCat.CategoryId,
                "01000000000",
                subCat.Id,
                city?.Id,
                i % 2 == 0 ? "New" : "Used"
            );

            if (i <= 4) item.Approve(); // Approve some items
            if (i == 1 || i == 5) item.SetFeatured(7);
            if (i == 2 || i == 6) item.SetPromoted(7);

            context.MarketplaceItems.Add(item);
        }
        await context.SaveChangesAsync();
    }

    private static async Task SeedSubscriptionPlansAsync(KhadamatDbContext context)
    {
        var plans = new List<SubscriptionPlan>
        {
            new SubscriptionPlan("الباقة التجريبية (مجانية)", 0, 30, 2, false),
            new SubscriptionPlan("الباقة الأساسية", 150, 30, 10, false),
            new SubscriptionPlan("الباقة المميزة (Premium)", 400, 30, 50, true),
            new SubscriptionPlan("الباقة السنوية للمحترفين", 1500, 365, 100, true)
        };
        await context.SubscriptionPlans.AddRangeAsync(plans);
        await context.SaveChangesAsync();
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = { "SuperAdmin", "SystemAdmin", "User" };
        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }
    }

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
    {
        // System Admin
        if (await userManager.FindByEmailAsync("admin@khadamat.com") == null)
        {
            var admin = new ApplicationUser { UserName = "Admin", Email = "admin@khadamat.com", FullName = "System Admin", Role = UserRole.SystemAdmin, EmailConfirmed = true };
            await userManager.CreateAsync(admin, "Admin@123");
            await userManager.AddToRoleAsync(admin, "SystemAdmin");
        }

        // Super Admin
        if (await userManager.FindByNameAsync("SuperAdmin") == null)
        {
            var superAdmin = new ApplicationUser { UserName = "SuperAdmin", Email = "superadmin@khadamat.com", FullName = "Super Admin User", Role = UserRole.SuperAdmin, EmailConfirmed = true };
            await userManager.CreateAsync(superAdmin, "Admin@123");
            await userManager.AddToRoleAsync(superAdmin, "SuperAdmin");
        }

        // Regular User
        if (await userManager.FindByNameAsync("RegularUser") == null)
        {
            var regularUser = new ApplicationUser { UserName = "RegularUser", Email = "regular@khadamat.com", FullName = "Regular User", Role = UserRole.User, EmailConfirmed = true };
            await userManager.CreateAsync(regularUser, "Admin@123");
            await userManager.AddToRoleAsync(regularUser, "User");
        }
    }

    private static async Task SeedRandomUsersAsync(UserManager<ApplicationUser> userManager, KhadamatDbContext context)
    {
        if (await userManager.Users.CountAsync() > 10) return;

        var cities = await context.Cities.ToListAsync();
        string[] names = { "أحمد", "محمد", "سارة", "ليلى", "هاني", "لينا", "كريم", "ليان", "ياسر" };
        var random = new Random();

        for (int i = 1; i <= 10; i++)
        {
            var name = names[random.Next(names.Length)];
            var email = $"user{i}@khadamat.com";
            if (await userManager.FindByEmailAsync(email) == null)
            {
                var city = cities.Any() ? cities[random.Next(cities.Count)] : null;
                var user = new ApplicationUser
                {
                    UserName = $"user{i}",
                    Email = email,
                    FullName = $"{name} {i}",
                    Role = UserRole.User,
                    IsActive = true,
                    EmailConfirmed = true,
                    CityId = city?.Id,
                    IsProvider = i <= 5
                };
                await userManager.CreateAsync(user, "User@123");
                await userManager.AddToRoleAsync(user, "User");

                if (user.IsProvider)
                {
                    var profile = new ProviderProfile
                    {
                        UserId = user.Id,
                        BusinessName = $"مؤسسة {name} للخدمات",
                        Bio = "نعمل بجودة عالية وإتقان لخدمتكم دائماً.",
                        ContactNumber = "010" + random.Next(10000000, 99999999),
                        Verified = true,
                        CityId = city?.Id
                    };
                    await context.ProviderProfiles.AddAsync(profile);
                    user.IsVerified = true;
                    await userManager.UpdateAsync(user);
                }
            }
        }
        await context.SaveChangesAsync();
    }

    private static async Task SeedServicesAsync(KhadamatDbContext context)
    {
        if (await context.Services.CountAsync() > 20) return;

        var providers = await context.ProviderProfiles.ToListAsync();
        var subCats = await context.SubCategories.ToListAsync();
        var cities = await context.Cities.ToListAsync();
        if (!providers.Any() || !subCats.Any()) return;

        var random = new Random();
        for (int i = 1; i <= 30; i++)
        {
            var provider = providers[random.Next(providers.Count)];
            var subCat = subCats[random.Next(subCats.Count)];
            var city = cities.Any() ? cities[random.Next(cities.Count)] : null;

            var service = new Service(
                subCategoryId: subCat.Id,
                categoryId: null,
                cityId: city?.Id,
                name: $"خدمة {subCat.Name} مميزة {i}",
                description: $"نقدم أفضل خدمات {subCat.Name} في بلدية {city?.City_Name_AR} بأسعار منافسة.",
                address: city?.City_Name_AR ?? "موقع العمل",
                providerProfileId: provider.Id,
                userCreated: provider.UserId
            );
            service.UpdateDetails(service.Name, service.Description, service.Address, price: random.Next(200, 3000));
            service.SetImage($"https://picsum.photos/seed/s{i}/800/600");
            service.Approve();
            context.Services.Add(service);
        }
        await context.SaveChangesAsync();
    }

    private static async Task SeedRatingsAsync(KhadamatDbContext context)
    {
        if (await context.Ratings.AnyAsync()) return;

        var services = await context.Services.ToListAsync();
        var users = await context.Users.Where(u => !u.IsProvider).ToListAsync();
        if (!services.Any() || !users.Any()) return;

        var random = new Random();
        foreach (var s in services)
        {
            var count = random.Next(1, 3);
            for (int i = 0; i < count; i++)
            {
                var user = users[random.Next(users.Count)];
                context.Ratings.Add(new Rating(s.Id, user.Id, random.Next(4, 6), "خدمة رائعة جداً، شكراً لكم!"));
            }
        }
        await context.SaveChangesAsync();
    }

    private static async Task SeedMessagesAsync(KhadamatDbContext context)
    {
        if (await context.Messages.AnyAsync()) return;

        var users = await context.Users.Take(5).ToListAsync();
        if (users.Count < 2) return;

        var random = new Random();
        for (int i = 0; i < 15; i++)
        {
            var sender = users[random.Next(users.Count)];
            var receiver = users[random.Next(users.Count)];
            if (sender.Id == receiver.Id) continue;

            context.Messages.Add(new Message(sender.Id, receiver.Id, "مرحباً، أود الاستفسار عن تفاصيل الخدمة المتاحة لديكم."));
        }
        await context.SaveChangesAsync();
    }

    private static async Task SeedMarketplaceCategoriesAsync(KhadamatDbContext context)
    {
        var marketplaceData = new Dictionary<string, List<string>>
        {
            { "الأثاث", new List<string> { "غرف نوم", "غرف معيشة", "سفرة وطاولات", "كراسي ومكاتب", "أثاث مكتبي", "أثاث أطفال", "أثاث خارجي", "أثاث مستعمل", "أثاث جديد" } },
            { "الأجهزة الإلكترونية", new List<string> { "موبايلات", "تابلت", "لابتوب", "كمبيوتر مكتبي", "شاشات", "طابعات", "كاميرات", "سماعات", "أجهزة ألعاب" } },
            { "الأجهزة المنزلية", new List<string> { "ثلاجات", "غسالات", "بوتاجازات", "ميكروويف", "تكييفات", "مراوح", "سخانات", "أجهزة مطبخ صغيرة" } },
            { "السيارات والمركبات", new List<string> { "سيارات", "موتوسيكلات", "دراجات", "قطع غيار", "إكسسوارات سيارات" } },
            { "الحيوانات الأليفة", new List<string> { "كلاب", "قطط", "طيور", "أسماك", "أدوات الحيوانات", "طعام الحيوانات" } },
            { "الملابس والأزياء", new List<string> { "ملابس رجالي", "ملابس نسائي", "ملابس أطفال", "أحذية", "شنط", "إكسسوارات", "ساعات" } },
            { "ألعاب وأطفال", new List<string> { "ألعاب أطفال", "عربيات أطفال", "سرير أطفال", "ملابس أطفال", "أدوات تعليمية" } },
            { "أدوات رياضية", new List<string> { "أجهزة رياضية منزلية", "أثقال", "أدوات جيم", "دراجات رياضية", "ملابس رياضية" } },
            { "كتب وأدوات تعليمية", new List<string> { "كتب مدرسية", "كتب جامعية", "روايات", "أدوات مكتبية" } },
            { "أدوات منزلية", new List<string> { "أدوات مطبخ", "أدوات ديكور", "سجاد", "ستائر", "إضاءة" } },
            { "أدوات ومعدات", new List<string> { "أدوات كهربائية", "أدوات يدوية", "معدات صناعية" } },
            { "أشياء متنوعة", new List<string> { "أخرى" } }
        };

        int displayOrder = 1;
        foreach (var categoryPair in marketplaceData)
        {
            var category = new MarketplaceCategory { Name = categoryPair.Key, DisplayOrder = displayOrder++ };
            await context.MarketplaceCategories.AddAsync(category);
            await context.SaveChangesAsync();

            int subOrder = 1;
            foreach (var subName in categoryPair.Value)
            {
                await context.MarketplaceSubCategories.AddAsync(new MarketplaceSubCategory { Name = subName, CategoryId = category.Id, DisplayOrder = subOrder++ });
            }
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedCategoriesAndSubCategoriesAsync(KhadamatDbContext context)
    {
        var mainCats = await context.MainCategories.ToDictionaryAsync(m => m.Name);

        if (mainCats.TryGetValue("حرفيون", out var crafts))
        {
            var cats = new List<Category> { new Category { Name = "سباكة", MainCategoryId = crafts.Id }, new Category { Name = "كهرباء", MainCategoryId = crafts.Id } };
            await context.Categories.AddRangeAsync(cats);
            await context.SaveChangesAsync();

            await context.SubCategories.AddRangeAsync(
                new SubCategory { Name = "تأسيس سباكة", CategoryId = cats[0].Id },
                new SubCategory { Name = "صيانة أدوات صحية", CategoryId = cats[0].Id },
                new SubCategory { Name = "تركيب نجفات", CategoryId = cats[1].Id }
            );
        }

        if (mainCats.TryGetValue("صحة", out var health))
        {
            var cats = new List<Category> { new Category { Name = "عيادات", MainCategoryId = health.Id } };
            await context.Categories.AddRangeAsync(cats);
            await context.SaveChangesAsync();

            await context.SubCategories.AddRangeAsync(
                new SubCategory { Name = "اسنان", CategoryId = cats[0].Id },
                new SubCategory { Name = "اطفال", CategoryId = cats[0].Id }
            );
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedLocationsAsync(KhadamatDbContext context)
    {
        if (await context.Governorates.AnyAsync()) return;

        var cairo = new Governorate { Governorate_Name_AR = "القاهرة", Governorate_Name_EN = "Cairo", DisplayOrder = 1, Approved = true };
        context.Governorates.Add(cairo);
        await context.SaveChangesAsync();

        context.Cities.AddRange(
            new City { GovernorateId = cairo.Id, City_Name_AR = "مدينة نصر", City_Name_EN = "Nasr City", Approved = true, DisplayOrder = 1 },
            new City { GovernorateId = cairo.Id, City_Name_AR = "المعادي", City_Name_EN = "Maadi", Approved = true, DisplayOrder = 2 }
        );
        await context.SaveChangesAsync();
    }

    private static async Task SeedAdsAsync(KhadamatDbContext context)
    {
        var now = DateTime.UtcNow;
        var ad = new Ad("مرحباً بكم في خدمات", "اكتشف أفضل المحترفين في مدينتك الآن.", now, now.AddMonths(1), "Slider");
        ad.SetMainImage("https://picsum.photos/seed/ad/1200/400");
        ad.Approve();
        context.Ads.Add(ad);
        await context.SaveChangesAsync();
    }

    // ────────────────────────────────────────────────────────────────────────
    // Phase 9: Seed Ad Packages (Basic / Silver / Gold / Platinum)
    // ────────────────────────────────────────────────────────────────────────
    private static async Task SeedAdPackagesAsync(KhadamatDbContext context)
    {
        var packages = new List<AdPackage>
        {
            new AdPackage(
                name: "الباقة الأساسية",
                price: 99,
                durationDays: 30,
                tier: AdPackageTier.Basic,
                maxAds: 1,
                isFeatured: false,
                isSponsored: false,
                isBanner: false,
                priorityBoost: 0,
                description: "إعلان واحد لمدة 30 يوماً"),

            new AdPackage(
                name: "الباقة الفضية",
                price: 199,
                durationDays: 30,
                tier: AdPackageTier.Silver,
                maxAds: 3,
                isFeatured: false,
                isSponsored: true,
                isBanner: false,
                priorityBoost: 2,
                description: "3 إعلانات ممولة في نتائج البحث"),

            new AdPackage(
                name: "الباقة الذهبية",
                price: 399,
                durationDays: 30,
                tier: AdPackageTier.Gold,
                maxAds: 5,
                isFeatured: true,
                isSponsored: true,
                isBanner: true,
                priorityBoost: 5,
                description: "مزودون مميزون + بانر رئيسي + إعلانات ممولة"),

            new AdPackage(
                name: "الباقة البلاتينية",
                price: 699,
                durationDays: 30,
                tier: AdPackageTier.Platinum,
                maxAds: 10,
                isFeatured: true,
                isSponsored: true,
                isBanner: true,
                priorityBoost: 10,
                description: "أفضل ظهور على المنصة مع كل المميزات")
        };

        await context.AdPackages.AddRangeAsync(packages);
        await context.SaveChangesAsync();
    }

    // ────────────────────────────────────────────────────────────────────────
    // Phase 10: Seed Point Reward Rules
    // ────────────────────────────────────────────────────────────────────────
    private static async Task SeedPointRewardRulesAsync(KhadamatDbContext context)
    {
        var rules = new List<PointRewardRule>
        {
            new PointRewardRule(PointActionType.Referral,       50, "50 نقطة لكل صديق يسجل بالرابط"),
            new PointRewardRule(PointActionType.Review,         10, "10 نقاط عند استلام تقييم من عميل"),
            new PointRewardRule(PointActionType.OrderCompleted, 20, "20 نقطة عند إتمام طلب خدمة")
        };

        await context.PointRewardRules.AddRangeAsync(rules);
        await context.SaveChangesAsync();
    }
}
