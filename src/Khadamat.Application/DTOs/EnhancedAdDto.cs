namespace Khadamat.Application.DTOs;

/// <summary>
/// نظام إدارة الإعلانات الشامل
/// Comprehensive Ads Management System
/// </summary>
public class EnhancedAdDto
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    
    // نوع الإعلان: صورة، نص، فيديو، مختلط
    // Ad Type: Image, Text, Video, Mixed
    public string AdType { get; set; } = "Image"; // Image, Text, Video, Mixed
    
    // المحتوى - Content
    public string? ImageUrl { get; set; }
    public string? VideoUrl { get; set; } // YouTube URL
    public string? TextContent { get; set; }
    
    // الاستهداف المتطور - Advanced Targeting
    public string? TargetUrl { get; set; }
    public string? TargetCategories { get; set; } // Comma-separated category IDs
    public string? TargetGovernorates { get; set; } // Comma-separated IDs
    public string? TargetCities { get; set; } // Comma-separated IDs
    public string? TargetServices { get; set; } // Comma-separated IDs
    public string? TargetUserGender { get; set; } // Male, Female, All
    public string? TargetDays { get; set; } // Mon,Tue,Wed...
    public string? TargetMonths { get; set; } // Jan,Feb,Mar...
    public TimeOnly? TargetTimeStart { get; set; } // HH:mm
    public TimeOnly? TargetTimeEnd { get; set; } // HH:mm
    public string? TargetKeywords { get; set; } // Comma-separated keywords
    
    // مكان العرض - Placement
    public string Placement { get; set; } = "Slider"; // Slider, Sidebar, Header, Footer, InContent
    public int DisplayOrder { get; set; }
    
    // الجدولة - Scheduling
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    // الحالة - Status
    public bool IsActive { get; set; } = true;
    public int ViewCount { get; set; }
    public int ClickCount { get; set; }
    
    // البيانات الوصفية - Metadata
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
}

/// <summary>
/// إحصائيات الإعلان
/// Ad Statistics
/// </summary>
public class AdStatisticsDto
{
    public int TotalAds { get; set; }
    public int ActiveAds { get; set; }
    public int TotalViews { get; set; }
    public int TotalClicks { get; set; }
    public double ClickThroughRate { get; set; }
}
