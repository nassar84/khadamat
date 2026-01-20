using MediatR;
using System.Collections.Generic;

namespace Khadamat.Application.Features.Services.Commands;

public record UpdateServiceCommand : IRequest<bool>
{
    public int Id { get; set; }
    public int? CategoryId { get; set; }
    public int? SubCategoryId { get; set; }
    public int? CityId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
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
