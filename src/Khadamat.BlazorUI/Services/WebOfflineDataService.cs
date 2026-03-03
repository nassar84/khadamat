using Khadamat.Application.DTOs;
using Khadamat.Shared.Interfaces;
using Khadamat.Application.Interfaces;
using Blazored.LocalStorage;

namespace Khadamat.BlazorUI.Services;

public class WebOfflineDataService : IOfflineDataService
{
    private readonly ILocalStorageService _localStorage;

    public WebOfflineDataService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task SaveServicesAsync(List<ServiceDto> services)
    {
        await _localStorage.SetItemAsync("offline_services", services);
    }

    public async Task<List<ServiceDto>> GetServicesAsync()
    {
        return await _localStorage.GetItemAsync<List<ServiceDto>>("offline_services") ?? new List<ServiceDto>();
    }

    public async Task SaveMainCategoriesAsync(List<MainCategoryDto> categories)
    {
        await _localStorage.SetItemAsync("offline_categories", categories);
    }

    public async Task<List<MainCategoryDto>> GetMainCategoriesAsync()
    {
        return await _localStorage.GetItemAsync<List<MainCategoryDto>>("offline_categories") ?? new List<MainCategoryDto>();
    }

    public Task AddSyncActionAsync(string action, string data)
    {
        // For web PWA we could use IndexedDB, but for now simple fallback
        return Task.CompletedTask;
    }
}
