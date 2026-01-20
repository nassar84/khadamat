# 📱 Khadamat Mobile App

## Overview

Khadamat Mobile is a .NET MAUI Blazor Hybrid application that provides native mobile access to the Khadamat service marketplace platform. It reuses the existing Blazor UI components while adding native device capabilities.

## 🎯 Features

### ✅ Implemented Features

- **Native Device Integration**
  - 📸 Camera & Gallery access
  - 📍 GPS location services
  - 🔔 Push notifications
  - 📞 Phone calls & SMS
  - 💬 WhatsApp integration
  - 📤 Content sharing
  - 📁 File picking

- **User Experience**
  - 🌍 Full RTL Arabic support
  - 📱 Mobile-first responsive design
  - 🧭 Bottom navigation
  - 🎨 Glassmorphism theme
  - ⚡ Smooth animations
  - 💾 Offline support

- **Core Functionality**
  - Browse services
  - Search & filter
  - View service details
  - Contact providers
  - Manage favorites
  - User authentication
  - Profile management

## 🏗️ Architecture

```
Khadamat.MobileApp/
├── Components/
│   └── Routes.razor          # App routing
├── Platforms/
│   ├── Android/              # Android-specific code
│   ├── iOS/                  # iOS-specific code
│   └── Windows/              # Windows-specific code (optional)
├── Services/                 # Device service implementations
│   ├── CameraService.cs
│   ├── LocationService.cs
│   ├── NotificationService.cs
│   ├── PhoneService.cs
│   ├── ShareService.cs
│   └── FilePickerService.cs
├── Resources/                # App resources
│   ├── AppIcon/
│   ├── Splash/
│   ├── Fonts/
│   └── Images/
├── wwwroot/                  # Web assets
├── MauiProgram.cs            # App configuration
├── MainPage.xaml             # Main page
└── App.xaml                  # App definition
```

## 🚀 Getting Started

### Prerequisites

- .NET 9.0 SDK or later
- Visual Studio 2022 17.8+ or Visual Studio Code
- Android SDK (for Android development)
- Xcode (for iOS development, Mac only)

### Installation

1. **Clone the repository**
   ```bash
   cd e:\MVC\khadamat
   ```

2. **Restore packages**
   ```bash
   dotnet restore src/Khadamat.MobileApp/Khadamat.MobileApp.csproj
   ```

3. **Update API URL**
   
   Edit `MauiProgram.cs` and update the API base URL:
   ```csharp
   var apiBaseUrl = "https://your-api-url.com";
   ```

### Running the App

#### Android

```bash
# Build
dotnet build src/Khadamat.MobileApp/Khadamat.MobileApp.csproj -f net9.0-android

# Run on emulator
dotnet build src/Khadamat.MobileApp/Khadamat.MobileApp.csproj -t:Run -f net9.0-android

# Run on device
dotnet build src/Khadamat.MobileApp/Khadamat.MobileApp.csproj -t:Run -f net9.0-android -p:AndroidAttachDebugger=true
```

#### iOS (Mac only)

```bash
# Build
dotnet build src/Khadamat.MobileApp/Khadamat.MobileApp.csproj -f net9.0-ios

# Run on simulator
dotnet build src/Khadamat.MobileApp/Khadamat.MobileApp.csproj -t:Run -f net9.0-ios
```

## 📦 Dependencies

### NuGet Packages

- **Microsoft.Maui.Controls** - MAUI framework
- **Microsoft.AspNetCore.Components.WebView.Maui** - Blazor WebView
- **CommunityToolkit.Maui** - Community toolkit
- **Plugin.LocalNotification** - Local notifications
- **Blazored.LocalStorage** - Local storage

### Project References

- **Khadamat.BlazorUI** - Shared UI components
- **Khadamat.Application** - Application layer
- **Khadamat.Shared** - Device service interfaces

## 🔐 Permissions

### Android

The app requests the following permissions:

- Internet & Network access
- Camera
- Storage (Read/Write)
- Location (Fine/Coarse)
- Phone calls
- Notifications
- Vibration

### iOS

Configure in `Info.plist`:

```xml
<key>NSCameraUsageDescription</key>
<string>نحتاج للوصول إلى الكاميرا لالتقاط صور الخدمات</string>

<key>NSPhotoLibraryUsageDescription</key>
<string>نحتاج للوصول إلى معرض الصور لاختيار صور الخدمات</string>

<key>NSLocationWhenInUseUsageDescription</key>
<string>نحتاج لموقعك لعرض الخدمات القريبة منك</string>
```

## 🎨 Customization

### Theme Colors

Update theme colors in `wwwroot/css/app.css`:

```css
:root {
    --theme-primary: #3b82f6;
    --theme-secondary: #8b5cf6;
    --theme-glow: rgba(59, 130, 246, 0.3);
}
```

### App Icon & Splash

Replace files in:
- `Resources/AppIcon/appicon.svg`
- `Resources/Splash/splash.svg`

## 📱 Device Services Usage

### Camera

```csharp
@inject IDeviceCameraService CameraService

// Capture photo
var photo = await CameraService.CapturePhotoAsync();

// Pick from gallery
var photo = await CameraService.PickPhotoAsync();
```

### Location

```csharp
@inject ILocationService LocationService

// Get current location
var location = await LocationService.GetCurrentLocationAsync();

// Open maps
await LocationService.OpenMapsNavigationAsync(lat, lon, "Place Name");
```

### Phone

```csharp
@inject IPhoneService PhoneService

// Make call
await PhoneService.MakePhoneCallAsync("01012345678");

// Open WhatsApp
await PhoneService.OpenWhatsAppChatAsync("01012345678", "Hello!");
```

### Notifications

```csharp
@inject INotificationService NotificationService

// Show notification
await NotificationService.ShowNotificationAsync(new NotificationRequest
{
    Title = "عنوان",
    Message = "رسالة",
    NotificationId = 1
});
```

### Share

```csharp
@inject IShareService ShareService

// Share text
await ShareService.ShareTextAsync("Share this!", "Title");

// Share link
await ShareService.ShareLinkAsync("https://example.com", "Check this out");
```

## 🐛 Troubleshooting

### Common Issues

**App crashes on startup**
- Check MauiProgram.cs service registration
- Verify all project references are correct
- Clean and rebuild solution

**Camera not working**
- Verify permissions in AndroidManifest.xml / Info.plist
- Check device has camera
- Test on physical device (not emulator)

**API calls fail**
- Check HttpClient BaseAddress
- Verify API is running
- Check network connectivity
- For Android emulator, use `http://10.0.2.2:5000` for localhost

**RTL not working**
- Ensure `FlowDirection="RightToLeft"` in MainPage.xaml
- Check CSS direction properties

## 📊 Performance Tips

1. **Image Optimization**
   - Compress images before upload
   - Use appropriate image sizes
   - Implement lazy loading

2. **API Calls**
   - Implement caching
   - Use pagination
   - Minimize payload size

3. **UI Rendering**
   - Virtualize long lists
   - Avoid heavy animations
   - Use CSS transforms

## 🚢 Deployment

### Android

1. **Generate Signing Key**
   ```bash
   keytool -genkey -v -keystore khadamat.keystore -alias khadamat -keyalg RSA -keysize 2048 -validity 10000
   ```

2. **Configure Signing**
   
   Add to `.csproj`:
   ```xml
   <PropertyGroup Condition="'$(Configuration)' == 'Release'">
       <AndroidKeyStore>True</AndroidKeyStore>
       <AndroidSigningKeyStore>khadamat.keystore</AndroidSigningKeyStore>
       <AndroidSigningKeyAlias>khadamat</AndroidSigningKeyAlias>
   </PropertyGroup>
   ```

3. **Build Release**
   ```bash
   dotnet publish -f net9.0-android -c Release
   ```

4. **Upload to Google Play**
   - Create app in Google Play Console
   - Upload AAB file
   - Complete store listing
   - Submit for review

### iOS

1. **Configure Provisioning**
   - Create App ID in Apple Developer
   - Create provisioning profile
   - Configure in Xcode

2. **Build Archive**
   ```bash
   dotnet publish -f net9.0-ios -c Release
   ```

3. **Upload to App Store**
   - Use Xcode or Transporter
   - Complete App Store Connect listing
   - Submit for review

## 📝 License

This project is part of the Khadamat system.

## 🤝 Contributing

Contributions are welcome! Please follow the existing code style and patterns.

## 📞 Support

For issues or questions:
- Check the documentation
- Review existing issues
- Contact the development team

---

**Built with ❤️ using .NET MAUI Blazor Hybrid**
