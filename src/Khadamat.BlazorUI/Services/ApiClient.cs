using System.Net.Http.Json;
using Khadamat.Application.DTOs;
using Khadamat.Application.Common.Models;
using Khadamat.Application.Features.Services.Queries;
using System.Text.Json;
using Khadamat.Application.Features.Services.Commands;

namespace Khadamat.BlazorUI.Services;

public class ApiClient
{
    private readonly HttpClient _http;
    
    public ApiClient(HttpClient http)
    {
        _http = http;
    }

    // Services
    public async Task<PaginatedResult<ServiceDto>> GetServicesAsync(string? search = null, int? subCategoryId = null, string? userId = null, bool? isApproved = true, int page = 1, int pageSize = 10)
    {
        var url = $"api/v1/services?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(search)) url += $"&search={Uri.EscapeDataString(search)}";
        if (subCategoryId.HasValue) url += $"&subCategoryId={subCategoryId}";
        if (!string.IsNullOrEmpty(userId)) url += $"&userId={Uri.EscapeDataString(userId)}";
        if (isApproved.HasValue) url += $"&isApproved={isApproved}";
        
        return await _http.GetFromJsonAsync<PaginatedResult<ServiceDto>>(url) ?? new PaginatedResult<ServiceDto>(new List<ServiceDto>(), 0, page, pageSize);
    }

    public async Task<PaginatedResult<ServiceDto>> GetMyServicesAsync(int page = 1)
    {
        return await _http.GetFromJsonAsync<PaginatedResult<ServiceDto>>($"api/v1/services/myservices?page={page}") 
               ?? new PaginatedResult<ServiceDto>(new List<ServiceDto>(), 0, page, 10);
    }

    public async Task<ServiceDto?> GetServiceByIdAsync(int id)
    {
        return await _http.GetFromJsonAsync<ServiceDto>($"api/v1/services/{id}");
    }

    public async Task<int?> CreateServiceAsync(CreateServiceCommand command)
    {
        var response = await _http.PostAsJsonAsync("api/v1/services", command);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            return result.GetProperty("id").GetInt32();
        }
        else
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"CreateServiceAsync Failed: {response.StatusCode} - {errorContent}");
        }
        return null;
    }

    public async Task<bool> UpdateServiceAsync(int id, UpdateServiceCommand command)
    {
        var response = await _http.PutAsJsonAsync($"api/v1/services/{id}", command);
        return response.IsSuccessStatusCode;
    }

    // Categories
    public async Task<List<MainCategoryDto>> GetMainCategoriesAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<MainCategoryDto>>>("api/v1/categories/main");
        return response?.Data ?? new List<MainCategoryDto>();
    }

    public async Task<List<CategoryDto>> GetCategoriesByMainIdAsync(int mainId)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<CategoryDto>>>($"api/v1/categories/main/{mainId}/categories");
        return response?.Data ?? new List<CategoryDto>();
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<CategoryDto>>($"api/v1/categories/categories/{id}");
        return response?.Data;
    }

    public async Task<SubCategoryDto?> GetSubCategoryByIdAsync(int id)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<SubCategoryDto>>($"api/v1/categories/subcategories/{id}");
        return response?.Data;
    }

    public async Task<List<SubCategoryDto>> GetSubCategoriesByCategoryIdAsync(int catId)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<SubCategoryDto>>>($"api/v1/categories/{catId}/subcategories");
        return response?.Data ?? new List<SubCategoryDto>();
    }

    // Category Management
    public async Task<bool> CreateMainCategoryAsync(MainCategoryDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/v1/categories/main", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateMainCategoryAsync(int id, MainCategoryDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/v1/categories/main/{id}", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteMainCategoryAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/v1/categories/main/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CreateCategoryAsync(CategoryDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/v1/categories", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateCategoryAsync(int id, CategoryDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/v1/categories/{id}", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/v1/categories/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CreateSubCategoryAsync(SubCategoryDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/v1/categories/sub", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateSubCategoryAsync(int id, SubCategoryDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/v1/categories/sub/{id}", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteSubCategoryAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/v1/categories/sub/{id}");
        return response.IsSuccessStatusCode;
    }

    // Locations
    public async Task<List<GovernorateDto>> GetGovernoratesAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<GovernorateDto>>>("api/v1/locations/governorates");
        return response?.Data ?? new List<GovernorateDto>();
    }

    public async Task<List<CityDto>> GetCitiesAsync(int governorateId)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<CityDto>>>($"api/v1/locations/governorates/{governorateId}/cities");
        return response?.Data ?? new List<CityDto>();
    }

    public async Task<List<CityDto>> GetCitiesAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<CityDto>>>("api/v1/locations/cities");
        return response?.Data ?? new List<CityDto>();
    }

    public async Task<bool> CreateGovernorateAsync(GovernorateDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/v1/locations/governorates", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateGovernorateAsync(int id, GovernorateDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/v1/locations/governorates/{id}", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteGovernorateAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/v1/locations/governorates/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CreateCityAsync(CityDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/v1/locations/cities", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateCityAsync(int id, CityDto dto)
    {
        var response = await _http.PutAsJsonAsync($"api/v1/locations/cities/{id}", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteCityAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/v1/locations/cities/{id}");
        return response.IsSuccessStatusCode;
    }

    // Ads
    public async Task<List<EnhancedAdDto>> GetSliderAdsAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<EnhancedAdDto>>>("api/v1/ads/slider");
        return response?.Data ?? new List<EnhancedAdDto>();
    }

    public async Task<List<EnhancedAdDto>> GetAllAdsAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<EnhancedAdDto>>>("api/v1/ads");
        return response?.Data ?? new List<EnhancedAdDto>();
    }

    public async Task<bool> CreateAdAsync(EnhancedAdDto ad)
    {
        var response = await _http.PostAsJsonAsync("api/v1/ads", ad);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateAdAsync(int id, EnhancedAdDto ad)
    {
        var response = await _http.PutAsJsonAsync($"api/v1/ads/{id}", ad);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAdAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/v1/ads/{id}");
        return response.IsSuccessStatusCode;
    }

    // Auth
    public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
    {
        var response = await _http.PostAsJsonAsync("api/v1/auth/login", loginDto);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        }
        return null;
    }

    public async Task<ApiResponse<AuthResponse>?> GetProfileAsync()
    {
        return await _http.GetFromJsonAsync<ApiResponse<AuthResponse>>("api/v1/auth/profile");
    }

    public async Task<dynamic?> GetProviderProfileAsync(string userId)
    {
        try
        {
            return await _http.GetFromJsonAsync<dynamic>($"api/v1/providers/{userId}");
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> ApplyProviderAsync(ApplyProviderDto dto)
    {
        var response = await _http.PostAsJsonAsync("api/v1/providers/apply", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateProviderProfileAsync(UpdateProviderProfileRequest dto)
    {
        var response = await _http.PutAsJsonAsync("api/v1/providers/profile", dto);
        return response.IsSuccessStatusCode;
    }

    // Admin
    public async Task<AdminStatsDto?> GetAdminStatsAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<AdminStatsDto>>("api/v1/admin/stats");
        return response?.Data;
    }

    public async Task<List<UserDto>> GetUsersManagementAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<UserDto>>>("api/v1/admin/users");
        return response?.Data ?? new List<UserDto>();
    }

    public async Task<List<PendingProviderDto>> GetPendingProvidersAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<PendingProviderDto>>>("api/v1/admin/providers/pending");
        return response?.Data ?? new List<PendingProviderDto>();
    }

    public async Task<bool> ApproveProviderAsync(int id)
    {
        var response = await _http.PostAsync($"api/v1/admin/providers/{id}/approve", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<RecentActivityDto>> GetRecentAuditLogsAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<RecentActivityDto>>>("api/v1/admin/audit-logs/recent");
        return response?.Data ?? new List<RecentActivityDto>();
    }

    public async Task<bool> RejectProviderAsync(int id)
    {
        var response = await _http.PostAsync($"api/v1/admin/providers/{id}/reject", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ToggleUserStatusAsync(string id)
    {
        var response = await _http.PostAsync($"api/v1/admin/users/{id}/toggle-status", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteUserAsync(string id)
    {
        var response = await _http.DeleteAsync($"api/v1/admin/users/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ApproveServiceAsync(int id)
    {
        var response = await _http.PostAsync($"api/v1/admin/services/{id}/approve", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RejectServiceAsync(int id)
    {
        var response = await _http.PostAsync($"api/v1/admin/services/{id}/reject", null);
        return response.IsSuccessStatusCode;
    }

    // Posts
    public async Task<List<PostDto>> GetProviderPostsAsync(int providerId)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<PostDto>>>($"api/v1/posts/provider/{providerId}");
        return response?.Data ?? new List<PostDto>();
    }

    public async Task<bool> CreatePostAsync(CreatePostRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/v1/posts", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeletePostAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/v1/posts/{id}");
        return response.IsSuccessStatusCode;
    }

    // Reviews (My Ratings/Comments)
    public async Task<List<MyReviewDto>> GetMyReviewsAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<MyReviewDto>>>("api/v1/reviews/my");
        return response?.Data ?? new List<MyReviewDto>();
    }

    public async Task<bool> CreateReviewAsync(CreateReviewRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/v1/reviews", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteReviewAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/v1/reviews/{id}");
        return response.IsSuccessStatusCode;
    }

    // Favorites
    public async Task<List<ServiceDto>> GetMyFavoritesAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<ServiceDto>>>("api/v1/favorites");
        return response?.Data ?? new List<ServiceDto>();
    }

    public async Task<bool> ToggleFavoriteAsync(int serviceId)
    {
        var response = await _http.PostAsync($"api/v1/favorites/toggle/{serviceId}", null);
        return response.IsSuccessStatusCode;
    }

    // Comments
    public async Task<List<MyCommentDto>> GetMyCommentsAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<MyCommentDto>>>("api/v1/comments/my");
        return response?.Data ?? new List<MyCommentDto>();
    }

    public async Task<bool> CreateCommentAsync(CreateCommentRequest request)
    {
        var response = await _http.PostAsJsonAsync("api/v1/comments", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteCommentAsync(int id)
    {
        var response = await _http.DeleteAsync($"api/v1/comments/{id}");
        return response.IsSuccessStatusCode;
    }
}

// DTOs for Client usage
public class CreatePostRequest
{
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
}

public class CreateReviewRequest
{
    public int ServiceId { get; set; }
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
}

public class MyReviewDto
{
    public int Id { get; set; }
    public int ServiceId { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class CreateCommentRequest
{
    public int PostId { get; set; }
    public string Text { get; set; } = string.Empty;
}

public class MyCommentDto
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public string PostContentSnippet { get; set; } = string.Empty;
    public string ProviderName { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class LoginDto { public string Email { get; set; } = ""; public string Password { get; set; } = ""; }
public class AuthResponseDto { public string Token { get; set; } = ""; public string RefreshToken { get; set; } = ""; public string FullName { get; set; } = ""; }
