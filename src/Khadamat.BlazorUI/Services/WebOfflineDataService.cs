using Khadamat.Application.DTOs;
using Khadamat.Shared.Interfaces;
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

    public Task AddSyncActionAsync(string action, string data)
    {
        // For web PWA we could use IndexedDB, but for now simple fallback
        return Task.CompletedTask;
    }
}
