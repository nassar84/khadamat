using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Khadamat.Domain.Entities;
using Khadamat.Infrastructure.Identity;

namespace Khadamat.Infrastructure.Persistence;

public class KhadamatDbContext : IdentityDbContext<ApplicationUser>
{
    public KhadamatDbContext(DbContextOptions<KhadamatDbContext> options) : base(options)
    {
    }

    public DbSet<ProviderProfile> ProviderProfiles { get; set; }
    public DbSet<MainCategory> MainCategories { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<SubCategory> SubCategories { get; set; }
    public DbSet<Service> Services { get; set; }
    public DbSet<Rating> Ratings { get; set; }
    public DbSet<Like> Likes { get; set; }
    public DbSet<Post> Posts { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<ProviderSubscription> ProviderSubscriptions { get; set; }
    public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
    public DbSet<Ad> Ads { get; set; }
    public DbSet<AdImage> AdImages { get; set; }
    public DbSet<Favorite> Favorites { get; set; }
    public DbSet<Message> Messages { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<Governorate> Governorates { get; set; }
    public DbSet<City> Cities { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Global Filter for Soft Delete
        builder.Entity<Service>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<ProviderProfile>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Post>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<MainCategory>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Category>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<SubCategory>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Governorate>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<City>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Ad>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<AdImage>().HasQueryFilter(e => !e.IsDeleted);

        // Configure relationships and constraints
        builder.Entity<Governorate>().HasMany(g => g.Cities).WithOne(c => c.Governorate).HasForeignKey(c => c.GovernorateId);
        builder.Entity<City>().HasMany(c => c.Services).WithOne(s => s.City).HasForeignKey(s => s.CityId);
        builder.Entity<City>().HasMany(c => c.ProviderProfiles).WithOne(p => p.City).HasForeignKey(p => p.CityId);
        
        builder.Entity<MainCategory>().HasMany(m => m.Categories).WithOne(c => c.MainCategory).HasForeignKey(c => c.MainCategoryId);
        builder.Entity<Category>().HasMany(c => c.SubCategories).WithOne(s => s.Category).HasForeignKey(s => s.CategoryId);
        builder.Entity<Category>().HasMany(c => c.Services).WithOne(s => s.Category).HasForeignKey(s => s.CategoryId);
        builder.Entity<SubCategory>().HasMany(s => s.Services).WithOne(se => se.SubCategory).HasForeignKey(se => se.SubCategoryId);

        // Unified Account: ProviderProfile <-> ApplicationUser
        builder.Entity<ApplicationUser>()
            .HasOne(u => u.ProviderProfile)
            .WithOne() // Uni-directional from User -> Profile logic
            .HasForeignKey<ProviderProfile>(p => p.UserId)
            .IsRequired(false);

        // Provider <-> Services
        builder.Entity<ProviderProfile>()
            .HasMany(p => p.Services)
            .WithOne(s => s.ProviderProfile)
            .HasForeignKey(s => s.ProviderProfileId);
            
        builder.Entity<ProviderProfile>().HasMany(p => p.Posts).WithOne(po => po.Provider).HasForeignKey(po => po.ProviderId);
        
        // Ad Relationships
        builder.Entity<Ad>().HasMany(a => a.AdImages).WithOne(ai => ai.Ad).HasForeignKey(ai => ai.AdId);
        builder.Entity<Category>().HasMany<Ad>().WithOne(a => a.Category).HasForeignKey(a => a.CategoryID);
        builder.Entity<SubCategory>().HasMany<Ad>().WithOne(a => a.SubCategory).HasForeignKey(a => a.SubCategoryID);
        
        // Subscription Relationships
        builder.Entity<ProviderSubscription>()
            .HasOne(ps => ps.Provider)
            .WithOne(p => p.Subscription)
            .HasForeignKey<ProviderSubscription>(ps => ps.ProviderId);
            
        builder.Entity<ProviderSubscription>()
            .HasOne(ps => ps.Plan)
            .WithMany()
            .HasForeignKey(ps => ps.PlanId);

        // Configure decimal precision for Price fields
        builder.Entity<Service>()
            .Property(s => s.Price)
            .HasPrecision(18, 2);
            
        builder.Entity<SubscriptionPlan>()
            .Property(sp => sp.Price)
            .HasPrecision(18, 2);
    }
}
