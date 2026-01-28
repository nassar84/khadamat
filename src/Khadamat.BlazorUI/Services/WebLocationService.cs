using Khadamat.Shared.Interfaces;
using Microsoft.JSInterop;

namespace Khadamat.BlazorUI.Services;

public class WebLocationService : ILocationService
{
    private readonly IJSRuntime _js;

    public WebLocationService(IJSRuntime js)
    {
        _js = js;
    }

    public Task<DeviceLocation?> GetCurrentLocationAsync()
    {
        // Simple mock
        return Task.FromResult<DeviceLocation?>(null);
    }

    public Task<bool> IsLocationEnabledAsync() => Task.FromResult(true);

    public Task<bool> RequestLocationPermissionAsync() => Task.FromResult(true);

    public double CalculateDistance(double lat1, double lon1, double lat2, double lon2) => 0;

    public async Task OpenMapsNavigationAsync(double destinationLat, double destinationLong, string destinationAddress = "")
    {
        var url = string.IsNullOrEmpty(destinationAddress) 
            ? $"https://www.google.com/maps/dir/?api=1&destination={destinationLat},{destinationLong}"
            : $"https://www.google.com/maps/dir/?api=1&destination={Uri.EscapeDataString(destinationAddress)}";
        
        await _js.InvokeVoidAsync("location.assign", url);
    }
}
