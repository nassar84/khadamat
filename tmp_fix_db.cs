using Microsoft.EntityFrameworkCore;
using Khadamat.Infrastructure.Persistence;
using Khadamat.Domain.Entities;
using System;
using System.Linq;

var optionsBuilder = new DbContextOptionsBuilder<KhadamatDbContext>();
optionsBuilder.UseSqlServer("Server=(localdb)\\mssqllocaldb;Database=KhadamatDb;Trusted_Connection=True;MultipleActiveResultSets=true");

using var context = new KhadamatDbContext(optionsBuilder.Options);
var settings = context.AppSettings.FirstOrDefault();
if (settings != null)
{
    Console.WriteLine($"AppName: {settings.ApplicationName}");
    if (settings.ApplicationName != "خدماوي")
    {
        settings.ApplicationName = "خدماوي";
        context.SaveChanges();
        Console.WriteLine("Updated AppName to خدماوي");
    }
}
else
{
    Console.WriteLine("No settings found");
}
