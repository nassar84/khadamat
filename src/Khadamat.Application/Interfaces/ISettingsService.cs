using Khadamat.Application.Common.Models;
using Khadamat.Application.DTOs;

namespace Khadamat.Application.Interfaces;

public interface ISettingsService
{
    Task<ApiResponse<AppSettingsDto>> GetSettingsAsync();
    Task<ApiResponse<bool>> UpdateSettingsAsync(UpdateAppSettingsRequest request);
}
