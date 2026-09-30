using Khadamat.Shared.Interfaces;
using Microsoft.Maui.Storage;

namespace Khadamat.MobileApp.Security;

public class MauiSecureStorageService : ISecureStorageService
{
    public async Task SaveAsync(string key, string value)
    {
        await SecureStorage.Default.SetAsync(key, value);
    }

    public async Task<string?> GetAsync(string key)
    {
        return await SecureStorage.Default.GetAsync(key);
    }

    public void Remove(string key)
    {
        SecureStorage.Default.Remove(key);
    }

    public Task RemoveAsync(string key)
    {
        SecureStorage.Default.Remove(key);
        return Task.CompletedTask;
    }

    public void RemoveAll()
    {
        SecureStorage.Default.RemoveAll();
    }

    public Task RemoveAllAsync()
    {
        SecureStorage.Default.RemoveAll();
        return Task.CompletedTask;
    }
}
