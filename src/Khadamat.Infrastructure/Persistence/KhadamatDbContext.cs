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
    public DbSet<ServiceEditRequest> ServiceEditRequests { get; set; }
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
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }
    public DbSet<Governorate> Governorates { get; set; }
    public DbSet<City> Cities { get; set; }
    public DbSet<AppSettings> AppSettings { get; set; }
    public DbSet<ServiceRequest> ServiceRequests { get; set; }
    public DbSet<MarketplaceItem> MarketplaceItems { get; set; }
    public DbSet<MarketplaceImage> MarketplaceImages { get; set; }
    public DbSet<MarketplaceItemView> MarketplaceItemViews { get; set; }
    public DbSet<MarketplaceCategory> MarketplaceCategories { get; set; }
    public DbSet<MarketplaceSubCategory> MarketplaceSubCategories { get; set; }
    public DbSet<Payment> Payments { get; set; }

    // ── Advertisement & Growth System ─────────────────────────────────────
    public DbSet<AdPackage> AdPackages { get; set; }
    public DbSet<AdCampaign> AdCampaigns { get; set; }
    public DbSet<Advertisement> Advertisements { get; set; }
    public DbSet<AdImpression> AdImpressions { get; set; }
    public DbSet<AdClick> AdClicks { get; set; }
    public DbSet<AdStatistic> AdStatistics { get; set; }
    public DbSet<AdExtension> AdExtensions { get; set; }
    public DbSet<PromotionalOffer> PromotionalOffers { get; set; }
    public DbSet<TrialAdvertisement> TrialAdvertisements { get; set; }
    // ── Referral & Points ─────────────────────────────────────────────────
    public DbSet<ReferralCode> ReferralCodes { get; set; }
    public DbSet<Referral> Referrals { get; set; }
    public DbSet<ProviderPoints> ProviderPoints { get; set; }
    public DbSet<RewardConversion> RewardConversions { get; set; }
    public DbSet<PointRewardRule> PointRewardRules { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Global Filter for Soft Delete
        builder.Entity<Service>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<ServiceRequest>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<ProviderProfile>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Post>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<MainCategory>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Category>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<SubCategory>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Governorate>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<City>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<Ad>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<AdImage>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<ApplicationUser>().HasQueryFilter(u => !u.IsDeleted);
        builder.Entity<MarketplaceItem>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<MarketplaceImage>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<MarketplaceCategory>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<MarketplaceSubCategory>().HasQueryFilter(e => !e.IsDeleted);

        // Configure relationships and constraints
        builder.Entity<Governorate>().HasMany(g => g.Cities).WithOne(c => c.Governorate).HasForeignKey(c => c.GovernorateId);
        builder.Entity<City>().HasMany(c => c.Services).WithOne(s => s.City).HasForeignKey(s => s.CityId);
        builder.Entity<City>().HasMany(c => c.ProviderProfiles).WithOne(p => p.City).HasForeignKey(p => p.CityId);
        
        builder.Entity<MainCategory>().HasMany(m => m.Categories).WithOne(c => c.MainCategory).HasForeignKey(c => c.MainCategoryId);
        builder.Entity<MainCategory>().Property(c => c.DisplayOrder).HasColumnName("Order");
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
        builder.Entity<Service>().HasMany<Ad>().WithOne(a => a.Service).HasForeignKey(a => a.ServiceID);
        
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
        builder.Entity<Rating>().ToTable("Ratings");

        // ServiceRequest Relationships
        builder.Entity<ServiceRequest>()
            .HasOne(sr => sr.Service)
            .WithMany()
            .HasForeignKey(sr => sr.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ServiceRequest>()
            .HasOne(sr => sr.Provider)
            .WithMany()
            .HasForeignKey(sr => sr.ProviderId)
            .OnDelete(DeleteBehavior.Restrict);

        // Marketplace Configuration
        builder.Entity<MarketplaceCategory>()
            .HasMany(c => c.SubCategories)
            .WithOne(s => s.Category)
            .HasForeignKey(s => s.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<MarketplaceItem>()
            .HasOne(m => m.Category)
            .WithMany(c => c.Items)
            .HasForeignKey(m => m.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<MarketplaceItem>()
            .HasOne(m => m.SubCategory)
            .WithMany(s => s.Items)
            .HasForeignKey(m => m.SubCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<MarketplaceItem>()
            .HasOne(m => m.City)
            .WithMany()
            .HasForeignKey(m => m.CityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<MarketplaceItem>()
            .HasMany(m => m.Images)
            .WithOne(i => i.MarketplaceItem)
            .HasForeignKey(i => i.MarketplaceItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<MarketplaceItem>()
            .HasOne<ApplicationUser>()
            .WithMany(u => u.MarketplaceItems)
            .HasForeignKey(m => m.SellerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<MarketplaceItemView>()
            .HasOne(v => v.MarketplaceItem)
            .WithMany(m => m.ItemViews)
            .HasForeignKey(v => v.MarketplaceItemId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<MarketplaceItem>()
            .Property(m => m.Price)
            .HasPrecision(18, 2);

        builder.Entity<Favorite>()
            .HasOne(f => f.MarketplaceItem)
            .WithMany()
            .HasForeignKey(f => f.MarketplaceItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Payment>()
            .Property(p => p.Amount)
            .HasPrecision(18, 2);

        builder.Entity<Payment>()
            .HasOne(p => p.MarketplaceItem)
            .WithMany()
            .HasForeignKey(p => p.MarketplaceItemId)
            .OnDelete(DeleteBehavior.SetNull);

        // ── Advertisement System ───────────────────────────────────────────
        builder.Entity<AdCampaign>()
            .HasMany(c => c.Advertisements)
            .WithOne(a => a.Campaign)
            .HasForeignKey(a => a.CampaignId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<AdCampaign>()
            .HasOne(c => c.Package)
            .WithMany()
            .HasForeignKey(c => c.PackageId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Advertisement>()
            .HasOne(a => a.Category)
            .WithMany()
            .HasForeignKey(a => a.CategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Advertisement>()
            .HasOne(a => a.SubCategory)
            .WithMany()
            .HasForeignKey(a => a.SubCategoryId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Advertisement>()
            .HasOne(a => a.Service)
            .WithMany()
            .HasForeignKey(a => a.ServiceId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<Advertisement>()
            .HasOne(a => a.City)
            .WithMany()
            .HasForeignKey(a => a.CityId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<AdImpression>()
            .HasOne(i => i.Advertisement)
            .WithMany()
            .HasForeignKey(i => i.AdvertisementId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<AdClick>()
            .HasOne(c => c.Advertisement)
            .WithMany()
            .HasForeignKey(c => c.AdvertisementId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<AdStatistic>()
            .HasOne(s => s.Advertisement)
            .WithMany()
            .HasForeignKey(s => s.AdvertisementId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<AdStatistic>()
            .HasIndex(s => new { s.AdvertisementId, s.Date })
            .IsUnique();

        builder.Entity<AdExtension>()
            .HasOne(e => e.Advertisement)
            .WithMany()
            .HasForeignKey(e => e.AdvertisementId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<AdCampaign>().Property(c => c.Budget).HasPrecision(18, 2);
        builder.Entity<Advertisement>().Property(a => a.Bid).HasPrecision(18, 2);
        builder.Entity<PromotionalOffer>().Property(o => o.DiscountPercentage).HasPrecision(5, 2);

        builder.Entity<Advertisement>().HasQueryFilter(a => !a.IsDeleted);
        builder.Entity<AdCampaign>().HasQueryFilter(c => !c.IsDeleted);

        // ── Referral & Points System ───────────────────────────────────────
        builder.Entity<ReferralCode>()
            .HasMany(r => r.Referrals)
            .WithOne(ref1 => ref1.ReferralCode)
            .HasForeignKey(ref1 => ref1.ReferralCodeId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<ReferralCode>()
            .HasIndex(r => r.Code)
            .IsUnique();

        builder.Entity<ProviderPoints>()
            .HasIndex(p => p.ProviderId)
            .IsUnique();
    }
}
