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
        // 1. Seed Roles
        await SeedRolesAsync(roleManager);

        // 2. Seed Users
        await SeedUsersAsync(userManager);

        // 3. Seed Main Categories
        if (!await context.MainCategories.AnyAsync())
        {
            var mainCategories = new List<MainCategory>
            {
                new MainCategory { Name = "صحة", Icon = "🏥", Color = "medical", DisplayOrder = 1 },
                new MainCategory { Name = "تعليم", Icon = "🎓", Color = "education", DisplayOrder = 2 },
                new MainCategory { Name = "متاجر", Icon = "🏪", Color = "stores", DisplayOrder = 3 },
                new MainCategory { Name = "ماكولات ومشروبات", Icon = "🍲", Color = "food", DisplayOrder = 4 },
                new MainCategory { Name = "مكاتب", Icon = "🏢", Color = "offices", DisplayOrder = 5 },
                new MainCategory { Name = "حرفيون", Icon = "🛠️", Color = "crafts", DisplayOrder = 6 },
                new MainCategory { Name = "تسوق اون لين", Icon = "🛒", Color = "online", DisplayOrder = 7 },
                new MainCategory { Name = "مواصلات", Icon = "🚗", Color = "transport", DisplayOrder = 8 },
                new MainCategory { Name = "صيانة سيارات", Icon = "🔧", Color = "auto", DisplayOrder = 9 },
                new MainCategory { Name = "خدمات حكومية", Icon = "🏛️", Color = "gov", DisplayOrder = 10 },
                new MainCategory { Name = "خدمات اخرى", Icon = "✨", Color = "other", DisplayOrder = 11 }
            };

            await context.MainCategories.AddRangeAsync(mainCategories);
            await context.SaveChangesAsync();
        }

        // 4. Seed Categories and SubCategories
        if (!await context.Categories.AnyAsync())
        {
            await SeedCategoriesAndSubCategoriesAsync(context);
        }

        // 5. Seed Provider Profile
        if (!await context.ProviderProfiles.AnyAsync())
        {
            await SeedProviderProfileAsync(context, userManager);
        }

        // 6. Seed Services
        if (!await context.Services.AnyAsync())
        {
            await SeedServicesAsync(context);
        }

        // 7. Seed Locations
        if (!await context.Governorates.AnyAsync())
        {
            await SeedLocationsAsync(context);
        }

        // 8. Seed Ads
        if (!await context.Ads.AnyAsync())
        {
            await SeedAdsAsync(context);
        }
    }

    private static async Task SeedAdsAsync(KhadamatDbContext context)
    {
        var now = DateTime.UtcNow;
        
        var ad1 = new Ad("تحديثات جديدة!", "استكشف الواجهة الجديدة كلياً مع نظام التحكم المتطور للمديرين.", now.AddDays(-1), now.AddMonths(2), "Image");
        ad1.UpdateDetails(ad1.Title, ad1.Description, ad1.StartDate, ad1.EndDate, placement: "Slider");
        ad1.SetMainImage("hero-gradient-3");
        ad1.Approve();

        var ad2 = new Ad("خصومات الصيانة", "وفر 30% على صيانة التكييفات اليوم!", now.AddDays(-1), now.AddMonths(1), "Image");
        ad2.UpdateDetails(ad2.Title, ad2.Description, ad2.StartDate, ad2.EndDate, placement: "Slider"); // Note: there's a typo in seed code using ad1.Title for ad2, I'll fix it while I'm here
        ad2.SetMainImage("hero-gradient-1");
        ad2.Approve();

        var ad3 = new Ad("كشف مجاني", "احصل على فحص مجاني للأسنان عند حجز أول موعد.", now.AddDays(-1), now.AddMonths(1), "Image");
        ad3.UpdateDetails(ad3.Title, ad3.Description, ad3.StartDate, ad3.EndDate, placement: "Slider");
        ad3.SetMainImage("hero-gradient-2");
        ad3.Approve();
        
        await context.Ads.AddRangeAsync(ad1, ad2, ad3);
        await context.SaveChangesAsync();
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        string[] roles = { "SuperAdmin", "SystemAdmin", "User" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task SeedUsersAsync(UserManager<ApplicationUser> userManager)
    {
        // Super Admin User
        var superAdminEmail = "superadmin@khadamat.com";
        var superAdminUserName = "SuperAdmin";
        if (await userManager.FindByEmailAsync(superAdminEmail) == null)
        {
            var superAdminUser = new ApplicationUser
            {
                UserName = superAdminUserName,
                Email = superAdminEmail,
                FullName = "Super Admin User",
                Role = UserRole.SuperAdmin,
                IsActive = true,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(superAdminUser, "Admin@123");
            await userManager.AddToRoleAsync(superAdminUser, "SuperAdmin");
        }

        // System Admin User
        var systemAdminEmail = "admin@khadamat.com";
        var systemAdminUserName = "Admin";
        if (await userManager.FindByEmailAsync(systemAdminEmail) == null)
        {
            var systemAdminUser = new ApplicationUser
            {
                UserName = systemAdminUserName,
                Email = systemAdminEmail,
                FullName = "System Admin User",
                Role = UserRole.SystemAdmin,
                IsActive = true,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(systemAdminUser, "Admin@123");
            await userManager.AddToRoleAsync(systemAdminUser, "SystemAdmin");
        }

        // Regular User (can be provider if they have a profile)
        var userEmail = "user@khadamat.com";
        var userUserName = "RegularUser";
        if (await userManager.FindByEmailAsync(userEmail) == null)
        {
            var regularUser = new ApplicationUser
            {
                UserName = userUserName,
                Email = userEmail,
                FullName = "Regular User",
                Role = UserRole.User,
                IsActive = true,
                EmailConfirmed = true
            };
            await userManager.CreateAsync(regularUser, "Admin@123");
            await userManager.AddToRoleAsync(regularUser, "User");
        }
    }

    private static async Task SeedCategoriesAndSubCategoriesAsync(KhadamatDbContext context)
    {
        var mainCats = await context.MainCategories.ToDictionaryAsync(m => m.Name);

        // Health
        if (mainCats.TryGetValue("صحة", out var healthMain))
        {
            var cats = new List<Category>
            {
                new Category { Name = "مراكز طبية", MainCategoryId = healthMain.Id },
                new Category { Name = "عيادات", MainCategoryId = healthMain.Id },
                new Category { Name = "معامل", MainCategoryId = healthMain.Id },
                new Category { Name = "معامل تحاليل", MainCategoryId = healthMain.Id },
                new Category { Name = "تمريض", MainCategoryId = healthMain.Id },
                new Category { Name = "صيدليات", MainCategoryId = healthMain.Id },
                new Category { Name = "مستلزمات طبية", MainCategoryId = healthMain.Id },
                new Category { Name = "مستشفيات", MainCategoryId = healthMain.Id }
            };
            await context.Categories.AddRangeAsync(cats);
            await context.SaveChangesAsync();

            var catDict = cats.ToDictionary(c => c.Name);
            
            // SubCategories for Centers
            await context.SubCategories.AddRangeAsync(
                new SubCategory { Name = "مراكز جراحة", CategoryId = catDict["مراكز طبية"].Id },
                new SubCategory { Name = "مراكز نساء وتوليد", CategoryId = catDict["مراكز طبية"].Id }
            );

            // SubCategories for Clinics
            var clinicId = catDict["عيادات"].Id;
            await context.SubCategories.AddRangeAsync(
                new SubCategory { Name = "اطفال", CategoryId = clinicId },
                new SubCategory { Name = "باطنة", CategoryId = clinicId },
                new SubCategory { Name = "نساء وتوليد", CategoryId = clinicId },
                new SubCategory { Name = "طوارق", CategoryId = clinicId },
                new SubCategory { Name = "مسالك بولية", CategoryId = clinicId },
                new SubCategory { Name = "اسنان", CategoryId = clinicId },
                new SubCategory { Name = "جلدية وتناسلية", CategoryId = clinicId },
                new SubCategory { Name = "غدد", CategoryId = clinicId },
                new SubCategory { Name = "مخ واعصاب", CategoryId = clinicId },
                new SubCategory { Name = "اوعية دموية", CategoryId = clinicId },
                new SubCategory { Name = "جراحة", CategoryId = clinicId },
                new SubCategory { Name = "صدر", CategoryId = clinicId }
            );

            // SubCategories for Labs
            await context.SubCategories.AddRangeAsync(
                new SubCategory { Name = "معامل تحاليل", CategoryId = catDict["معامل"].Id },
                new SubCategory { Name = "معامل اشعة", CategoryId = catDict["معامل"].Id }
            );
        }

        // Education
        if (mainCats.TryGetValue("تعليم", out var eduMain))
        {
            var cats = new List<Category>
            {
                new Category { Name = "حضانات", MainCategoryId = eduMain.Id },
                new Category { Name = "محفظين قرآن", MainCategoryId = eduMain.Id },
                new Category { Name = "كورسات", MainCategoryId = eduMain.Id },
                new Category { Name = "حضانة", MainCategoryId = eduMain.Id },
                new Category { Name = "مدرسين", MainCategoryId = eduMain.Id }
            };
            await context.Categories.AddRangeAsync(cats);
            await context.SaveChangesAsync();
            var catDict = cats.ToDictionary(c => c.Name);

            // SubCategories for Courses
            await context.SubCategories.AddRangeAsync(
                new SubCategory { Name = "علوم شرعية", CategoryId = catDict["كورسات"].Id },
                new SubCategory { Name = "كورسات كمبيوتر", CategoryId = catDict["كورسات"].Id },
                new SubCategory { Name = "كورسات رسم", CategoryId = catDict["كورسات"].Id },
                new SubCategory { Name = "كورسات خياطة واشغال يدوية", CategoryId = catDict["كورسات"].Id },
                new SubCategory { Name = "كورسات طبخ وحلويات", CategoryId = catDict["كورسات"].Id },
                new SubCategory { Name = "كورسات خط", CategoryId = catDict["كورسات"].Id }
            );

            // SubCategories for Teachers
            var teacherId = catDict["مدرسين"].Id;
            await context.SubCategories.AddRangeAsync(
                new SubCategory { Name = "تأسيس", CategoryId = teacherId },
                new SubCategory { Name = "ابتدائى", CategoryId = teacherId },
                new SubCategory { Name = "اعدادى", CategoryId = teacherId },
                new SubCategory { Name = "ثانوى", CategoryId = teacherId },
                new SubCategory { Name = "ابتدائى تجريبى", CategoryId = teacherId },
                new SubCategory { Name = "اعدادى تجريبى", CategoryId = teacherId },
                new SubCategory { Name = "ثانوى تجريبى", CategoryId = teacherId }
            );
        }

        // Stores
        if (mainCats.TryGetValue("متاجر", out var storesMain))
        {
            var cats = new List<Category>
            {
                new Category { Name = "سوبر ماركت", MainCategoryId = storesMain.Id },
                new Category { Name = "ملابس", MainCategoryId = storesMain.Id },
                new Category { Name = "احذية", MainCategoryId = storesMain.Id },
                new Category { Name = "ادوات منزلية", MainCategoryId = storesMain.Id },
                new Category { Name = "منظفات / ورقيات", MainCategoryId = storesMain.Id },
                new Category { Name = "موبيل", MainCategoryId = storesMain.Id },
                new Category { Name = "كمبيوتر وطباعة", MainCategoryId = storesMain.Id },
                new Category { Name = "لعب اطفال", MainCategoryId = storesMain.Id },
                new Category { Name = "دهب وفضيات", MainCategoryId = storesMain.Id },
                new Category { Name = "ادوات مدرسة وهدايا", MainCategoryId = storesMain.Id },
                new Category { Name = "نظارات", MainCategoryId = storesMain.Id },
                new Category { Name = "ستائر واقمشة", MainCategoryId = storesMain.Id },
                new Category { Name = "فلاتر", MainCategoryId = storesMain.Id },
                new Category { Name = "تكييف", MainCategoryId = storesMain.Id }
            };
            await context.Categories.AddRangeAsync(cats);
            await context.SaveChangesAsync();
            var catDict = cats.ToDictionary(c => c.Name);

            // SubCategories for Clothes
            await context.SubCategories.AddRangeAsync(
                new SubCategory { Name = "ملابس اطفال", CategoryId = catDict["ملابس"].Id },
                new SubCategory { Name = "ملابس حريمى", CategoryId = catDict["ملابس"].Id },
                new SubCategory { Name = "ملابس رجالى", CategoryId = catDict["ملابس"].Id }
            );
        }

        // Food & Drinks
        if (mainCats.TryGetValue("ماكولات ومشروبات", out var foodMain))
        {
            var cats = new List<Category>
            {
                new Category { Name = "مطاعم ووجبات سريعة", MainCategoryId = foodMain.Id },
                new Category { Name = "كافيهات", MainCategoryId = foodMain.Id },
                new Category { Name = "مشروبات", MainCategoryId = foodMain.Id },
                new Category { Name = "اكل بيتى", MainCategoryId = foodMain.Id },
                new Category { Name = "حلويات", MainCategoryId = foodMain.Id },
                new Category { Name = "تسالى", MainCategoryId = foodMain.Id },
                new Category { Name = "ولائم وعزومات", MainCategoryId = foodMain.Id },
                new Category { Name = "مستلزمات حلويات", MainCategoryId = foodMain.Id },
                new Category { Name = "عطارة", MainCategoryId = foodMain.Id }
            };
            await context.Categories.AddRangeAsync(cats);
            await context.SaveChangesAsync();
            var catDict = cats.ToDictionary(c => c.Name);

            // SubCategories for Restaurants
            var restId = catDict["مطاعم ووجبات سريعة"].Id;
            await context.SubCategories.AddRangeAsync(
                new SubCategory { Name = "مطاعم عائلية", CategoryId = restId },
                new SubCategory { Name = "فاست فود", CategoryId = restId },
                new SubCategory { Name = "شعبى", CategoryId = restId },
                new SubCategory { Name = "اسماك", CategoryId = restId },
                new SubCategory { Name = "بيتزا وفطائر", CategoryId = restId },
                new SubCategory { Name = "كشرى", CategoryId = restId }
            );
        }

        // Offices
        if (mainCats.TryGetValue("مكاتب", out var officeMain))
        {
            await context.Categories.AddRangeAsync(
                new Category { Name = "استديو تصوير", MainCategoryId = officeMain.Id },
                new Category { Name = "محامى", MainCategoryId = officeMain.Id },
                new Category { Name = "ماذون", MainCategoryId = officeMain.Id },
                new Category { Name = "دعاية واعلان", MainCategoryId = officeMain.Id },
                new Category { Name = "هندسة", MainCategoryId = officeMain.Id },
                new Category { Name = "محاسب قانونى", MainCategoryId = officeMain.Id },
                new Category { Name = "مصور", MainCategoryId = officeMain.Id }
            );
        }

        // Craftsmen
        if (mainCats.TryGetValue("حرفيون", out var craftsMain))
        {
            await context.Categories.AddRangeAsync(
                new Category { Name = "كهرباء", MainCategoryId = craftsMain.Id },
                new Category { Name = "سباكة", MainCategoryId = craftsMain.Id },
                new Category { Name = "نقاش", MainCategoryId = craftsMain.Id },
                new Category { Name = "نجارة", MainCategoryId = craftsMain.Id },
                new Category { Name = "حدادة", MainCategoryId = craftsMain.Id },
                new Category { Name = "بناء وهدد", MainCategoryId = craftsMain.Id },
                new Category { Name = "صيانة اجهزة منزلية", MainCategoryId = craftsMain.Id },
                new Category { Name = "خياطة", MainCategoryId = craftsMain.Id },
                new Category { Name = "تكييف", MainCategoryId = craftsMain.Id },
                new Category { Name = "حلاق", MainCategoryId = craftsMain.Id },
                new Category { Name = "دراى كلين", MainCategoryId = craftsMain.Id }
            );
        }

        // Online Shopping
        if (mainCats.TryGetValue("تسوق اون لين", out var onlineMain))
        {
            await context.Categories.AddRangeAsync(
                new Category { Name = "ملابس وادوات تجميل", MainCategoryId = onlineMain.Id },
                new Category { Name = "اكلات وحلويات", MainCategoryId = onlineMain.Id },
                new Category { Name = "اداوت منزلية", MainCategoryId = onlineMain.Id },
                new Category { Name = "دليفرى", MainCategoryId = onlineMain.Id }
            );
        }

        // Transportation
        if (mainCats.TryGetValue("مواصلات", out var transportMain))
        {
            await context.Categories.AddRangeAsync(
                new Category { Name = "سيارة ملاكى", MainCategoryId = transportMain.Id },
                new Category { Name = "سيارة نقل ركاب", MainCategoryId = transportMain.Id },
                new Category { Name = "نص نقل", MainCategoryId = transportMain.Id },
                new Category { Name = "توكتوك", MainCategoryId = transportMain.Id }
            );
        }

        // Auto Maintenance
        if (mainCats.TryGetValue("صيانة سيارات", out var autoMain))
        {
            await context.Categories.AddRangeAsync(
                new Category { Name = "ميكانيكا سيارات", MainCategoryId = autoMain.Id },
                new Category { Name = "كهرباء سيارات", MainCategoryId = autoMain.Id },
                new Category { Name = "عفشة", MainCategoryId = autoMain.Id },
                new Category { Name = "سروجى", MainCategoryId = autoMain.Id },
                new Category { Name = "زيوت وتشحيم", MainCategoryId = autoMain.Id },
                new Category { Name = "مغسلة", MainCategoryId = autoMain.Id },
                new Category { Name = "لحام كاوتش", MainCategoryId = autoMain.Id },
                new Category { Name = "سمكرة ودوكو سيارات", MainCategoryId = autoMain.Id }
            );
        }

        // Government
        if (mainCats.TryGetValue("خدمات حكومية", out var govMain))
        {
            await context.Categories.AddRangeAsync(
                new Category { Name = "كهرباء", MainCategoryId = govMain.Id },
                new Category { Name = "مياة", MainCategoryId = govMain.Id },
                new Category { Name = "غاز", MainCategoryId = govMain.Id },
                new Category { Name = "سجل مدنى", MainCategoryId = govMain.Id },
                new Category { Name = "مجلس المدينة", MainCategoryId = govMain.Id },
                new Category { Name = "الادارة التعليمية", MainCategoryId = govMain.Id },
                new Category { Name = "اسعاف", MainCategoryId = govMain.Id }
            );
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedProviderProfileAsync(KhadamatDbContext context, UserManager<ApplicationUser> userManager)
    {
        var providerUser = await userManager.FindByEmailAsync("user@khadamat.com");
        if (providerUser != null)
        {
            var profile = new ProviderProfile
            {
                UserId = providerUser.Id,
                BusinessName = "أعمال أحمد للسباكة",
                Bio = "خبرة 10 سنوات في مجال السباكة والصيانة المنزلية.",
                Location = "الرياض، حي الملز",
                ContactNumber = "0501234567",
                Verified = true,
                Photo = "https://ui-avatars.com/api/?name=Ahmed+Provider&background=random"
            };
            await context.ProviderProfiles.AddAsync(profile);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedServicesAsync(KhadamatDbContext context)
    {
        var providers = await context.ProviderProfiles.ToListAsync();
        var allSubCategories = await context.SubCategories.ToListAsync();
        var allCategories = await context.Categories.ToListAsync();

        if (!providers.Any()) return;

        var random = new Random();
        var services = new List<Service>();

        string[] locations = { "الإسكندرية", "القاهرة", "الجيزة", "المنصورة", "طنطا", "الزقازيق", "بورسعيد", "السويس" };
        
        for (int i = 1; i <= 20; i++)
        {
            var provider = providers[random.Next(providers.Count)];
            
            // Randomly decide if it's under Category (20% chance) or SubCategory (80% chance)
            bool isCategoryOnly = random.Next(1, 100) <= 20;

            Service service;
            string location = locations[random.Next(locations.Length)];
            decimal price = random.Next(50, 2000);

            if (isCategoryOnly && allCategories.Any())
            {
                var cat = allCategories[random.Next(allCategories.Count)];
                service = new Service(
                    subCategoryId: null,
                    categoryId: cat.Id,
                    cityId: null,
                    name: $"خدمة {cat.Name} المميزة رقم {i}",
                    description: $"وصف تفصيلي لخدمة {cat.Name} المتاحة لجميع العملاء بجودة عالية وأفضل الأسعار.",
                    address: location,
                    providerProfileId: provider.Id, 
                    userCreated: provider.UserId
                );
            }
            else if (allSubCategories.Any())
            {
                var sub = allSubCategories[random.Next(allSubCategories.Count)];
                service = new Service(
                    subCategoryId: sub.Id,
                    categoryId: null,
                    cityId: null,
                    name: $"خدمة {sub.Name} احترافية {i}",
                    description: $"نقدم لكم أفضل خدمات {sub.Name} بخبرة تزيد عن 5 سنوات في {location}. تواصل معنا الآن.",
                    address: location,
                    providerProfileId: provider.Id,
                    userCreated: provider.UserId
                );
            }
            else continue;

            service.UpdateDetails(service.Name, service.Description, service.Address, price: price);
            service.SetImage($"https://picsum.photos/seed/service{i}/600/400");
            service.Approve();
            services.Add(service);
        }

        await context.Services.AddRangeAsync(services);
        await context.SaveChangesAsync();
    }

    private static async Task SeedLocationsAsync(KhadamatDbContext context)
    {
        var governorate = new Governorate
        {
            Governorate_Name_AR = "القاهرة",
            Governorate_Name_EN = "Cairo",
            DisplayOrder = 1,
            Approved = true,
            UserCreated = "system"
        };
        
        await context.Governorates.AddAsync(governorate);
        await context.SaveChangesAsync();

        var city = new City
        {
            GovernorateId = governorate.Id,
            City_Name_AR = "القاهرة",
            City_Name_EN = "Cairo",
            DisplayOrder = 1,
            Approved = true,
            UserCreated = "system"
        };
        
        await context.Cities.AddAsync(city);
        await context.SaveChangesAsync();
    }
}
