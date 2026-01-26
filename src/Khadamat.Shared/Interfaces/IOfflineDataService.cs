using Khadamat.Application.DTOs;

namespace Khadamat.Shared.Interfaces;

public interface IOfflineDataService
{
    Task SaveServicesAsync(List<ServiceDto> services);
    Task<List<ServiceDto>> GetServicesAsync();
    Task AddSyncActionAsync(string action, string data);
}
