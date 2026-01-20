using System.Threading.Tasks;

namespace Khadamat.Shared.Interfaces;

/// <summary>
/// Represents a geographic location
/// </summary>
public class DeviceLocation
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? Altitude { get; set; }
    public double? Accuracy { get; set; }
    public DateTime Timestamp { get; set; }
}

/// <summary>
/// Interface for device location services
/// </summary>
public interface ILocationService
{
    /// <summary>
    /// Get current device location
    /// </summary>
    Task<DeviceLocation?> GetCurrentLocationAsync();

    /// <summary>
    /// Check if location services are enabled
    /// </summary>
    Task<bool> IsLocationEnabledAsync();

    /// <summary>
    /// Request location permissions
    /// </summary>
    Task<bool> RequestLocationPermissionAsync();

    /// <summary>
    /// Calculate distance between two points in kilometers
    /// </summary>
    double CalculateDistance(double lat1, double lon1, double lat2, double lon2);

    /// <summary>
    /// Open maps app with navigation to specific location
    /// </summary>
    Task OpenMapsNavigationAsync(double latitude, double longitude, string placeName);
}
