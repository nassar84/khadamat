using Khadamat.Shared.Interfaces;
using Khadamat.MobileApp.Helpers;

namespace Khadamat.MobileApp.Services;

public class LocationService : ILocationService
{
    public async Task<DeviceLocation?> GetCurrentLocationAsync()
    {
        try
        {
            var hasPermission = await RequestLocationPermissionAsync();
            if (!hasPermission)
            {
                await KhadamatDialogHelper.ShowWarningAsync("تنبيه", "يرجى السماح بالوصول إلى الموقع الجغرافي لتقديم أفضل خدمة");
                return null;
            }

            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            var location = await Geolocation.Default.GetLocationAsync(request);

            if (location == null) return null;

            return new DeviceLocation
            {
                Latitude = location.Latitude,
                Longitude = location.Longitude,
                Altitude = location.Altitude,
                Accuracy = location.Accuracy,
                Timestamp = location.Timestamp.DateTime
            };
        }
        catch (FeatureNotSupportedException)
        {
            await KhadamatDialogHelper.ShowErrorAsync("خطأ", "خدمة الموقع غير مدعومة على هذا الجهاز");
            return null;
        }
        catch (PermissionException)
        {
            await KhadamatDialogHelper.ShowWarningAsync("تنبيه", "يرجى السماح بالوصول إلى الموقع");
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Location error: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> IsLocationEnabledAsync()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            return status == PermissionStatus.Granted;
        }
        catch
        {
            return false;
        }
    }

    public async Task<bool> RequestLocationPermissionAsync()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            
            if (status == PermissionStatus.Granted)
                return true;

            if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
            {
                // On iOS, once denied, user must enable in settings
                await KhadamatDialogHelper.ShowWarningAsync("إذن مطلوب", "يرجى تفعيل خدمة الموقع من إعدادات التطبيق");
                return false;
            }

            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            return status == PermissionStatus.Granted;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Permission error: {ex.Message}");
            return false;
        }
    }

    public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        var R = 6371; // Earth radius in kilometers
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private double ToRadians(double degrees) => degrees * Math.PI / 180;

    public async Task OpenMapsNavigationAsync(double latitude, double longitude, string placeName)
    {
        try
        {
            var location = new Location(latitude, longitude);
            var options = new MapLaunchOptions 
            { 
                Name = placeName,
                NavigationMode = NavigationMode.Driving
            };
            
            await Map.Default.OpenAsync(location, options);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Maps error: {ex.Message}");
            await KhadamatDialogHelper.ShowErrorAsync("خطأ", "تعذر فتح تطبيق الخرائط");
        }
    }
}
