using System.Collections.Generic;
using System.Threading.Tasks;
using Khadamat.Application.DTOs;

namespace Khadamat.Application.Interfaces;

public interface ITesterTrackingService
{
    Task<TesterDeviceDto> RecordPingAsync(TesterPingRequest request, string? currentUserId, string? clientIp);
    Task<bool> IdentifyTesterAsync(TesterIdentifyRequest request, string? currentUserId);
    Task<TestersDashboardSummaryDto> GetDashboardAsync();
    Task<bool> AssignTesterAsync(int id, AssignTesterRequest request);
    Task<List<TesterActivityLogDto>> GetDeviceLogsAsync(int id);
    Task<bool> DeleteTesterDeviceAsync(int id);
    Task<bool> IsTrackingEnabledAsync();
    Task<bool> SetTrackingEnabledAsync(bool enabled);
}
