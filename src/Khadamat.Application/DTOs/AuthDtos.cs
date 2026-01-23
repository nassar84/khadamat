using System.ComponentModel.DataAnnotations;

namespace Khadamat.Application.DTOs;

public class RegisterRequest
{
    [Required(ErrorMessage = "الاسم الكامل مطلوب")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "الاسم يجب أن يكون بين 3 و 100 حرف")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "اسم المستخدم مطلوب")]
    [RegularExpression(@"^[a-zA-Z0-9_]*$", ErrorMessage = "اسم المستخدم يجب أن يحتوي على أحرف وأرقام وجرار سفلي فقط")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
    [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "كلمة المرور مطلوبة")]
    [MinLength(6, ErrorMessage = "كلمة المرور يجب أن لا تقل عن 6 أحرف")]
    public string Password { get; set; } = string.Empty;
    
    [Required]
    public string UserType { get; set; } = "User"; // User, SystemAdmin, SuperAdmin

    [Required(ErrorMessage = "رقم الهاتف مطلوب")]
    [Phone(ErrorMessage = "رقم الهاتف غير صحيح")]
    [RegularExpression(@"^01[0125][0-9]{8}$", ErrorMessage = "رقم الهاتف يجب أن يكون رقم مصري صحيح (11 رقم)")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "الرجاء اختيار المدينة")]
    [Range(1, int.MaxValue, ErrorMessage = "الرجاء اختيار المدينة")]
    public int CityId { get; set; }
}

public class LoginRequest
{
    [Required(ErrorMessage = "اسم المستخدم مطلوب")]
    public string UserName { get; set; } = string.Empty;

    [Required(ErrorMessage = "كلمة المرور مطلوبة")]
    public string Password { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string Id { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public bool IsVerified { get; set; }
    public bool IsProvider { get; set; }
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
    
    // Extended Profile Info
    public string FullName { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }

    // Location Information
    public int? CityId { get; set; }
    public string? CityName { get; set; }
    public int? GovernorateId { get; set; }
    public string? GovernorateName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? ImageUrl { get; set; }
    public string? Bio { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? InstagramUrl { get; set; }
    public string? TwitterUrl { get; set; }
    public string? FacebookUrl { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? TikTokUrl { get; set; }
}

public class UpdateProfileRequest
{
    [Required(ErrorMessage = "الاسم الكامل مطلوب")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "الاسم يجب أن يكون بين 3 و 100 حرف")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "رقم الهاتف مطلوب")]
    [Phone(ErrorMessage = "رقم الهاتف غير صحيح")]
    [RegularExpression(@"^01[0125][0-9]{8}$", ErrorMessage = "رقم الهاتف يجب أن يكون رقم مصري صحيح (11 رقم)")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "الرجاء اختيار المدينة")]
    [Range(1, int.MaxValue, ErrorMessage = "الرجاء اختيار المدينة")]
    public int CityId { get; set; }

    public string? ProfileImageUrl { get; set; }
    public string? Bio { get; set; }
    public string? WebsiteUrl { get; set; }
    public string? InstagramUrl { get; set; }
    public string? TwitterUrl { get; set; }
    public string? FacebookUrl { get; set; }
    public string? LinkedInUrl { get; set; }
    public string? TikTokUrl { get; set; }
}

public class RefreshTokenRequest
{
    [Required]
    public string Token { get; set; } = string.Empty;
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}

public class CreateUserDto
{
    [Required(ErrorMessage = "الاسم الكامل مطلوب")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
    [EmailAddress(ErrorMessage = "صيغة البريد الإلكتروني غير صحيحة")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "كلمة المرور مطلوبة")]
    [MinLength(6, ErrorMessage = "كلمة المرور يجب أن لا تقل عن 6 أحرف")]
    public string Password { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }
    public int? CityId { get; set; }
    public string Role { get; set; } = "User";
}
