using Khadamat.Shared.Interfaces;
using Blazored.LocalStorage;

namespace Khadamat.BlazorUI.Services;

public class WebSecureStorageService : ISecureStorageService
{
    private readonly ILocalStorageService _localStorage;

    public WebSecureStorageService(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public async Task SaveAsync(string key, string value)
    {
        await _localStorage.SetItemAsync(key, value);
    }

    public async Task<string?> GetAsync(string key)
    {
        return await _localStorage.GetItemAsync<string>(key);
    }

    public void Remove(string key)
    {
        _ = _localStorage.RemoveItemAsync(key);
    }

    public Task RemoveAsync(string key)
    {
        return _localStorage.RemoveItemAsync(key).AsTask();
    }

    public void RemoveAll()
    {
        _ = _localStorage.ClearAsync();
    }

    public Task RemoveAllAsync()
    {
        return _localStorage.ClearAsync().AsTask();
    }
}
