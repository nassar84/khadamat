namespace Khadamat.Application.DTOs;

public class AdminStatsDto
{
    public int TotalUsers { get; set; }
    public int TotalProviders { get; set; }
    public int TotalServices { get; set; }
    public int TotalOrders { get; set; }
}

public class UserDto
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public int? CityId { get; set; }
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public bool IsVerified { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? Gender { get; set; } // "Male", "Female", or null
    public DateTime CreatedAt { get; set; }

    // Enhanced Fields
    public string? Bio { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? InstagramUrl { get; set; }
    public string? TwitterUrl { get; set; }
    public string? FacebookUrl { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? TikTokUrl { get; set; }
}

public class PendingProviderDto
{
    public int Id { get; set; }
    public string UserId { get; set; }
    public string Name { get; set; } // User FullName
    public string Email { get; set; } // User Email
    public string BusinessName { get; set; }
    public string Phone { get; set; } // Provider Contact Number
    public string City { get; set; }
    public string Expertise { get; set; } // Bio or Category Description
    public DateTime JoinedAt { get; set; }
    public string IdCardImage { get; set; }
    public string CertificateImage { get; set; }
}

public class ApplyProviderDto
{
    public string BusinessName { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public int CityId { get; set; }
    public string ContactNumber { get; set; } = string.Empty;
    public string? WebsiteUrl { get; set; }
    public string? IdCardImage { get; set; }
    public string? CertificateImage { get; set; }
}

public class AdDto
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string ImageUrl { get; set; } = "";
    public string? TargetUrl { get; set; }
}

public class RecentActivityDto
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public string Time { get; set; } = "";
    public string Icon { get; set; } = "";
}

public class UpdateProviderProfileRequest
{
    public string BusinessName { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string ContactNumber { get; set; } = string.Empty;
    public string? WebsiteUrl { get; set; }
    public string? InstagramUrl { get; set; }
    public string? TwitterUrl { get; set; }
    public string? Photo { get; set; }
}

public class ChangePasswordDto
{
    public string UserId { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
