using Microsoft.Extensions.DependencyInjection;
using Khadamat.Infrastructure.Persistence;
using Khadamat.Domain.Entities;
using Khadamat.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace Khadamat.Infrastructure.Persistence;

public class ServiceImporter
{
    public static async Task Run(IServiceProvider services, string csvPath)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<KhadamatDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        if (!File.Exists(csvPath))
        {
            Console.WriteLine($"Error: File not found at {csvPath}");
            Console.WriteLine($"Current Directory: {Directory.GetCurrentDirectory()}");
            return;
        }

        var lines = File.ReadAllLines(csvPath);
        if (lines.Length <= 1) return; // Only header

        Console.WriteLine($"Starting import of {lines.Length - 1} records...");

        for (int i = 1; i < lines.Length; i++)
        {
            var line = lines[i];
            var parts = line.Split(',');
            if (parts.Length < 9) continue;

            string subCatName = parts[0].Trim();
            string cityName = parts[1].Trim();
            string name = parts[2].Trim();
            string desc = parts[3].Trim();
            string addr = parts[4].Trim();
            decimal price = decimal.TryParse(parts[5], out var p) ? p : 0;
            string phone = parts[6].Trim();
            string whatsapp = parts[7].Trim();
            string email = parts[8].Trim();

            try
            {
                // 1. Resolve Provider
                var user = await userManager.FindByEmailAsync(email);
                if (user == null) { Console.WriteLine($"Skipping {name}: User {email} not found."); continue; }
                
                var provider = await context.ProviderProfiles.FirstOrDefaultAsync(p => p.UserId == user.Id);
                if (provider == null) { Console.WriteLine($"Skipping {name}: User {email} is not a provider."); continue; }

                // 2. Resolve SubCategory
                var subCat = await context.SubCategories.FirstOrDefaultAsync(s => s.Name == subCatName);
                if (subCat == null) { Console.WriteLine($"Skipping {name}: SubCategory {subCatName} not found."); continue; }

                // 3. Resolve City
                var city = await context.Cities.FirstOrDefaultAsync(c => c.City_Name_AR == cityName);
                
                // 4. Create Service
                var service = new Service(
                    subCategoryId: subCat.Id,
                    categoryId: null,
                    cityId: city?.Id,
                    name: name,
                    description: desc,
                    address: addr,
                    providerProfileId: provider.Id,
                    userCreated: user.Id
                );

                service.UpdateDetails(name, desc, addr, price, phone1: phone, whatsApp: whatsapp);
                service.Approve(); // Auto-approve for bulk import
                
                context.Services.Add(service);
                Console.WriteLine($"[SUCCESS] Loaded: {name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Failed to load {name}: {ex.Message}");
            }
        }

        await context.SaveChangesAsync();
        Console.WriteLine("Import completed successfully.");
    }
}
