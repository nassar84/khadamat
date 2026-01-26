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

    public Task<(double Latitude, double Longitude)> GetCurrentLocationAsync()
    {
        // Simple mock or return (0,0)
        return Task.FromResult((0.0, 0.0));
    }

    public async Task OpenMapsNavigationAsync(double destinationLat, double destinationLong, string destinationAddress = "")
    {
        var url = string.IsNullOrEmpty(destinationAddress) 
            ? $"https://www.google.com/maps/dir/?api=1&destination={destinationLat},{destinationLong}"
            : $"https://www.google.com/maps/dir/?api=1&destination={Uri.EscapeDataString(destinationAddress)}";
        
        await _js.InvokeVoidAsync("location.assign", url);
    }
}
