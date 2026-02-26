using System;
using System.Collections.Generic;

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
    public List<MarketplaceImageDto> Images { get; set; } = new();
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
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int CategoryId { get; set; }
    public int? SubCategoryId { get; set; }
    public int? CityId { get; set; }
    public string ContactPhone { get; set; } = string.Empty;
    public string Condition { get; set; } = "Used"; // New, Used
    public List<string> Images { get; set; } = new(); // Base64 strings
}
