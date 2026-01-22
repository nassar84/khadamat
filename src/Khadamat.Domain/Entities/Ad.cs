using System;
using System.Collections.Generic;

namespace Khadamat.Domain.Entities;

public class Ad : BaseEntity
{
    // Foreign Keys
    public int? ActivityID { get; private set; }
    public int? CategoryID { get; private set; }
    public int? SubCategoryID { get; private set; }
    
    // Basic Information
    public string Title { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string AdType { get; private set; } = "Image"; // Image, Text, Video, Mixed
    
    // Content
    public string? ImagePath { get; private set; }
    public string? VideoUrl { get; private set; }
    public string? TextContent { get; private set; } // For text-only or mixed ads
    
    // Date Range
    public DateTime StartDate { get; private set; }
    public DateTime EndDate { get; private set; }
    
    // Status & Metadata
    public int DisplayOrder { get; private set; }
    public bool Approved { get; private set; }
    public string? Notes { get; private set; }
    public string? UserCreated { get; private set; }
    
    // Redirect & Placement
    public string? RedirectUrl { get; private set; }
    public string? Placement { get; private set; } // Slider, Sidebar, Header, Footer, InContent
    
    // Analytics
    public int Views { get; private set; }
    public int Clicks { get; private set; }
    
    // Location & Targeting
    public string? City { get; private set; }
    public string? Governorate { get; private set; }
    public string? TargetGovernorates { get; private set; }
    public string? TargetCities { get; private set; }
    public string? TargetServices { get; private set; }
    public string? TargetUserGender { get; private set; }
    public string? TargetDays { get; private set; }
    public string? TargetMonths { get; private set; }
    public TimeOnly? TargetTimeStart { get; private set; }
    public TimeOnly? TargetTimeEnd { get; private set; }
    public string? TargetKeywords { get; private set; } // Comma separated
    
    // Display Options
    public bool ShowImage { get; private set; }
    public bool ShowText { get; private set; }
    
    // Navigation Properties
    public virtual Category? Category { get; private set; }
    public virtual SubCategory? SubCategory { get; private set; }
    public virtual ICollection<AdImage> AdImages { get; private set; } = new List<AdImage>();
    
    // Constructor for EF Core
    protected Ad() { }

    public Ad(
        string title, 
        string description,
        DateTime startDate,
        DateTime endDate,
        string adType = "Image",
        int? activityId = null,
        int? categoryId = null,
        int? subCategoryId = null,
        string? userCreated = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Ad title is required.");
            
        if (endDate <= startDate)
            throw new ArgumentException("End date must be after start date.");

        Title = title;
        Description = description;
        StartDate = startDate;
        EndDate = endDate;
        AdType = adType;
        ActivityID = activityId;
        CategoryID = categoryId;
        SubCategoryID = subCategoryId;
        UserCreated = userCreated;
        
        // Defaults
        Approved = false;
        DisplayOrder = 0;
        Views = 0;
        Clicks = 0;
        ShowImage = true;
        ShowText = true;
    }

    public void UpdateDetails(
        string title, 
        string description,
        DateTime startDate,
        DateTime endDate,
        string? redirectUrl = null,
        string? placement = null,
        string? city = null,
        string? governorate = null,
        string? videoUrl = null,
        string? textContent = null,
        string? targetKeywords = null,
        string? adType = null,
        string? targetGovernorates = null,
        string? targetCities = null,
        string? targetServices = null,
        string? targetUserGender = null,
        string? targetDays = null,
        string? targetMonths = null,
        TimeOnly? targetTimeStart = null,
        TimeOnly? targetTimeEnd = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Ad title is required.");
            
        if (endDate <= startDate)
            throw new ArgumentException("End date must be after start date.");

        Title = title;
        Description = description;
        StartDate = startDate;
        EndDate = endDate;
        RedirectUrl = redirectUrl;
        Placement = placement;
        City = city;
        Governorate = governorate;
        VideoUrl = videoUrl;
        TextContent = textContent;
        TargetKeywords = targetKeywords;
        TargetGovernorates = targetGovernorates;
        TargetCities = targetCities;
        TargetServices = targetServices;
        TargetUserGender = targetUserGender;
        TargetDays = targetDays;
        TargetMonths = targetMonths;
        TargetTimeStart = targetTimeStart;
        TargetTimeEnd = targetTimeEnd;
        
        if (!string.IsNullOrEmpty(adType))
            AdType = adType;
            
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetMainImage(string imagePath)
    {
        ImagePath = imagePath;
    }

    public void AddImage(string imagePath, int displayOrder = 0)
    {
        var adImage = new AdImage(Id, imagePath, displayOrder);
        AdImages.Add(adImage);
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
        Views++;
    }

    public void IncrementClicks()
    {
        Clicks++;
    }

    public void SetDisplayOptions(bool showImage, bool showText)
    {
        ShowImage = showImage;
        ShowText = showText;
    }

    public bool IsActive()
    {
        var now = DateTime.UtcNow;
        return Approved && !IsDeleted && now >= StartDate && now <= EndDate;
    }
}
