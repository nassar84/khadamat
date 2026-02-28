using Khadamat.Shared.Interfaces;
using Microsoft.Maui.Storage;

namespace Khadamat.MobileApp.Services;

public class MauiSecureStorageService : ISecureStorageService
{
    public Task SaveAsync(string key, string value)
    {
        Preferences.Default.Set(key, value);
        return Task.CompletedTask;
    }

    public Task<string?> GetAsync(string key)
    {
        return Task.FromResult<string?>(Preferences.Default.Get<string?>(key, null));
    }

    public void Remove(string key)
    {
        Preferences.Default.Remove(key);
    }

    public void RemoveAll()
    {
        Preferences.Default.Clear();
    }
}
