using System.Net.Http.Json;
using Khadamat.Application.DTOs;
using Khadamat.Application.Common.Models;
using Khadamat.Application.Features.Services.Queries;
using System.Text.Json;
using Khadamat.Application.Features.Services.Commands;
using Microsoft.AspNetCore.Components.Forms;
using System.Net.Http.Headers;

namespace Khadamat.BlazorUI.Services;

public class ApiClient
{
    private readonly HttpClient _http;
    private readonly Khadamat.BlazorUI.State.AppState _appState;
    
    public ApiClient(HttpClient http, Khadamat.BlazorUI.State.AppState appState)
    {
        _http = http;
        _appState = appState;
    }

    public string BaseUrl => _http.BaseAddress?.ToString() ?? "";

    public string GetAbsoluteUrl(string? relativeUrl)
    {
        if (string.IsNullOrEmpty(relativeUrl)) return "";

        relativeUrl = relativeUrl.Replace("\\", "/");

        if (relativeUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase) || 
            relativeUrl.StartsWith("data:", StringComparison.OrdinalIgnoreCase)) 
            return relativeUrl;
            
        return $"{BaseUrl.TrimEnd('/')}/{relativeUrl.TrimStart('/')}";
    }

    public async Task<T?> PostAsync<T>(string url, object data)
    {
        var response = await _http.PostAsJsonAsync(url, data);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<T>();
        return default;
    }

    public async Task<T?> PostMultipartAsync<T>(string url, MultipartFormDataContent content)
    {
        var response = await _http.PostAsync(url, content);
        if (response.IsSuccessStatusCode)
            return await response.Content.ReadFromJsonAsync<T>();
        return default;
    }

    private static ApiResponse<AppSettingsDto>? _settingsCache;
    private static List<MainCategoryDto>? _mainCategoriesCache;
    private static List<GovernorateDto>? _governoratesCache;
    private static List<MarketplaceCategoryDto>? _marketCategoriesCache;
    private static readonly System.Collections.Generic.Dictionary<int, List<CityDto>> _citiesCache = new();

    // Settings
    public async Task<ApiResponse<AppSettingsDto>> GetSettingsAsync()
    {
        if (_settingsCache != null) return _settingsCache;
        _settingsCache = await _http.GetFromJsonAsync<ApiResponse<AppSettingsDto>>("v1/settings") 
               ?? ApiResponse<AppSettingsDto>.Fail("Failed to fetch settings");
        return _settingsCache;
    }

    public async Task<ApiResponse<bool>> UpdateSettingsAsync(UpdateAppSettingsRequest request)
    {
        var response = await _http.PutAsJsonAsync("v1/settings", request);
        _settingsCache = null; // Invalidate cache
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>() 
               ?? ApiResponse<bool>.Fail("Failed to update settings");
    }
 
    // Services
    public async Task<PaginatedResult<ServiceDto>> GetServicesAsync(string? search = null, int? categoryId = null, int? subCategoryId = null, int? governorateId = null, int? cityId = null, string? userId = null, bool? isApproved = true, string? sortBy = "latest", int page = 1, int pageSize = 10)
    {
        var url = $"v1/services?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(search)) url += $"&search={Uri.EscapeDataString(search)}";
        if (categoryId.HasValue) url += $"&categoryId={categoryId}";
        if (subCategoryId.HasValue) url += $"&subCategoryId={subCategoryId}";
        if (governorateId.HasValue) url += $"&governorateId={governorateId}";
        if (cityId.HasValue) url += $"&cityId={cityId}";
        if (!string.IsNullOrEmpty(userId)) url += $"&userId={Uri.EscapeDataString(userId)}";
        if (isApproved.HasValue) url += $"&isApproved={isApproved.Value.ToString().ToLower()}";
        if (!string.IsNullOrEmpty(sortBy)) url += $"&sortBy={sortBy}";
        
        try
        {
            return await _http.GetFromJsonAsync<PaginatedResult<ServiceDto>>(url) ?? new PaginatedResult<ServiceDto>(new List<ServiceDto>(), 0, page, pageSize);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching services: {ex.Message}");
            return new PaginatedResult<ServiceDto>(new List<ServiceDto>(), 0, page, pageSize);
        }
    }

    public async Task<PaginatedResult<ServiceDto>> GetMyServicesAsync(int page = 1)
    {
        try
        {
            return await _http.GetFromJsonAsync<PaginatedResult<ServiceDto>>($"v1/services/myservices?page={page}") 
                   ?? new PaginatedResult<ServiceDto>(new List<ServiceDto>(), 0, page, 10);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching my services: {ex.Message}");
            return new PaginatedResult<ServiceDto>(new List<ServiceDto>(), 0, page, 10);
        }
    }

    public async Task<ServiceDto?> GetServiceByIdAsync(int id)
    {
        return await _http.GetFromJsonAsync<ServiceDto>($"v1/services/{id}");
    }

    public async Task<PaginatedResult<ServiceDto>> GetSimilarServicesAsync(int id, int count = 4)
    {
        return await _http.GetFromJsonAsync<PaginatedResult<ServiceDto>>($"v1/services/{id}/similar?count={count}")
               ?? new PaginatedResult<ServiceDto>(new List<ServiceDto>(), 0, 1, count);
    }

    public async Task<int?> CreateServiceAsync(CreateServiceCommand command)
    {
        var response = await _http.PostAsJsonAsync("v1/services", command);
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
        var response = await _http.PutAsJsonAsync($"v1/services/{id}", command);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RequestServiceEditAsync(int serviceId, object editDto)
    {
        var response = await _http.PostAsJsonAsync($"v1/services/{serviceId}/request-edit", editDto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteServiceAsync(int id)
    {
        var response = await _http.DeleteAsync($"v1/services/{id}");
        return response.IsSuccessStatusCode;
    }

    // Categories
    public async Task<List<MainCategoryDto>> GetMainCategoriesAsync()
    {
        if (_mainCategoriesCache != null) return _mainCategoriesCache;
        try 
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<List<MainCategoryDto>>>("v1/categories/main");
            _mainCategoriesCache = response?.Data ?? new List<MainCategoryDto>();
            return _mainCategoriesCache;
        }
        catch
        {
            return new List<MainCategoryDto>();
        }
    }

    public async Task<List<CategoryDto>> GetCategoriesByMainIdAsync(int mainId)
    {
        try
        {
            var url = $"v1/categories/main/{mainId}/categories";
            Console.WriteLine($"ApiClient.GetCategoriesByMainIdAsync: {url}");
            var response = await _http.GetFromJsonAsync<ApiResponse<List<CategoryDto>>>(url);
            var count = response?.Data?.Count ?? 0;
            Console.WriteLine($"ApiClient.GetCategoriesByMainIdAsync success: {count} items");
            return response?.Data ?? new List<CategoryDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ApiClient.GetCategoriesByMainIdAsync ERROR: {ex.Message}");
            return new List<CategoryDto>();
        }
    }

    public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
    {
        try
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<CategoryDto>>($"v1/categories/categories/{id}");
            return response?.Data;
        }
        catch
        {
            return null;
        }
    }

    public async Task<SubCategoryDto?> GetSubCategoryByIdAsync(int id)
    {
        try
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<SubCategoryDto>>($"v1/categories/subcategories/{id}");
            return response?.Data;
        }
        catch
        {
            return null;
        }
    }

    public async Task<List<SubCategoryDto>> GetSubCategoriesByCategoryIdAsync(int catId)
    {
        try
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<List<SubCategoryDto>>>($"v1/categories/{catId}/subcategories");
            return response?.Data ?? new List<SubCategoryDto>();
        }
        catch
        {
            return new List<SubCategoryDto>();
        }
    }

    // Category Management
    public async Task<bool> CreateMainCategoryAsync(MainCategoryDto dto)
    {
        var response = await _http.PostAsJsonAsync("v1/categories/main", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateMainCategoryAsync(int id, MainCategoryDto dto)
    {
        var response = await _http.PutAsJsonAsync($"v1/categories/main/{id}", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteMainCategoryAsync(int id)
    {
        var response = await _http.DeleteAsync($"v1/categories/main/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CreateCategoryAsync(CategoryDto dto)
    {
        var response = await _http.PostAsJsonAsync("v1/categories", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateCategoryAsync(int id, CategoryDto dto)
    {
        var response = await _http.PutAsJsonAsync($"v1/categories/{id}", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var response = await _http.DeleteAsync($"v1/categories/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CreateSubCategoryAsync(SubCategoryDto dto)
    {
        var response = await _http.PostAsJsonAsync("v1/categories/sub", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateSubCategoryAsync(int id, SubCategoryDto dto)
    {
        var response = await _http.PutAsJsonAsync($"v1/categories/sub/{id}", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteSubCategoryAsync(int id)
    {
        var response = await _http.DeleteAsync($"v1/categories/sub/{id}");
        return response.IsSuccessStatusCode;
    }

    // Locations
    public async Task<List<GovernorateDto>> GetGovernoratesAsync()
    {
        if (_governoratesCache != null) return _governoratesCache;
        var response = await _http.GetFromJsonAsync<ApiResponse<List<GovernorateDto>>>("v1/locations/governorates");
        _governoratesCache = response?.Data ?? new List<GovernorateDto>();
        return _governoratesCache;
    }

    public async Task<List<CityDto>> GetCitiesAsync(int governorateId)
    {
        lock (_citiesCache)
        {
            if (_citiesCache.TryGetValue(governorateId, out var cachedCities))
            {
                return cachedCities;
            }
        }

        try
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<List<CityDto>>>($"v1/locations/governorates/{governorateId}/cities");
            var cities = response?.Data ?? new List<CityDto>();
            if (cities.Any())
            {
                lock (_citiesCache)
                {
                    _citiesCache[governorateId] = cities;
                }
            }
            return cities;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching cities: {ex.Message}");
            return new List<CityDto>();
        }
    }

    public async Task<List<CityDto>> GetCitiesAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<CityDto>>>("v1/locations/cities");
        return response?.Data ?? new List<CityDto>();
    }

    public async Task<CityDto?> GetCityByIdAsync(int id)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<CityDto>>($"v1/locations/cities/{id}");
        return response?.Data;
    }

    public async Task<bool> CreateGovernorateAsync(GovernorateDto dto)
    {
        var response = await _http.PostAsJsonAsync("v1/locations/governorates", dto);
        if (response.IsSuccessStatusCode) _governoratesCache = null;
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateGovernorateAsync(int id, GovernorateDto dto)
    {
        var response = await _http.PutAsJsonAsync($"v1/locations/governorates/{id}", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteGovernorateAsync(int id)
    {
        var response = await _http.DeleteAsync($"v1/locations/governorates/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> CreateCityAsync(CityDto dto)
    {
        var response = await _http.PostAsJsonAsync("v1/locations/cities", dto);
        if (response.IsSuccessStatusCode)
        {
            lock (_citiesCache)
            {
                _citiesCache.Remove(dto.GovernorateId);
            }
        }
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateCityAsync(int id, CityDto dto)
    {
        var response = await _http.PutAsJsonAsync($"v1/locations/cities/{id}", dto);
        if (response.IsSuccessStatusCode)
        {
            lock (_citiesCache)
            {
                _citiesCache.Clear();
            }
        }
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteCityAsync(int id)
    {
        var response = await _http.DeleteAsync($"v1/locations/cities/{id}");
        if (response.IsSuccessStatusCode)
        {
            lock (_citiesCache)
            {
                _citiesCache.Clear();
            }
        }
        return response.IsSuccessStatusCode;
    }

    // Ad Packages
    public async Task<List<AdPackageDto>> GetAdPackagesAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<AdPackageDto>>>("v1/adpackages");
        return response?.Data ?? new List<AdPackageDto>();
    }

    public async Task<bool> CreateAdPackageAsync(CreateAdPackageRequest request)
    {
        var response = await _http.PostAsJsonAsync("v1/adpackages", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateAdPackageAsync(int id, CreateAdPackageRequest request)
    {
        var response = await _http.PutAsJsonAsync($"v1/adpackages/{id}", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAdPackageAsync(int id)
    {
        var response = await _http.DeleteAsync($"v1/adpackages/{id}");
        return response.IsSuccessStatusCode;
    }

    // Ads
    public async Task<List<EnhancedAdDto>> GetSliderAdsAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<EnhancedAdDto>>>("v1/ads/slider");
        return response?.Data ?? new List<EnhancedAdDto>();
    }

    public async Task<List<EnhancedAdDto>> GetAllAdsAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<EnhancedAdDto>>>("v1/ads");
        return response?.Data ?? new List<EnhancedAdDto>();
    }

    public async Task<List<EnhancedAdDto>> GetSearchAdsAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<EnhancedAdDto>>>("v1/ads/placements/Search");
        return response?.Data ?? new List<EnhancedAdDto>();
    }

    public async Task<List<EnhancedAdDto>> GetAdsByPlacementAsync(string placement)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<EnhancedAdDto>>>($"v1/ads/placements/{placement}");
        return response?.Data ?? new List<EnhancedAdDto>();
    }

    public async Task<List<EnhancedAdDto>> GetCategoryAdsAsync(int categoryId)
    {
        var ads = await GetAdsByPlacementAsync("Category");
        // Filter ads matching the specific category, or ads targeting all categories (null)
        return ads.Where(a => string.IsNullOrEmpty(a.TargetCategories) || a.TargetCategories == categoryId.ToString()).ToList();
    }

    public async Task<bool> CreateAdAsync(EnhancedAdDto ad)
    {
        var response = await _http.PostAsJsonAsync("v1/ads", ad);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateAdAsync(int id, EnhancedAdDto ad)
    {
        var response = await _http.PutAsJsonAsync($"v1/ads/{id}", ad);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteAdAsync(int id)
    {
        var response = await _http.DeleteAsync($"v1/ads/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task TrackAdViewAsync(int adId)
    {
        await _http.PostAsync($"v1/ads/{adId}/track-view", null);
    }

    public async Task TrackAdClickAsync(int adId)
    {
        await _http.PostAsync($"v1/ads/{adId}/track-click", null);
    }

    public async Task<List<EnhancedAdDto>> GetMyAdsAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<EnhancedAdDto>>>("v1/ads/my");
        return response?.Data ?? new List<EnhancedAdDto>();
    }

    // Advertisements System - Referral & Points
    public async Task<ReferralCodeDto?> GetMyReferralCodeAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<ReferralCodeDto>("v1/Advertisements/referral/my-code");
        }
        catch { return null; }
    }

    public async Task<int> GetMyPointsBalanceAsync()
    {
        try
        {
            var response = await _http.GetFromJsonAsync<PointsBalanceDto>("v1/Advertisements/points/balance");
            return response?.Balance ?? 0;
        }
        catch { return 0; }
    }

    public async Task<bool> ConvertPointsToAdDaysAsync(ConvertPointsRequestDto request)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("v1/Advertisements/points/convert", request);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    // ── Economy: Promos & Analytics (Phases 8 & 11) ───────────────────────

    public async Task<List<PromoCodeDto>> GetActivePromotionsAsync()
    {
        try
        {
            var response = await _http.GetFromJsonAsync<List<PromoCodeDto>>("v1/Advertisements/promotions");
            return response ?? new List<PromoCodeDto>();
        }
        catch { return new List<PromoCodeDto>(); }
    }

    public async Task<bool> ApplyPromoCodeAsync(ApplyPromoRequestDto request)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("v1/Advertisements/apply-promo", request);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<AdAnalyticsDto?> GetMyAnalyticsAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<AdAnalyticsDto>("v1/Advertisements/my-analytics");
        }
        catch { return null; }
    }

    public async Task<AdAnalyticsDto?> GetGlobalAnalyticsAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<AdAnalyticsDto>("v1/Advertisements/admin/analytics");
        }
        catch { return null; }
    }

    // Auth
    public async Task<AuthResponseDto?> LoginAsync(LoginDto loginDto)
    {
        var response = await _http.PostAsJsonAsync("v1/auth/login", loginDto);
        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<AuthResponseDto>();
        }
        return null;
    }

    public async Task<ApiResponse<AuthResponse>?> GetProfileAsync()
    {
        return await _http.GetFromJsonAsync<ApiResponse<AuthResponse>>("v1/auth/profile");
    }

    public async Task<dynamic?> GetProviderProfileAsync(string userId)
    {
        try
        {
            return await _http.GetFromJsonAsync<dynamic>($"v1/providers/{userId}");
        }
        catch
        {
            return null;
        }
    }

    public async Task<bool> ApplyProviderAsync(ApplyProviderDto dto)
    {
        var response = await _http.PostAsJsonAsync("v1/providers/apply", dto);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateProviderProfileAsync(UpdateProviderProfileRequest dto)
    {
        var response = await _http.PutAsJsonAsync("v1/providers/profile", dto);
        return response.IsSuccessStatusCode;
    }

    // Admin
    public async Task<AdminStatsDto?> GetAdminStatsAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<AdminStatsDto>>("v1/admin/stats");
        return response?.Data;
    }

    public async Task<List<UserDto>> GetUsersManagementAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<UserDto>>>("v1/admin/users");
        return response?.Data ?? new List<UserDto>();
    }

    public async Task<List<PendingProviderDto>> GetPendingProvidersAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<PendingProviderDto>>>("v1/admin/providers/pending");
        return response?.Data ?? new List<PendingProviderDto>();
    }

    public async Task<bool> ApproveProviderAsync(int id)
    {
        var response = await _http.PostAsync($"v1/admin/providers/{id}/approve", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<RecentActivityDto>> GetRecentAuditLogsAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<RecentActivityDto>>>("v1/admin/audit-logs/recent");
        return response?.Data ?? new List<RecentActivityDto>();
    }

    public async Task<bool> RejectProviderAsync(int id)
    {
        var response = await _http.PostAsync($"v1/admin/providers/{id}/reject", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ToggleUserStatusAsync(string id)
    {
        var response = await _http.PostAsync($"v1/admin/users/{id}/toggle-status", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteUserAsync(string id)
    {
        var response = await _http.DeleteAsync($"v1/admin/users/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ApproveServiceAsync(int id)
    {
        var response = await _http.PostAsync($"v1/admin/services/{id}/approve", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RejectServiceAsync(int id)
    {
        var response = await _http.PostAsync($"v1/admin/services/{id}/reject", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<ServiceEditRequestDto>> GetServiceEditRequestsAsync(string? status = null)
    {
        var url = "v1/admin/services/edit-requests";
        if (!string.IsNullOrEmpty(status)) url += $"?status={status}";
        
        var response = await _http.GetFromJsonAsync<ApiResponse<List<ServiceEditRequestDto>>>(url);
        return response?.Data ?? new List<ServiceEditRequestDto>();
    }

    public async Task<bool> UpdateServiceEditRequestStatusAsync(int id, Khadamat.Application.Features.Services.Commands.UpdateServiceEditRequestCommand command)
    {
        var response = await _http.PostAsJsonAsync($"v1/admin/services/edit-requests/{id}/status", command);
        return response.IsSuccessStatusCode;
    }

    // Posts
    public async Task<List<PostDto>> GetProviderPostsAsync(int providerId)
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<PostDto>>>($"v1/posts/provider/{providerId}");
        return response?.Data ?? new List<PostDto>();
    }

    public async Task<bool> CreatePostAsync(CreatePostRequest request)
    {
        var response = await _http.PostAsJsonAsync("v1/posts", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeletePostAsync(int id)
    {
        var response = await _http.DeleteAsync($"v1/posts/{id}");
        return response.IsSuccessStatusCode;
    }

    // Reviews (My Ratings/Comments)
    public async Task<List<MyReviewDto>> GetMyReviewsAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<MyReviewDto>>>("v1/reviews/my");
        return response?.Data ?? new List<MyReviewDto>();
    }

    public async Task<bool> CreateReviewAsync(CreateReviewRequest request)
    {
        var response = await _http.PostAsJsonAsync("v1/reviews", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> AddReviewAsync(int serviceId, int rating, string comment)
    {
        return await CreateReviewAsync(new CreateReviewRequest { ServiceId = serviceId, Rating = rating, Comment = comment });
    }

    public async Task<bool> DeleteReviewAsync(int id)
    {
        var response = await _http.DeleteAsync($"v1/reviews/{id}");
        return response.IsSuccessStatusCode;
    }

    // Favorites
    public async Task<List<ServiceDto>> GetMyFavoritesAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<ServiceDto>>>("v1/favorites");
        return response?.Data ?? new List<ServiceDto>();
    }

    public async Task<bool> ToggleFavoriteAsync(int serviceId)
    {
        var response = await _http.PostAsync($"v1/favorites/toggle/{serviceId}", null);
        return response.IsSuccessStatusCode;
    }

    // Comments
    public async Task<List<MyCommentDto>> GetMyCommentsAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<MyCommentDto>>>("v1/comments/my");
        return response?.Data ?? new List<MyCommentDto>();
    }

    public async Task<bool> CreateCommentAsync(CreateCommentRequest request)
    {
        var response = await _http.PostAsJsonAsync("v1/comments", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteCommentAsync(int id)
    {
        var response = await _http.DeleteAsync($"v1/comments/{id}");
        return response.IsSuccessStatusCode;
    }
    // Notifications
    public async Task<List<NotificationDto>> GetMyNotificationsAsync()
    {
        var response = await _http.GetFromJsonAsync<ApiResponse<List<NotificationDto>>>("v1/notifications");
        return response?.Data ?? new List<NotificationDto>();
    }

    public async Task<bool> MarkNotificationAsReadAsync(int id)
    {
        var response = await _http.PostAsync($"v1/notifications/{id}/read", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> RecordShareAsync(string itemType, int itemId)
    {
        var response = await _http.PostAsJsonAsync("v1/Advertisements/record-share", new { ItemType = itemType, ItemId = itemId });
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> MarkAllNotificationsAsReadAsync()
    {
        var response = await _http.PostAsync("v1/notifications/read-all", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> SendAdminNotificationAsync(string? userId, string title, string message, string? link = null, string? role = null, int? govId = null, int? cityId = null, int? catId = null)
    {
        var request = new { 
            UserId = userId, 
            Title = title, 
            Message = message, 
            RelatedLink = link,
            TargetRole = role,
            GovernorateId = govId,
            CityId = cityId,
            MainCategoryId = catId
        };
        var response = await _http.PostAsJsonAsync("v1/notifications/send", request);
        return response.IsSuccessStatusCode;
    }

    // Messages
    public async Task<List<ConversationDto>> GetConversationsAsync()
    {
        try
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<List<ConversationDto>>>("v1/messages");
            return response?.Data ?? new List<ConversationDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching conversations: {ex.Message}");
            return new List<ConversationDto>();
        }
    }

    public async Task<List<MessageDto>> GetMessagesAsync(string userId)
    {
        try
        {
            var response = await _http.GetFromJsonAsync<ApiResponse<List<MessageDto>>>($"v1/messages/{userId}");
            return response?.Data ?? new List<MessageDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching messages: {ex.Message}");
            return new List<MessageDto>();
        }
    }

    public async Task<bool> SendMessageAsync(string receiverId, string content)
    {
        try
        {
            var request = new { ReceiverId = receiverId, Content = content };
            var response = await _http.PostAsJsonAsync("v1/messages", request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message: {ex.Message}");
            return false;
        }
    }

    // Subscriptions
    public async Task<List<SubscriptionPlanDto>> GetSubscriptionPlansAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<List<SubscriptionPlanDto>>("v1/subscriptions/plans") 
                   ?? new List<SubscriptionPlanDto>();
        }
        catch
        {
            return new List<SubscriptionPlanDto>();
        }
    }

    public async Task<ProviderSubscriptionDto?> GetMySubscriptionAsync()
    {
        try
        {
            var response = await _http.GetAsync("v1/subscriptions/my-subscription");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ProviderSubscriptionDto>();
            }
        }
        catch { }
        return null;
    }

    public async Task<ApiResponse<bool>> SubscribeAsync(int planId)
    {
        var response = await _http.PostAsJsonAsync("v1/subscriptions/subscribe", new SubscribeRequest { PlanId = planId });
        return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>() 
               ?? ApiResponse<bool>.Fail("فشل الاشتراك في الباقة");
    }

    // Service Requests
    public async Task<List<ServiceRequestDto>> GetMyRequestsAsync()
    {
        try
        {
            var response = await _http.GetFromJsonAsync<List<ServiceRequestDto>>("v1/requests/my-requests");
            return response ?? new List<ServiceRequestDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching requests: {ex.Message}");
            return new List<ServiceRequestDto>();
        }
    }

    public async Task<ApiResponse<int>> CreateRequestAsync(CreateServiceRequestDto dto)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("v1/requests", dto);
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<int>>() 
                       ?? ApiResponse<int>.Fail("فشل في إنشاء الطلب");
            }
            return ApiResponse<int>.Fail("فشل في إنشاء الطلب");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error creating request: {ex.Message}");
            return ApiResponse<int>.Fail(ex.Message);
        }
    }

    public async Task<List<ServiceRequestDto>> GetProviderRequestsAsync()
    {
        try
        {
            var response = await _http.GetFromJsonAsync<List<ServiceRequestDto>>("v1/requests/provider-requests");
            return response ?? new List<ServiceRequestDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching provider requests: {ex.Message}");
            return new List<ServiceRequestDto>();
        }
    }

    public async Task<ApiResponse<bool>> UpdateRequestStatusAsync(int requestId, Khadamat.Domain.Enums.RequestStatus status, string? notes = null)
    {
        try
        {
            var command = new { Status = status, ProviderNotes = notes };
            var response = await _http.PutAsJsonAsync($"v1/requests/{requestId}/status", command);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>() 
                       ?? ApiResponse<bool>.Fail("فشل في تحديث حالة الطلب");
            }
            return ApiResponse<bool>.Fail("فشل في تحديث حالة الطلب");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error updating request status: {ex.Message}");
            return ApiResponse<bool>.Fail(ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> CancelRequestAsync(int requestId)
    {
        try
        {
            var response = await _http.PutAsync($"v1/requests/my-requests/{requestId}/cancel", null);
            
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>() 
                       ?? ApiResponse<bool>.Fail("فشل في إلغاء الطلب");
            }
            return ApiResponse<bool>.Fail("فشل في إلغاء الطلب");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error canceling request: {ex.Message}");
            return ApiResponse<bool>.Fail(ex.Message);
        }
    }

    // Marketplace
    public async Task<List<MarketplaceItemDto>> GetLatestMarketplaceItemsAsync(int count = 10)
    {
        try
        {
            return await _http.GetFromJsonAsync<List<MarketplaceItemDto>>($"v1/Marketplace/latest?count={count}") ?? new List<MarketplaceItemDto>();
        }
        catch { return new List<MarketplaceItemDto>(); }
    }

    public async Task<List<MarketplaceItemDto>> GetFeaturedMarketplaceItemsAsync(int count = 6)
    {
        try
        {
            return await _http.GetFromJsonAsync<List<MarketplaceItemDto>>($"v1/Marketplace/featured?count={count}") ?? new List<MarketplaceItemDto>();
        }
        catch { return new List<MarketplaceItemDto>(); }
    }

    public async Task<List<MarketplaceItemDto>> SearchMarketplaceItemsAsync(string? q = null, int? categoryId = null, int? subCategoryId = null, int? governorateId = null, int? cityId = null, string? condition = null, decimal? minPrice = null, decimal? maxPrice = null, int page = 1, int pageSize = 12)
    {
        try
        {
            var url = $"v1/Marketplace/search?page={page}&pageSize={pageSize}";
            if (!string.IsNullOrEmpty(q)) url += $"&q={Uri.EscapeDataString(q)}";
            if (categoryId.HasValue && categoryId > 0) url += $"&categoryId={categoryId}";
            if (subCategoryId.HasValue && subCategoryId > 0) url += $"&subCategoryId={subCategoryId}";
            if (governorateId.HasValue && governorateId > 0) url += $"&governorateId={governorateId}";
            if (cityId.HasValue && cityId > 0) url += $"&cityId={cityId}";
            if (!string.IsNullOrEmpty(condition)) url += $"&condition={condition}";
            if (minPrice.HasValue && minPrice > 0) url += $"&minPrice={minPrice}";
            if (maxPrice.HasValue && maxPrice > 0) url += $"&maxPrice={maxPrice}";

            var result = await _http.GetFromJsonAsync<PaginatedResult<MarketplaceItemDto>>(url);
            return result?.Items ?? new List<MarketplaceItemDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error searching marketplace: {ex.Message}");
            return new List<MarketplaceItemDto>();
        }
    }

    public async Task<MarketplaceItemDto?> GetMarketplaceItemByIdAsync(int id)
    {
        try
        {
            return await _http.GetFromJsonAsync<MarketplaceItemDto>($"v1/Marketplace/{id}");
        }
        catch { return null; }
    }

    public async Task<(MarketplaceItemDto? Item, string? Error)> CreateMarketplaceItemAsync(CreateMarketplaceItemRequest request)
    {
        try 
        {
            var response = await _http.PostAsJsonAsync("v1/Marketplace", request);
            if (response.IsSuccessStatusCode)
            {
                var item = await response.Content.ReadFromJsonAsync<MarketplaceItemDto>();
                return (item, null);
            }
            
            var error = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"ANTIGRAVITY_LOG: CreateMarketplaceItem Error: {response.StatusCode} - {error}");
            return (null, error);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ANTIGRAVITY_LOG: CreateMarketplaceItem Exception: {ex.Message}");
            return (null, ex.Message);
        }
    }

    public async Task<bool> UpdateMarketplaceItemAsync(int id, CreateMarketplaceItemRequest request)
    {
        var response = await _http.PutAsJsonAsync($"v1/Marketplace/{id}", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteMarketplaceItemAsync(int id)
    {
        var response = await _http.DeleteAsync($"v1/Marketplace/{id}");
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> MarkMarketplaceItemAsSoldAsync(int id)
    {
        var response = await _http.PostAsync($"v1/Marketplace/{id}/sold", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ChangeMarketplaceItemStatusAsync(int id, string status)
    {
        try
        {
            var request = new { Status = status };
            var response = await _http.PostAsJsonAsync($"v1/Marketplace/{id}/change-status", request);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<bool> LockMarketplaceItemAsync(int id)
    {
        var response = await _http.PostAsync($"v1/Marketplace/{id}/lock", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ToggleMarketplaceFavoriteAsync(int id)
    {
        var response = await _http.PostAsync($"v1/Marketplace/{id}/favorite", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> IsMarketplaceFavoriteAsync(int id)
    {
        try
        {
            return await _http.GetFromJsonAsync<bool>($"v1/Marketplace/{id}/is-favorite");
        }
        catch { return false; }
    }

    // Admin Marketplace
    public async Task<List<MarketplaceItemDto>> GetAllMarketplaceItemsAdminAsync(int pageSize = 300)
    {
        try
        {
            return await _http.GetFromJsonAsync<List<MarketplaceItemDto>>($"v1/Marketplace/admin/items?pageSize={pageSize}") ?? new List<MarketplaceItemDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"GetAllMarketplaceItemsAdmin error: {ex.Message}");
            return new List<MarketplaceItemDto>();
        }
    }

    public async Task<MarketplaceSettingsDto?> GetMarketplaceSettingsAsync()
    {
        try
        {
            return await _http.GetFromJsonAsync<MarketplaceSettingsDto>("v1/Marketplace/settings");
        }
        catch { return null; }
    }

    public async Task<bool> ApproveMarketplaceItemAsync(int id, string? notes = null)
    {
        var response = await _http.PostAsync($"v1/Marketplace/{id}/approve?notes={Uri.EscapeDataString(notes ?? "")}", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> ApproveMarketplaceItemWithDatesAsync(int id, DateTime startDate, DateTime endDate, string? notes = null)
    {
        try
        {
            var url = $"v1/Marketplace/{id}/approve?startDate={startDate:yyyy-MM-dd}&endDate={endDate:yyyy-MM-dd}";
            if (!string.IsNullOrEmpty(notes))
                url += $"&notes={Uri.EscapeDataString(notes)}";
            var response = await _http.PostAsync(url, null);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public async Task<bool> RejectMarketplaceItemAsync(int id, string? notes = null)
    {
        var response = await _http.PostAsync($"v1/Marketplace/{id}/reject?notes={Uri.EscapeDataString(notes ?? "")}", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> SetMarketplaceItemFeaturedAsync(int id, int days = 7)
    {
        var response = await _http.PostAsync($"v1/Marketplace/{id}/set-featured?days={days}", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> SetMarketplaceItemPromotedAsync(int id, int days = 7)
    {
        var response = await _http.PostAsync($"v1/Marketplace/{id}/set-promoted?days={days}", null);
        return response.IsSuccessStatusCode;
    }

    public async Task<List<MarketplaceItemDto>> GetMyMarketplaceItemsAsync()
    {
        try
        {
            var response = await _http.GetFromJsonAsync<PaginatedResult<MarketplaceItemDto>>("v1/Marketplace/my-items");
            return response?.Items ?? new List<MarketplaceItemDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching my marketplace items: {ex.Message}");
            return new List<MarketplaceItemDto>();
        }
    }

    public async Task<List<MarketplaceCategoryDto>> GetMarketplaceCategoriesAsync()
    {
        if (_marketCategoriesCache != null) return _marketCategoriesCache;
        try
        {
            _marketCategoriesCache = await _http.GetFromJsonAsync<List<MarketplaceCategoryDto>>("v1/Marketplace/categories") ?? new List<MarketplaceCategoryDto>();
            return _marketCategoriesCache;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching marketplace categories: {ex.Message}");
            return new List<MarketplaceCategoryDto>();
        }
    }

    public async Task<List<MarketplaceSubCategoryDto>> GetMarketplaceSubCategoriesAsync(int categoryId)
    {
        try
        {
            return await _http.GetFromJsonAsync<List<MarketplaceSubCategoryDto>>($"v1/Marketplace/categories/{categoryId}/subcategories") ?? new List<MarketplaceSubCategoryDto>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error fetching marketplace subcategories: {ex.Message}");
            return new List<MarketplaceSubCategoryDto>();
        }
    }

    public async Task<ImageUploadResponse> UploadImageAsync(IBrowserFile file)
    {
        try
        {
            var content = new MultipartFormDataContent();
            var fileContent = new StreamContent(file.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024));
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(fileContent, "file", file.Name);

            var response = await _http.PostAsync("v1/upload", content);
            return await response.Content.ReadFromJsonAsync<ImageUploadResponse>() 
                   ?? new ImageUploadResponse { Success = false, Message = "Failed to parse upload response" };
        }
        catch (Exception ex)
        {
            return new ImageUploadResponse { Success = false, Message = $"Upload error: {ex.Message}" };
        }
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

public class ImageUploadResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? ImageUrl { get; set; }
}
