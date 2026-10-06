# Khadamat Mobile App - Complete Implementation Guide

## 📱 Project Overview

This guide provides step-by-step instructions to integrate a .NET MAUI Blazor Hybrid mobile application with the existing Khadamat system.

---

## 🏗️ STEP 1: Convert BlazorUI to Razor Class Library

### 1.1 Modify Khadamat.BlazorUI.csproj

Change the SDK from `Microsoft.NET.Sdk.Web` to `Microsoft.NET.Sdk.Razor`:

```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">
  <PropertyGroup>
    <TargetFramework>net9.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <AddRazorSupportForMvc>true</AddRazorSupportForMvc>
  </PropertyGroup>

  <ItemGroup>
    <SupportedPlatform Include="browser" />
    <SupportedPlatform Include="android" />
    <SupportedPlatform Include="ios" />
  </ItemGroup>

  <!-- Keep existing package references -->
  <ItemGroup>
    <PackageReference Include="Blazored.LocalStorage" Version="4.5.0" />
    <PackageReference Include="Microsoft.AspNetCore.Components.Web" Version="9.0.0" />
    <!-- ... other packages ... -->
  </ItemGroup>

  <!-- Keep existing project references -->
  <ItemGroup>
    <ProjectReference Include="..\Khadamat.Application\Khadamat.Application.csproj" />
    <ProjectReference Include="..\Khadamat.Shared\Khadamat.Shared.csproj" />
  </ItemGroup>
</Project>
```

### 1.2 Create Web Host Project (Optional for Web)

If you want to keep the web version, create a new `Khadamat.Web` project:

```bash
dotnet new blazorwasm -n Khadamat.Web -o src/Khadamat.Web
```

---

## 🚀 STEP 2: Create MAUI Blazor Hybrid Project

### 2.1 Create Project

```bash
cd e:\MVC\khadamat\src
dotnet new maui-blazor -n Khadamat.MobileApp -o Khadamat.MobileApp
```

### 2.2 Project Structure

```
Khadamat.MobileApp/
├── Platforms/
│   ├── Android/
│   │   ├── MainActivity.cs
│   │   ├── MainApplication.cs
│   │   ├── AndroidManifest.xml
│   │   └── Resources/
│   ├── iOS/
│   │   ├── AppDelegate.cs
│   │   ├── Program.cs
│   │   └── Info.plist
│   └── Windows/
├── Resources/
│   ├── AppIcon/
│   ├── Splash/
│   ├── Fonts/
│   └── Images/
├── Services/
│   ├── CameraService.cs
│   ├── LocationService.cs
│   ├── NotificationService.cs
│   ├── PhoneService.cs
│   ├── ShareService.cs
│   └── FilePickerService.cs
├── wwwroot/
├── MauiProgram.cs
├── MainPage.xaml
├── MainPage.xaml.cs
└── App.xaml
```

### 2.3 Khadamat.MobileApp.csproj

```xml
<Project Sdk="Microsoft.NET.Sdk.Razor">
  <PropertyGroup>
    <TargetFrameworks>net9.0-android;net9.0-ios</TargetFrameworks>
    <!-- For Windows support, add: net9.0-windows10.0.19041.0 -->
    
    <OutputType>Exe</OutputType>
    <RootNamespace>Khadamat.MobileApp</RootNamespace>
    <UseMaui>true</UseMaui>
    <SingleProject>true</SingleProject>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>

    <!-- Display name -->
    <ApplicationTitle>خدمات</ApplicationTitle>

    <!-- App Identifier -->
    <ApplicationId>com.khadamat.app</ApplicationId>

    <!-- Versions -->
    <ApplicationDisplayVersion>1.0</ApplicationDisplayVersion>
    <ApplicationVersion>1</ApplicationVersion>

    <!-- Android -->
    <SupportedOSPlatformVersion Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'android'">24.0</SupportedOSPlatformVersion>
    
    <!-- iOS -->
    <SupportedOSPlatformVersion Condition="$([MSBuild]::GetTargetPlatformIdentifier('$(TargetFramework)')) == 'ios'">12.0</SupportedOSPlatformVersion>
  </PropertyGroup>

  <ItemGroup>
    <!-- MAUI Core -->
    <PackageReference Include="Microsoft.Maui.Controls" Version="8.0.90" />
    <PackageReference Include="Microsoft.Maui.Controls.Compatibility" Version="8.0.90" />
    <PackageReference Include="Microsoft.AspNetCore.Components.WebView.Maui" Version="8.0.90" />
    
    <!-- Community Toolkit -->
    <PackageReference Include="CommunityToolkit.Maui" Version="7.0.1" />
    <PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
    
    <!-- Media -->
    <PackageReference Include="Microsoft.Maui.Controls.MediaElement" Version="8.0.0" />
    
    <!-- Notifications -->
    <PackageReference Include="Plugin.LocalNotification" Version="11.1.3" />
    
    <!-- Storage -->
    <PackageReference Include="Blazored.LocalStorage" Version="4.5.0" />
  </ItemGroup>

  <ItemGroup>
    <!-- Project References -->
    <ProjectReference Include="..\Khadamat.BlazorUI\Khadamat.BlazorUI.csproj" />
    <ProjectReference Include="..\Khadamat.Application\Khadamat.Application.csproj" />
    <ProjectReference Include="..\Khadamat.Shared\Khadamat.Shared.csproj" />
  </ItemGroup>
</Project>
```

---

## 🔧 STEP 3: Configure MauiProgram.cs

```csharp
using CommunityToolkit.Maui;
using Khadamat.BlazorUI.Services;
using Khadamat.BlazorUI.Services.Auth;
using Khadamat.BlazorUI.State;
using Khadamat.MobileApp.Services;
using Khadamat.Shared.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;

namespace Khadamat.MobileApp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("Cairo-Regular.ttf", "CairoRegular");
                fonts.AddFont("Cairo-Bold.ttf", "CairoBold");
            });

        // Add Blazor WebView
        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif

        // Configure HttpClient for API
        var apiBaseUrl = "https://your-api-url.com"; // Change to your API URL
        
        builder.Services.AddScoped(sp => new HttpClient
        {
            BaseAddress = new Uri(apiBaseUrl)
        });

        // Register Blazor UI Services
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<ApiClient>();
        builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
        builder.Services.AddScoped<AppState>();

        // Register Device Services
        builder.Services.AddSingleton<IDeviceCameraService, CameraService>();
        builder.Services.AddSingleton<ILocationService, LocationService>();
        builder.Services.AddSingleton<INotificationService, NotificationService>();
        builder.Services.AddSingleton<IPhoneService, PhoneService>();
        builder.Services.AddSingleton<IShareService, ShareService>();
        builder.Services.AddSingleton<IFilePickerService, FilePickerService>();

        // Blazored LocalStorage
        builder.Services.AddBlazoredLocalStorage();

        return builder.Build();
    }
}
```

---

## 📄 STEP 4: Configure MainPage.xaml

```xml
<?xml version="1.0" encoding="utf-8" ?>
<ContentPage xmlns="http://schemas.microsoft.com/dotnet/2021/maui"
             xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
             xmlns:local="clr-namespace:Khadamat.MobileApp"
             x:Class="Khadamat.MobileApp.MainPage"
             BackgroundColor="{DynamicResource PageBackgroundColor}"
             FlowDirection="RightToLeft">

    <BlazorWebView x:Name="blazorWebView" HostPage="wwwroot/index.html">
        <BlazorWebView.RootComponents>
            <RootComponent Selector="#app" ComponentType="{x:Type local:Main}" />
        </BlazorWebView.RootComponents>
    </BlazorWebView>

</ContentPage>
```

### MainPage.xaml.cs

```csharp
namespace Khadamat.MobileApp;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }
}
```

---

## 🎨 STEP 5: Create Main.razor Component

Create `Main.razor` in the MobileApp project:

```razor
@using Microsoft.AspNetCore.Components.Web
@using Khadamat.BlazorUI.Layout
@using Khadamat.BlazorUI.State

<Router AppAssembly="@typeof(Khadamat.BlazorUI.Pages.Index).Assembly">
    <Found Context="routeData">
        <RouteView RouteData="@routeData" DefaultLayout="@typeof(MobileLayout)" />
        <FocusOnNavigate RouteData="@routeData" Selector="h1" />
    </Found>
    <NotFound>
        <PageTitle>غير موجود</PageTitle>
        <LayoutView Layout="@typeof(MobileLayout)">
            <div class="not-found-container">
                <h1>404</h1>
                <p>عذراً، الصفحة المطلوبة غير موجودة</p>
                <a href="/" class="btn btn-primary">العودة للرئيسية</a>
            </div>
        </LayoutView>
    </NotFound>
</Router>
```

---

## 📱 STEP 6: Create MobileLayout.razor

Create `MobileLayout.razor` in BlazorUI/Layout:

```razor
@inherits LayoutComponentBase
@inject Khadamat.BlazorUI.State.AppState AppState

<div class="mobile-app">
    <main class="mobile-content">
        @Body
    </main>
    
    <MobileBottomNav />
</div>

<style>
    .mobile-app {
        display: flex;
        flex-direction: column;
        height: 100vh;
        width: 100vw;
        overflow: hidden;
    }

    .mobile-content {
        flex: 1;
        overflow-y: auto;
        overflow-x: hidden;
        -webkit-overflow-scrolling: touch;
    }
</style>
```

---

## 🧭 STEP 7: Create MobileBottomNav.razor

```razor
@inject NavigationManager Navigation
@inject AppState State

<nav class="bottom-nav">
    <button class="nav-item @(IsActive("/") ? "active" : "")" @onclick='() => Navigate("/")'>
        <i class="fa-solid fa-house"></i>
        <span>الرئيسية</span>
    </button>
    
    <button class="nav-item @(IsActive("/categories") ? "active" : "")" @onclick='() => Navigate("/categories")'>
        <i class="fa-solid fa-th-large"></i>
        <span>الأقسام</span>
    </button>
    
    <button class="nav-item @(IsActive("/favorites") ? "active" : "")" @onclick='() => Navigate("/favorites")'>
        <i class="fa-solid fa-heart"></i>
        <span>المفضلة</span>
    </button>
    
    @if (State.IsAuthenticated)
    {
        <button class="nav-item @(IsActive("/profile") ? "active" : "")" @onclick='() => Navigate("/profile")'>
            <i class="fa-solid fa-user"></i>
            <span>حسابي</span>
        </button>
    }
    else
    {
        <button class="nav-item" @onclick='() => Navigate("/login")'>
            <i class="fa-solid fa-sign-in-alt"></i>
            <span>دخول</span>
        </button>
    }
</nav>

<style>
    .bottom-nav {
        display: flex;
        justify-content: space-around;
        align-items: center;
        background: white;
        border-top: 1px solid rgba(0,0,0,0.1);
        padding: 8px 0;
        box-shadow: 0 -2px 10px rgba(0,0,0,0.05);
        position: sticky;
        bottom: 0;
        z-index: 1000;
    }

    .nav-item {
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: 4px;
        background: none;
        border: none;
        color: #64748b;
        font-size: 0.75rem;
        padding: 8px 12px;
        cursor: pointer;
        transition: all 0.3s;
    }

    .nav-item i {
        font-size: 1.25rem;
    }

    .nav-item.active {
        color: var(--theme-primary);
    }

    .nav-item:active {
        transform: scale(0.95);
    }
</style>

@code {
    private bool IsActive(string path)
    {
        return Navigation.Uri.EndsWith(path);
    }

    private void Navigate(string path)
    {
        Navigation.NavigateTo(path);
    }
}
```

---

## 📸 STEP 8: Implement Device Services

### CameraService.cs

```csharp
using Khadamat.Shared.Interfaces;

namespace Khadamat.MobileApp.Services;

public class CameraService : IDeviceCameraService
{
    public async Task<byte[]?> CapturePhotoAsync()
    {
        try
        {
            var photo = await MediaPicker.CapturePhotoAsync();
            if (photo == null) return null;

            using var stream = await photo.OpenReadAsync();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Camera error: {ex.Message}");
            return null;
        }
    }

    public async Task<byte[]?> PickPhotoAsync()
    {
        try
        {
            var photo = await MediaPicker.PickPhotoAsync();
            if (photo == null) return null;

            using var stream = await photo.OpenReadAsync();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Photo picker error: {ex.Message}");
            return null;
        }
    }

    public async Task<List<byte[]>> PickMultiplePhotosAsync(int maxCount = 5)
    {
        var results = new List<byte[]>();
        
        try
        {
            // Note: MAUI doesn't support multiple photo selection natively
            // You may need to use platform-specific code or a plugin
            for (int i = 0; i < maxCount; i++)
            {
                var photo = await PickPhotoAsync();
                if (photo != null)
                    results.Add(photo);
                else
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Multiple photo picker error: {ex.Message}");
        }

        return results;
    }

    public async Task<byte[]> CompressImageAsync(byte[] imageData, int quality = 80)
    {
        // Implement image compression logic
        // You may use ImageSharp or SkiaSharp for this
        await Task.CompletedTask;
        return imageData; // Placeholder
    }

    public async Task<bool> IsCameraAvailableAsync()
    {
        return await Task.FromResult(MediaPicker.IsCaptureSupported);
    }
}
```

### LocationService.cs

```csharp
using Khadamat.Shared.Interfaces;

namespace Khadamat.MobileApp.Services;

public class LocationService : ILocationService
{
    public async Task<DeviceLocation?> GetCurrentLocationAsync()
    {
        try
        {
            var location = await Geolocation.GetLocationAsync(new GeolocationRequest
            {
                DesiredAccuracy = GeolocationAccuracy.Medium,
                Timeout = TimeSpan.FromSeconds(10)
            });

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
        var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        return status == PermissionStatus.Granted;
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
        var location = new Location(latitude, longitude);
        var options = new MapLaunchOptions { Name = placeName };
        
        await Map.OpenAsync(location, options);
    }
}
```

---

## 🔔 STEP 9: Android Configuration

### AndroidManifest.xml

```xml
<?xml version="1.0" encoding="utf-8"?>
<manifest xmlns:android="http://schemas.android.com/apk/res/android">
    <application android:allowBackup="true" 
                 android:icon="@mipmap/appicon" 
                 android:roundIcon="@mipmap/appicon_round" 
                 android:supportsRtl="true">
    </application>
    
    <!-- Permissions -->
    <uses-permission android:name="android.permission.INTERNET" />
    <uses-permission android:name="android.permission.ACCESS_NETWORK_STATE" />
    <uses-permission android:name="android.permission.CAMERA" />
    <uses-permission android:name="android.permission.READ_EXTERNAL_STORAGE" />
    <uses-permission android:name="android.permission.WRITE_EXTERNAL_STORAGE" />
    <uses-permission android:name="android.permission.ACCESS_FINE_LOCATION" />
    <uses-permission android:name="android.permission.ACCESS_COARSE_LOCATION" />
    <uses-permission android:name="android.permission.CALL_PHONE" />
    <uses-permission android:name="android.permission.POST_NOTIFICATIONS" />
    
    <uses-feature android:name="android.hardware.camera" android:required="false" />
    <uses-feature android:name="android.hardware.location" android:required="false" />
</manifest>
```

---

## 📝 STEP 10: Build and Run

### Build Commands

```bash
# Clean
dotnet clean

# Restore
dotnet restore

# Build for Android
dotnet build -f net9.0-android

# Run on Android Emulator
dotnet build -t:Run -f net9.0-android

# Build for iOS (Mac only)
dotnet build -f net9.0-ios

# Run on iOS Simulator (Mac only)
dotnet build -t:Run -f net9.0-ios
```

---

## ✅ Testing Checklist

- [ ] App launches successfully
- [ ] Navigation works
- [ ] Camera captures photos
- [ ] Gallery picker works
- [ ] Location services functional
- [ ] Phone calls work
- [ ] WhatsApp integration works
- [ ] Notifications display
- [ ] Share functionality works
- [ ] API calls successful
- [ ] Authentication works
- [ ] Offline mode functional
- [ ] RTL layout correct
- [ ] Arabic fonts render properly

---

## 🚀 Deployment

### Android

1. Generate signing key
2. Configure signing in project properties
3. Build release APK/AAB
4. Upload to Google Play Console

### iOS

1. Configure provisioning profiles
2. Set up App Store Connect
3. Build archive
4. Upload via Xcode or Transporter

---

## 📚 Additional Resources

- [.NET MAUI Documentation](https://learn.microsoft.com/en-us/dotnet/maui/)
- [Blazor Hybrid](https://learn.microsoft.com/en-us/aspnet/core/blazor/hybrid/)
- [Community Toolkit](https://learn.microsoft.com/en-us/dotnet/communitytoolkit/maui/)

---

## 🆘 Troubleshooting

### Common Issues

**Issue**: App crashes on startup
- **Solution**: Check MauiProgram.cs service registration

**Issue**: Camera not working
- **Solution**: Verify permissions in AndroidManifest.xml / Info.plist

**Issue**: API calls fail
- **Solution**: Check HttpClient BaseAddress and CORS settings

**Issue**: RTL not working
- **Solution**: Set FlowDirection="RightToLeft" in MainPage.xaml

---

## 📞 Support

For issues or questions, please refer to the project documentation or contact the development team.
