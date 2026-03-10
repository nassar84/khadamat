namespace Khadamat.Application.DTOs;

public class AdPackageDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int DurationDays { get; set; }
    public string Tier { get; set; } = "Basic";
    public int MaxAds { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsSponsored { get; set; }
    public bool IsBanner { get; set; }
    public int PriorityBoost { get; set; }
    public bool IsActive { get; set; }
}

public class CreateAdPackageRequest
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int DurationDays { get; set; }
    public string Tier { get; set; } = "Basic";
    public int MaxAds { get; set; }
    public bool IsFeatured { get; set; }
    public bool IsSponsored { get; set; }
    public bool IsBanner { get; set; }
    public int PriorityBoost { get; set; }
}
