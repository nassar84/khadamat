using Khadamat.Shared.Interfaces;
using Microsoft.Maui.Storage;

namespace Khadamat.MobileApp.Services;

public class MauiSecureStorageService : ISecureStorageService
{
    public Task SaveAsync(string key, string value)
    {
        return SecureStorage.SetAsync(key, value);
    }

    public Task<string?> GetAsync(string key)
    {
        return SecureStorage.GetAsync(key);
    }

    public void Remove(string key)
    {
        SecureStorage.Remove(key);
    }

    public void RemoveAll()
    {
        SecureStorage.RemoveAll();
    }
}
