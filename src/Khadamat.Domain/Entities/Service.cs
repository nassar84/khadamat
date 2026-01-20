using System;
using System.Collections.Generic;
using Khadamat.Domain.Exceptions;

namespace Khadamat.Domain.Entities;

public class Service : BaseEntity
{
    // Foreign Keys
    public int? SubCategoryId { get; private set; }
    public int? CategoryId { get; private set; }
    public int? CityId { get; private set; }
    public string? UserId { get; private set; }
    
    // Basic Information
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;
    
    // Contact Information
    public string? Phone1 { get; private set; }
    public string? Phone2 { get; private set; }
    public string? WhatsApp { get; private set; }
    public string? Facebook { get; private set; }
    public string? Telegram { get; private set; }
    
    // Working Hours
    public string? Work_Days { get; private set; }
    public string? Work_Houers { get; private set; }
    
    // Media
    public string? ImageUrl { get; private set; }
    
    // Pricing
    public decimal? Price { get; private set; }
    
    // Status & Metadata
    public int DisplayOrder { get; private set; }
    public bool Approved { get; private set; }
    public string? Notes { get; private set; }
    public string? UserCreated { get; private set; }
    public int ViewsCount { get; private set; }
    
    // Navigation Properties
    public virtual Category? Category { get; private set; }
    public virtual SubCategory? SubCategory { get; private set; }
    public virtual City? City { get; private set; }
    public virtual ICollection<Rating> Ratings { get; private set; } = new List<Rating>();
    public virtual ICollection<Like> Likes { get; private set; } = new List<Like>();

    // Constructor for EF Core
    protected Service() { }

    public Service(
        int? subCategoryId, 
        int? categoryId, 
        int? cityId, 
        string name, 
        string description, 
        string address,
        string? userId = null)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length < 3)
            throw new BusinessRuleException("Service name must be at least 3 characters long.");
        
        if (!categoryId.HasValue && !subCategoryId.HasValue)
            throw new BusinessRuleException("Service must be linked to either a Category or a SubCategory.");
            
        SubCategoryId = subCategoryId;
        CategoryId = categoryId;
        CityId = cityId;
        Name = name;
        Description = description;
        Address = address;
        UserId = userId;
        Approved = false; // Requires admin approval
        DisplayOrder = 0;
        ViewsCount = 0;
        UserCreated = userId;
    }

    public void UpdateDetails(
        string name, 
        string description, 
        string address,
        decimal? price = null,
        string? phone1 = null,
        string? phone2 = null,
        string? whatsApp = null,
        string? facebook = null,
        string? telegram = null,
        string? workDays = null,
        string? workHours = null)
    {
        if (!string.IsNullOrWhiteSpace(name) && name.Length < 3)
            throw new BusinessRuleException("Service name must be at least 3 characters long.");

        Name = name;
        Description = description;
        Address = address;
        Price = price;
        Phone1 = phone1;
        Phone2 = phone2;
        WhatsApp = whatsApp;
        Facebook = facebook;
        Telegram = telegram;
        Work_Days = workDays;
        Work_Houers = workHours;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetImage(string imageUrl)
    {
        ImageUrl = imageUrl;
    }

    public void Approve(string? notes = null)
    {
        Approved = true;
        Notes = notes;
    }

    public void Reject(string? notes = null)
    {
        Approved = false;
        Notes = notes;
    }

    public void SetDisplayOrder(int order)
    {
        DisplayOrder = order;
    }

    public void IncrementViews()
    {
        ViewsCount++;
    }

    public void SetCategory(int? categoryId, int? subCategoryId)
    {
        if (!categoryId.HasValue && !subCategoryId.HasValue)
            throw new BusinessRuleException("Service must be linked to either a Category or a SubCategory.");
            
        CategoryId = categoryId;
        SubCategoryId = subCategoryId;
    }

    public void UpdateLocation(int? cityId)
    {
        CityId = cityId;
    }
}
