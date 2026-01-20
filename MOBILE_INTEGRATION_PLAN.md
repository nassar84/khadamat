# Khadamat Mobile Integration Plan

## 🎯 Overview
Integrate .NET MAUI Blazor Hybrid mobile app with existing Khadamat system while preserving all backend and UI logic.

## 📁 Project Structure

```
Khadamat/
├── src/
│   ├── Khadamat.Domain/              (Unchanged)
│   ├── Khadamat.Application/         (Unchanged)
│   ├── Khadamat.Infrastructure/      (Unchanged)
│   ├── Khadamat.WebAPI/              (Unchanged)
│   ├── Khadamat.BlazorUI/            (Convert to RCL)
│   ├── Khadamat.Shared/              (NEW - Device Interfaces)
│   └── Khadamat.MobileApp/           (NEW - MAUI Blazor Hybrid)
│       ├── Platforms/
│       │   ├── Android/
│       │   ├── iOS/
│       │   └── Windows/
│       ├── Services/
│       │   ├── CameraService.cs
│       │   ├── LocationService.cs
│       │   ├── NotificationService.cs
│       │   ├── PhoneService.cs
│       │   ├── ShareService.cs
│       │   └── FilePickerService.cs
│       ├── MauiProgram.cs
│       ├── MainPage.xaml
│       └── App.xaml
```

## 🔧 Implementation Steps

### Phase 1: Project Setup
1. ✅ Convert Khadamat.BlazorUI to Razor Class Library
2. ✅ Create Khadamat.Shared for device interfaces
3. ✅ Create Khadamat.MobileApp MAUI project
4. ✅ Configure project references

### Phase 2: Device Services
1. ✅ Define device service interfaces
2. ✅ Implement platform-specific services
3. ✅ Register services in DI container

### Phase 3: Mobile Features
1. ✅ Camera & Gallery integration
2. ✅ GPS & Location services
3. ✅ Push notifications
4. ✅ Phone integration (Call, WhatsApp)
5. ✅ Offline caching

### Phase 4: UI Adaptation
1. ✅ Mobile-first responsive design
2. ✅ Bottom navigation
3. ✅ RTL Arabic support
4. ✅ 3D card effects
5. ✅ Smooth transitions

### Phase 5: Testing & Deployment
1. ⏳ Android testing
2. ⏳ iOS testing
3. ⏳ Performance optimization
4. ⏳ App store preparation

## 🔐 Security Considerations

- Secure token storage using SecureStorage
- Permission handling (Camera, Location, Notifications)
- Data encryption for offline cache
- SSL pinning for API calls

## 📱 Target Platforms

- Android 7.0+ (API 24+)
- iOS 12.0+
- Windows 10 (optional)

## 🚀 Key Features

### Client Features
- Browse services with GPS-based filtering
- Capture and upload service photos
- Real-time notifications
- Offline favorites
- Direct call/WhatsApp integration
- Share service links

### Provider Features
- Multi-image service creation
- Camera integration
- Dashboard statistics
- Subscription management
- Push notifications for ratings

### Admin Features
- Web-only (existing system)
- Approval workflows
- Ad management
- Reports

## 📊 API Integration

All mobile features use existing WebAPI endpoints:
- `/api/v1/auth/*` - Authentication
- `/api/v1/services/*` - Service management
- `/api/v1/categories/*` - Categories
- `/api/v1/locations/*` - Governorates & Cities
- `/api/v1/admin/*` - Admin operations

## 🎨 Design System

- **Primary Color**: var(--theme-primary)
- **Secondary Color**: var(--theme-secondary)
- **RTL Support**: Full Arabic RTL
- **Typography**: Arabic-optimized fonts
- **Components**: Shared with BlazorUI

## 📦 NuGet Packages Required

```xml
<!-- MAUI Blazor -->
<PackageReference Include="Microsoft.Maui.Controls" Version="8.0.0" />
<PackageReference Include="Microsoft.Maui.Controls.Compatibility" Version="8.0.0" />
<PackageReference Include="Microsoft.AspNetCore.Components.WebView.Maui" Version="8.0.0" />

<!-- Device Features -->
<PackageReference Include="CommunityToolkit.Maui" Version="7.0.0" />
<PackageReference Include="Plugin.LocalNotification" Version="11.0.0" />

<!-- Shared -->
<PackageReference Include="Blazored.LocalStorage" Version="4.5.0" />
```

## 🔄 Migration Path

1. **Week 1**: Project setup and infrastructure
2. **Week 2**: Device services implementation
3. **Week 3**: UI adaptation and testing
4. **Week 4**: Polish, optimization, and deployment

## ✅ Success Criteria

- [ ] Mobile app runs on Android/iOS
- [ ] All existing features work
- [ ] Camera integration functional
- [ ] GPS location working
- [ ] Push notifications delivered
- [ ] Offline mode operational
- [ ] Performance meets targets (< 3s load time)
- [ ] No backend changes required
