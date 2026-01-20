using Microsoft.AspNetCore.Identity;
using Khadamat.Domain.Enums;
using Khadamat.Domain.Entities;
using System;

namespace Khadamat.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsProvider { get; set; }
    public bool IsVerified { get; set; }
    public string? ProfileImageUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int? CityId { get; set; }
    public virtual City? City { get; set; }

    // Unified Account: One-to-One link to ProviderProfile
    public virtual ProviderProfile? ProviderProfile { get; set; }

    // Refresh Token Support
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
}
