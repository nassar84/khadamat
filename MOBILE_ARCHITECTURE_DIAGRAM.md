# 🏗️ Khadamat Mobile Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                         USER DEVICES                                 │
│  📱 Android (API 24+)    📱 iOS (12.0+)    💻 Windows (Optional)    │
└─────────────────────────────────────────────────────────────────────┘
                                  │
                                  ▼
┌─────────────────────────────────────────────────────────────────────┐
│                    KHADAMAT.MOBILEAPP                                │
│                   (.NET MAUI Blazor Hybrid)                          │
│                                                                       │
│  ┌────────────────────────────────────────────────────────────┐    │
│  │              BLAZOR WEBVIEW                                 │    │
│  │  ┌──────────────────────────────────────────────────┐      │    │
│  │  │         KHADAMAT.BLAZORUI (RCL)                  │      │    │
│  │  │                                                   │      │    │
│  │  │  • Pages (Shared)                                │      │    │
│  │  │  • Components (Shared)                           │      │    │
│  │  │  • Layouts (Web + Mobile)                        │      │    │
│  │  │  • Services (UI Logic)                           │      │    │
│  │  └──────────────────────────────────────────────────┘      │    │
│  └────────────────────────────────────────────────────────────┘    │
│                                                                       │
│  ┌────────────────────────────────────────────────────────────┐    │
│  │              DEVICE SERVICES                                │    │
│  │                                                              │    │
│  │  📸 CameraService      📍 LocationService                   │    │
│  │  🔔 NotificationService 📞 PhoneService                     │    │
│  │  📤 ShareService       📁 FilePickerService                 │    │
│  └────────────────────────────────────────────────────────────┘    │
│                                                                       │
│  ┌────────────────────────────────────────────────────────────┐    │
│  │              PLATFORM APIS                                   │    │
│  │                                                              │    │
│  │  Android APIs    iOS APIs    Windows APIs                   │    │
│  └────────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────────┘
                                  │
                                  │ HTTPS/REST
                                  ▼
┌─────────────────────────────────────────────────────────────────────┐
│                      KHADAMAT.WEBAPI                                 │
│                    (ASP.NET Core Web API)                            │
│                                                                       │
│  • Authentication (JWT)                                              │
│  • Service Management                                                │
│  • User Management                                                   │
│  • Category Management                                               │
│  • Location Services                                                 │
└─────────────────────────────────────────────────────────────────────┘
                                  │
                                  ▼
┌─────────────────────────────────────────────────────────────────────┐
│                   KHADAMAT.INFRASTRUCTURE                            │
│                                                                       │
│  • Entity Framework Core                                             │
│  • Identity Management                                               │
│  • Data Persistence                                                  │
└─────────────────────────────────────────────────────────────────────┘
                                  │
                                  ▼
┌─────────────────────────────────────────────────────────────────────┐
│                   KHADAMAT.APPLICATION                               │
│                                                                       │
│  • Business Logic                                                    │
│  • DTOs                                                              │
│  • Interfaces                                                        │
│  • Validators                                                        │
└─────────────────────────────────────────────────────────────────────┘
                                  │
                                  ▼
┌─────────────────────────────────────────────────────────────────────┐
│                     KHADAMAT.DOMAIN                                  │
│                                                                       │
│  • Entities                                                          │
│  • Enums                                                             │
│  • Value Objects                                                     │
└─────────────────────────────────────────────────────────────────────┘
                                  │
                                  ▼
┌─────────────────────────────────────────────────────────────────────┐
│                      SQL SERVER DATABASE                             │
│                                                                       │
│  • Users & Authentication                                            │
│  • Services & Categories                                             │
│  • Locations & Cities                                                │
│  • Ratings & Reviews                                                 │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 🔄 Data Flow

### 1. User Interaction Flow
```
User Tap → Blazor Component → Device Service Interface → 
Platform Implementation → Native API → Result → UI Update
```

### 2. API Call Flow
```
Blazor Page → HttpClient → WebAPI Endpoint → 
Application Layer → Infrastructure → Database → 
Response → DTO → Blazor Page → UI Render
```

### 3. Camera Capture Flow
```
User Taps Camera → CameraService.CapturePhotoAsync() → 
MediaPicker.CapturePhotoAsync() → Native Camera → 
Photo Data → Convert to byte[] → Return to Blazor → 
Upload to API → Save to Database
```

### 4. Location Flow
```
User Requests Location → LocationService.GetCurrentLocationAsync() → 
Request Permission → Geolocation.GetLocationAsync() → 
GPS Hardware → Coordinates → Return to Blazor → 
Display on Map / Filter Services
```

---

## 🎯 Key Design Principles

### 1. **Separation of Concerns**
- Domain: Business entities
- Application: Business logic
- Infrastructure: Data access
- WebAPI: HTTP endpoints
- BlazorUI: Presentation
- MobileApp: Platform integration

### 2. **Dependency Injection**
- All services registered in MauiProgram.cs
- Interface-based design
- Easy to test and mock

### 3. **Shared UI**
- Single Blazor UI codebase
- Works on web and mobile
- Conditional rendering for platform-specific features

### 4. **Platform Abstraction**
- Interfaces in Khadamat.Shared
- Implementations in Khadamat.MobileApp
- Web stubs in Khadamat.BlazorUI (optional)

---

## 📱 Mobile-Specific Components

```
MobileApp/
├── Services/              ← Platform implementations
│   ├── CameraService
│   ├── LocationService
│   ├── NotificationService
│   ├── PhoneService
│   ├── ShareService
│   └── FilePickerService
│
├── Platforms/             ← Platform-specific code
│   ├── Android/
│   │   ├── MainActivity
│   │   ├── MainApplication
│   │   └── AndroidManifest.xml
│   │
│   └── iOS/
│       ├── AppDelegate
│       ├── Program
│       └── Info.plist
│
└── Components/            ← Mobile routing
    └── Routes.razor
```

---

## 🔐 Security Layers

```
┌─────────────────────────────────────┐
│  Device Security                     │
│  • Biometric Auth (Future)          │
│  • Secure Storage                   │
│  • Permission System                │
└─────────────────────────────────────┘
              │
              ▼
┌─────────────────────────────────────┐
│  Transport Security                  │
│  • HTTPS/TLS                        │
│  • Certificate Pinning (Future)     │
└─────────────────────────────────────┘
              │
              ▼
┌─────────────────────────────────────┐
│  Application Security                │
│  • JWT Authentication               │
│  • Token Refresh                    │
│  • Role-based Access                │
└─────────────────────────────────────┘
              │
              ▼
┌─────────────────────────────────────┐
│  Data Security                       │
│  • Encrypted Storage                │
│  • SQL Injection Prevention         │
│  • Input Validation                 │
└─────────────────────────────────────┘
```

---

## 🚀 Deployment Pipeline

```
Development
    │
    ├─► Build & Test
    │       │
    │       ├─► Unit Tests
    │       ├─► Integration Tests
    │       └─► UI Tests
    │
    ├─► Code Review
    │
    └─► Merge to Main
            │
            ├─► Android Build
            │       │
            │       ├─► Generate AAB
            │       ├─► Sign with Key
            │       └─► Upload to Play Console
            │
            └─► iOS Build
                    │
                    ├─► Archive
                    ├─► Sign with Certificate
                    └─► Upload to App Store Connect
```

---

## 📊 Performance Optimization

### 1. **Lazy Loading**
```csharp
@code {
    private List<ServiceDto> services;
    
    protected override async Task OnInitializedAsync()
    {
        // Load only first page
        services = await Api.GetServicesAsync(page: 1, pageSize: 20);
    }
    
    private async Task LoadMore()
    {
        // Load more on scroll
        var moreServices = await Api.GetServicesAsync(page: ++currentPage);
        services.AddRange(moreServices);
    }
}
```

### 2. **Image Optimization**
```csharp
// Compress before upload
var compressed = await CameraService.CompressImageAsync(photo, quality: 80);
```

### 3. **Caching**
```csharp
// Cache frequently accessed data
await LocalStorage.SetItemAsync("favorites", favorites);
var cached = await LocalStorage.GetItemAsync<List<int>>("favorites");
```

---

## 🎨 UI/UX Patterns

### 1. **Bottom Navigation**
- Always visible
- 4-5 main sections
- Active state indication
- Smooth transitions

### 2. **Pull to Refresh**
```csharp
<RefreshView IsRefreshing="@isRefreshing" OnRefresh="RefreshData">
    <!-- Content -->
</RefreshView>
```

### 3. **Loading States**
```csharp
@if (isLoading)
{
    <LoadingSpinner />
}
else if (data == null)
{
    <EmptyState />
}
else
{
    <DataView Data="@data" />
}
```

---

This architecture ensures:
- ✅ Clean separation of concerns
- ✅ Testability
- ✅ Maintainability
- ✅ Scalability
- ✅ Platform independence
- ✅ Code reusability
