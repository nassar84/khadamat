# 🚀 Khadamat Mobile - Quick Start Guide

## ⚡ Fast Track Setup (5 Minutes)

### Step 1: Verify Prerequisites ✅

```bash
# Check .NET version (should be 9.0+)
dotnet --version

# Check MAUI workload
dotnet workload list
```

If MAUI is not installed:
```bash
dotnet workload install maui
```

### Step 2: Update API Configuration 🔧

Edit `src/Khadamat.MobileApp/MauiProgram.cs`:

```csharp
// Line 35-38: Update API URL
var apiBaseUrl = DeviceInfo.Platform == DevicePlatform.Android 
    ? "http://10.0.2.2:5000"  // For Android emulator
    : "http://localhost:5000"; // For iOS simulator
```

**Production**: Replace with your actual API URL (e.g., `https://api.khadamat.com`)

### Step 3: Build the Project 🔨

```bash
cd e:\MVC\khadamat

# Restore all packages
dotnet restore

# Build mobile app
dotnet build src/Khadamat.MobileApp/Khadamat.MobileApp.csproj
```

### Step 4: Run on Android 📱

```bash
# Make sure Android emulator is running or device is connected
dotnet build src/Khadamat.MobileApp/Khadamat.MobileApp.csproj -t:Run -f net9.0-android
```

**First run may take 5-10 minutes** as it downloads Android dependencies.

### Step 5: Test Device Features 🎯

Once the app launches:

1. **Test Camera**: Navigate to service creation
2. **Test Location**: Check nearby services
3. **Test Phone**: Try calling a provider
4. **Test WhatsApp**: Send message to provider
5. **Test Share**: Share a service

---

## 🎨 Customization Quick Tips

### Change App Name

Edit `Khadamat.MobileApp.csproj`:
```xml
<ApplicationTitle>Your App Name</ApplicationTitle>
```

### Change App Icon

Replace:
- `Resources/AppIcon/appicon.svg`
- `Resources/Splash/splash.svg`

### Change Theme Colors

Edit `wwwroot/css/app.css`:
```css
:root {
    --theme-primary: #your-color;
    --theme-secondary: #your-color;
}
```

---

## 🐛 Quick Troubleshooting

### Build Errors

```bash
# Clean everything
dotnet clean

# Remove bin/obj folders
rm -rf src/*/bin src/*/obj

# Restore and rebuild
dotnet restore
dotnet build
```

### Android Emulator Issues

1. Open Android Studio
2. Tools → AVD Manager
3. Create new device (Pixel 5, API 33+)
4. Start emulator
5. Run app again

### API Connection Issues

**Android Emulator:**
- Use `http://10.0.2.2:5000` (NOT `localhost`)
- Ensure WebAPI is running
- Check firewall settings

**iOS Simulator:**
- Use `http://localhost:5000`
- Ensure WebAPI is running

---

## 📱 Testing on Physical Device

### Android

1. **Enable Developer Mode**
   - Settings → About Phone
   - Tap "Build Number" 7 times

2. **Enable USB Debugging**
   - Settings → Developer Options
   - Enable "USB Debugging"

3. **Connect Device**
   ```bash
   # Check device is connected
   adb devices
   
   # Run app
   dotnet build -t:Run -f net9.0-android
   ```

### iOS (Mac Only)

1. **Register Device**
   - Connect iPhone/iPad
   - Open Xcode
   - Window → Devices and Simulators
   - Register device

2. **Configure Provisioning**
   - Apple Developer Account required
   - Create App ID
   - Create provisioning profile

3. **Run App**
   ```bash
   dotnet build -t:Run -f net9.0-ios
   ```

---

## 🎯 Next Steps

1. ✅ **Test all features** on emulator
2. ✅ **Test on physical device**
3. ✅ **Customize branding** (icon, colors, name)
4. ✅ **Configure production API** URL
5. ✅ **Test with real data**
6. ✅ **Prepare for deployment**

---

## 📚 Useful Commands

```bash
# List available devices
dotnet build -t:ListDevices

# Clean build
dotnet clean && dotnet build

# Build release
dotnet build -c Release

# Publish Android AAB
dotnet publish -f net9.0-android -c Release

# View logs (Android)
adb logcat

# View logs (iOS)
xcrun simctl spawn booted log stream --predicate 'process == "Khadamat"'
```

---

## 🆘 Getting Help

1. Check `MOBILE_IMPLEMENTATION_GUIDE.md` for detailed instructions
2. Review `README.md` in MobileApp folder
3. Check MAUI documentation: https://learn.microsoft.com/dotnet/maui/
4. Contact development team

---

## ✨ Features Checklist

- [x] Camera integration
- [x] GPS location
- [x] Push notifications
- [x] Phone calls
- [x] WhatsApp integration
- [x] Content sharing
- [x] File picking
- [x] RTL Arabic support
- [x] Bottom navigation
- [x] Offline support
- [x] Authentication
- [x] Service browsing
- [x] Provider contact

---

**🎉 You're all set! Happy coding!**

For detailed documentation, see:
- `MOBILE_IMPLEMENTATION_GUIDE.md` - Complete implementation guide
- `MOBILE_INTEGRATION_PLAN.md` - Architecture and planning
- `src/Khadamat.MobileApp/README.md` - Mobile app specific docs
