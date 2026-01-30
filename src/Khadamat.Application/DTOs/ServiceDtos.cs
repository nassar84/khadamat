using System;
using System.Collections.Generic;

namespace Khadamat.Application.DTOs;

public class ServiceDto
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public int? SubCategoryId { get; set; }
    public int? CategoryId { get; set; }
    public int? CityId { get; set; }
    
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal? Price { get; set; }
    public string Location { get; set; } = string.Empty;
    
    // Contact Information
    public string? Phone1 { get; set; }
    public string? Phone2 { get; set; }
    public string? WhatsApp { get; set; }
    public string? Facebook { get; set; }
    public string? Telegram { get; set; }
    
    // Working Hours
    public string? WorkDays { get; set; }
    public string? WorkHours { get; set; }
    
    // Media
    public List<string> Images { get; set; } = new List<string>();
    public string? YouTubeUrl { get; set; }
    
    // Status & Metadata
    public DateTime CreatedAt { get; set; }
    public bool IsApproved { get; set; }
    public bool IsActive { get; set; }
    public int ViewsCount { get; set; }
    public string? Notes { get; set; }
    

    // Flattened Properties
    public string ProviderName { get; set; } = string.Empty;
    public string? ProviderPhoto { get; set; }
    public string SubCategoryName { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public int MainCategoryId { get; set; }
    public string MainCategoryName { get; set; } = string.Empty;
    
    // Location Properties
    public string? CityName { get; set; }
    public string? CityNameEn { get; set; }
    public int? GovernorateId { get; set; }
    public string? GovernorateName { get; set; }
    public string? GovernorateNameEn { get; set; }

    // Rich Details (Added)
    public double Rating { get; set; }
    public int RatersCount { get; set; }
    public bool IsFavorite { get; set; }
    public List<ReviewDto> Reviews { get; set; } = new();
    public List<PostDto> Posts { get; set; } = new();
}

public class ServiceCreateDto
{
    public int SubCategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal? Price { get; set; }
    public string Location { get; set; } = string.Empty;
    public List<string> Images { get; set; } = new List<string>();
    public string? YouTubeUrl { get; set; }
}

public class ReviewDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? UserAvatar { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class PostDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public int LikesCount { get; set; }
    public int? CommentsCount { get; set; }
    public List<CommentDto> Comments { get; set; } = new();
}

public class CommentDto
{
    public int Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
