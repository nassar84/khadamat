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

            if (!await context.Categories.AnyAsync())
            {
                await SeedCategoriesAndSubCategoriesAsync(context);
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
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SEED ERROR: {ex.Message}");
        }
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
        if (await userManager.FindByEmailAsync("admin@khadamat.com") == null)
        {
            var admin = new ApplicationUser { UserName = "Admin", Email = "admin@khadamat.com", FullName = "System Admin", Role = UserRole.SystemAdmin, EmailConfirmed = true };
            await userManager.CreateAsync(admin, "Admin@123");
            await userManager.AddToRoleAsync(admin, "SystemAdmin");
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
}
