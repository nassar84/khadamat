using MediatR;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Khadamat.Application.Features.Services.Commands;

public record CreateServiceCommand : IRequest<int>
{
    [Required(ErrorMessage = "الرجاء اختيار القسم")]
    public int? CategoryId { get; set; }
    
    public int? SubCategoryId { get; set; }
    
    [Required(ErrorMessage = "الرجاء اختيار المدينة")]
    [Range(1, int.MaxValue, ErrorMessage = "الرجاء اختيار المدينة")]
    public int? CityId { get; set; }
    
    [Required(ErrorMessage = "اسم النشاط مطلوب")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "الاسم يجب أن يكون بين 3 و 100 حرف")]
    public string Title { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "وصف النشاط مطلوب")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "الوصف يجب أن يكون بين 10 و 2000 حرف")]
    public string Description { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "العنوان مطلوب")]
    public string Address { get; set; } = string.Empty;
    
    public decimal? Price { get; set; }
    public string Location { get; set; } = string.Empty;
    public List<string> Images { get; set; } = new List<string>();
    
    // Contact Information
    public string? Phone1 { get; set; }
    public string? Phone2 { get; set; }
    public string? WhatsApp { get; set; }
    public string? Facebook { get; set; }
    public string? Telegram { get; set; }
    
    // Working Hours
    public string? WorkDays { get; set; }
    public string? WorkHours { get; set; }
    
    public string? YouTubeUrl { get; set; }
    
    // Set by Controller from Claims
    public string UserId { get; set; } = string.Empty;
}
