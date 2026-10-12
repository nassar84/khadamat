using System.Collections.Generic;
using System.Threading.Tasks;

namespace Khadamat.Application.Interfaces;

public interface IFcmPushService
{
    Task<bool> RegisterDeviceTokenAsync(string userId, string token, string platform, string? deviceId = null, string? appVersion = null);
    Task<bool> UnregisterDeviceTokenAsync(string token);
    Task<bool> SendPushToUserAsync(string userId, string title, string body, string? actionRoute = null, Dictionary<string, string>? data = null);
    Task<int> SendPushToUsersAsync(IEnumerable<string> userIds, string title, string body, string? actionRoute = null, Dictionary<string, string>? data = null);
    Task<int> SendBroadcastPushAsync(string title, string body, string? actionRoute = null, Dictionary<string, string>? data = null);
}
