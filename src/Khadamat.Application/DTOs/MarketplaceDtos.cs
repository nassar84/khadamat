using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Khadamat.Application.DTOs;

public class MarketplaceItemDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "EGP";
    public string Condition { get; set; } = string.Empty;
    public string ItemStatus { get; set; } = string.Empty;
    public string ContactPhone { get; set; } = string.Empty;
    public string SellerId { get; set; } = string.Empty;
    public string SellerName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int? SubCategoryId { get; set; }
    public string? SubCategoryName { get; set; }
    public int? CityId { get; set; }
    public string? CityName { get; set; }
    public int ViewsCount { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsPromoted { get; set; }
    public DateTime? FeaturedUntil { get; set; }
    public DateTime? PromotedUntil { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ListedAt { get; set; }           // تاريخ عرض السلعة فعلياً
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public DateTime? SoldDate { get; set; }
    public bool Approved { get; set; }               // هل تمت الموافقة عليه
    public List<MarketplaceImageDto> Images { get; set; } = new();

    // Computed
    public bool IsExpired => EndDate.HasValue && EndDate.Value < DateTime.UtcNow;
    public int? DaysRemaining => EndDate.HasValue
        ? (int?)Math.Max(0, (EndDate.Value - DateTime.UtcNow).TotalDays)
        : null;
}

public class MarketplaceImageDto
{
    public int Id { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public bool IsMain { get; set; }
    public int DisplayOrder { get; set; }
}


public class CreateMarketplaceItemRequest
{
    [Required(ErrorMessage = "يرجى إدخال عنوان الإعلان")]
    [StringLength(100, MinimumLength = 5, ErrorMessage = "العنوان يجب أن يكون بين 5 و 100 حرف")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "يرجى إدخال وصف للسلعة")]
    public string Description { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = "يرجى إدخال سعر صحيح")]
    public decimal Price { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "يرجى اختيار القسم الرئيسي")]
    public int CategoryId { get; set; }

    public int? SubCategoryId { get; set; }

    [Required(ErrorMessage = "يرجى اختيار المدينة")]
    public int? CityId { get; set; }

    [Required(ErrorMessage = "يرجى إدخال رقم التواصل")]
    [Phone(ErrorMessage = "رقم هاتف غير صحيح")]
    public string ContactPhone { get; set; } = string.Empty;

    public string Condition { get; set; } = "Used"; // New, Used
    public List<string> Images { get; set; } = new(); // Base64 strings
}
public class MarketplaceCategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public string? ImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public List<MarketplaceSubCategoryDto> SubCategories { get; set; } = new();
}

public class MarketplaceSubCategoryDto
{
    public int Id { get; set; }
    public int CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}

public class ChangeMarketplaceItemStatusRequest
{
    [Required]
    public string Status { get; set; } = string.Empty; // Available, Sold, Cancelled, Locked
}

public class MarketplaceSettingsDto
{
    public int DefaultListingDays { get; set; } = 30;
    public int MaxListingsPerUser { get; set; } = 10;
    public bool RequireApproval { get; set; } = true;
    public bool AutoExpire { get; set; } = true;
}
