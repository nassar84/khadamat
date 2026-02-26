using System;
using System.Collections.Generic;
using Khadamat.Domain.Exceptions;

namespace Khadamat.Domain.Entities;

public class MarketplaceItem : BaseEntity
{
    // Foreign Keys
    public int CategoryId { get; private set; }
    public int? SubCategoryId { get; private set; }
    public int? CityId { get; private set; }
    public string SellerId { get; private set; } = string.Empty; // Identity User Id

    // Basic Information
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public string Currency { get; private set; } = "EGP";
    
    // Properties
    public string Condition { get; private set; } = "Used"; // New, Used
    public string ItemStatus { get; private set; } = "Available"; // Available, Sold, Cancelled
    public string ContactPhone { get; private set; } = string.Empty;
    
    // Status & Metadata
    public bool Approved { get; private set; }
    public int ViewsCount { get; private set; }
    public string? AdminNotes { get; private set; }
    
    // Monetization
    public bool IsFeatured { get; private set; }
    public bool IsPromoted { get; private set; }
    public DateTime? FeaturedUntil { get; private set; }
    public DateTime? PromotedUntil { get; private set; }

    // Navigation Properties
    public virtual Category Category { get; private set; } = null!;
    public virtual SubCategory? SubCategory { get; private set; }
    public virtual City? City { get; private set; }
    public virtual ICollection<MarketplaceImage> Images { get; private set; } = new List<MarketplaceImage>();

    // Constructor for EF Core
    protected MarketplaceItem() { }

    public MarketplaceItem(
        string title, 
        string description, 
        decimal price, 
        string sellerId, 
        int categoryId, 
        string contactPhone,
        int? subCategoryId = null,
        int? cityId = null,
        string condition = "Used")
    {
        if (string.IsNullOrWhiteSpace(title) || title.Length < 3)
            throw new BusinessRuleException("Item title must be at least 3 characters long.");
            
        if (price < 0)
            throw new BusinessRuleException("Price cannot be negative.");

        Title = title;
        Description = description;
        Price = price;
        SellerId = sellerId;
        CategoryId = categoryId;
        SubCategoryId = subCategoryId;
        CityId = cityId;
        ContactPhone = contactPhone;
        Condition = condition;
        
        Approved = false; // Requires moderation
        ItemStatus = "Available";
        ViewsCount = 0;
    }

    public void UpdateItem(
        string title, 
        string description, 
        decimal price, 
        string contactPhone, 
        string condition,
        int categoryId,
        int? subCategoryId = null,
        int? cityId = null)
    {
        if (string.IsNullOrWhiteSpace(title) || title.Length < 3)
            throw new BusinessRuleException("Item title must be at least 3 characters long.");

        Title = title;
        Description = description;
        Price = price;
        ContactPhone = contactPhone;
        Condition = condition;
        CategoryId = categoryId;
        SubCategoryId = subCategoryId;
        CityId = cityId;
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsSold()
    {
        ItemStatus = "Sold";
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsAvailable()
    {
        ItemStatus = "Available";
        UpdatedAt = DateTime.UtcNow;
    }

    public void Approve(string? notes = null)
    {
        Approved = true;
        AdminNotes = notes;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Reject(string? notes = null)
    {
        Approved = false;
        AdminNotes = notes;
        UpdatedAt = DateTime.UtcNow;
    }

    public void IncrementViews()
    {
        ViewsCount++;
    }

    public void SetFeatured(int days)
    {
        IsFeatured = true;
        FeaturedUntil = DateTime.UtcNow.AddDays(days);
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetPromoted(int days)
    {
        IsPromoted = true;
        PromotedUntil = DateTime.UtcNow.AddDays(days);
        UpdatedAt = DateTime.UtcNow;
    }
}
