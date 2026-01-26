namespace Khadamat.Shared.Interfaces;

public interface ISecureStorageService
{
    Task SaveAsync(string key, string value);
    Task<string?> GetAsync(string key);
    void Remove(string key);
    void RemoveAll();
}
