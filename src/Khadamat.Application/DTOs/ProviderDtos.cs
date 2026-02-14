using System;
using System.Collections.Generic;

namespace Khadamat.Application.DTOs;

public class ProviderProfileDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string BusinessName { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string Photo { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public string? WebsiteUrl { get; set; }
    public string? InstagramUrl { get; set; }
    public string? TwitterUrl { get; set; }
    public bool Verified { get; set; }
    public string? CityName { get; set; }
    public int? CityId { get; set; }
}
