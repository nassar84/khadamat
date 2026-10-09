# ==============================================================================
# Publish Script for Khadamat: Builds APK + WebAPI (Blazor WASM) + Copies APK
# Enforces strictly ONE single fresh APK with current timestamp
# ==============================================================================

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host " 1. Cleaning old APKs and preparing for fresh build..." -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

# Remove any old APK files first to guarantee fresh build and single APK output
Get-ChildItem -Path "src/Khadamat.MobileApp/bin/Release/net8.0-android" -Recurse -Filter "*.apk" -ErrorAction SilentlyContinue | Remove-Item -Force -ErrorAction SilentlyContinue
Get-ChildItem -Path "src/Khadamat.WebAPI/wwwroot/downloads" -Filter "*.apk" -ErrorAction SilentlyContinue | Remove-Item -Force -ErrorAction SilentlyContinue
if (Test-Path "D:\maged\Khadamat\wwwroot\downloads") {
    Get-ChildItem -Path "D:\maged\Khadamat\wwwroot\downloads" -Filter "*.apk" -ErrorAction SilentlyContinue | Remove-Item -Force -ErrorAction SilentlyContinue
}

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host " 2. Building Android APK (Release - Single APK)..." -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

dotnet publish src/Khadamat.MobileApp/Khadamat.MobileApp.csproj -f net8.0-android -c Release
if ($LASTEXITCODE -ne 0) {
    Write-Error "Android APK build failed!"
    exit $LASTEXITCODE
}

$apkSource = "src/Khadamat.MobileApp/bin/Release/net8.0-android/android-arm64/publish/com.nassar84.khadamat-Signed.apk"
if (-not (Test-Path $apkSource)) {
    $apkSource = "src/Khadamat.MobileApp/bin/Release/net8.0-android/android-arm64/com.nassar84.khadamat-Signed.apk"
}

if (-not (Test-Path $apkSource)) {
    Write-Error "Could not find generated APK at: $apkSource"
    exit 1
}

Write-Host "APK Built successfully: $apkSource" -ForegroundColor Green

# Current build timestamp
$now = Get-Date

Write-Host "`n==========================================================" -ForegroundColor Cyan
Write-Host " 3. Placing single APK in WebAPI downloads with fresh timestamp..." -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

New-Item -ItemType Directory -Force -Path "src/Khadamat.WebAPI/wwwroot/downloads" | Out-Null
# Ensure any other APK is removed so only ONE APK exists
Get-ChildItem -Path "src/Khadamat.WebAPI/wwwroot/downloads" -Filter "*.apk" -ErrorAction SilentlyContinue | Remove-Item -Force -ErrorAction SilentlyContinue

Copy-Item -Force $apkSource -Destination "src/Khadamat.WebAPI/wwwroot/downloads/khadamat.apk"
(Get-Item "src/Khadamat.WebAPI/wwwroot/downloads/khadamat.apk").CreationTime = $now
(Get-Item "src/Khadamat.WebAPI/wwwroot/downloads/khadamat.apk").LastWriteTime = $now

Write-Host "`n==========================================================" -ForegroundColor Cyan
Write-Host " 4. Publishing WebAPI & Blazor WASM to D:\maged\Khadamat (Self-Contained win-x64)..." -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

# Self-contained: bundles .NET 8 runtime so the host server does NOT need it pre-installed.
# This fixes HTTP Error 500.31 (Failed to load ASP.NET Core runtime).
dotnet publish src/Khadamat.WebAPI/Khadamat.WebAPI.csproj -c Release -r win-x64 --self-contained true -o D:\maged\Khadamat
if ($LASTEXITCODE -ne 0) {
    Write-Error "WebAPI publish failed!"
    exit $LASTEXITCODE
}

Write-Host "`n==========================================================" -ForegroundColor Cyan
Write-Host " 5. Finalizing publish directory (Strictly ONE APK with current timestamp)..." -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

New-Item -ItemType Directory -Force -Path "D:\maged\Khadamat\wwwroot\downloads" | Out-Null

# Purge any duplicate or extra APKs (e.g. com.nassar84.khadamat-Signed.apk, Khadamawy.apk)
Get-ChildItem -Path "D:\maged\Khadamat\wwwroot\downloads" -Filter "*.apk" -ErrorAction SilentlyContinue | Where-Object { $_.Name -ne "khadamat.apk" } | Remove-Item -Force -ErrorAction SilentlyContinue

Copy-Item -Force $apkSource -Destination "D:\maged\Khadamat\wwwroot\downloads\khadamat.apk"
(Get-Item "D:\maged\Khadamat\wwwroot\downloads\khadamat.apk").CreationTime = $now
(Get-Item "D:\maged\Khadamat\wwwroot\downloads\khadamat.apk").LastWriteTime = $now

# Also update root convenience copy
Copy-Item -Force $apkSource -Destination "khadamat.apk"
(Get-Item "khadamat.apk").CreationTime = $now
(Get-Item "khadamat.apk").LastWriteTime = $now

# Verify that strictly ONE APK exists in destination
$publishedApks = Get-ChildItem -Path "D:\maged\Khadamat\wwwroot\downloads" -Filter "*.apk"
if ($publishedApks.Count -ne 1) {
    Write-Error "Error: Expected exactly 1 APK file in publish directory, but found $($publishedApks.Count)!"
    exit 1
}

Write-Host "`n==========================================================" -ForegroundColor Green
Write-Host " PUBLISH COMPLETE! Exactly ONE APK published with current timestamp:" -ForegroundColor Green
Write-Host "==========================================================" -ForegroundColor Green

$publishedApks | Select-Object Name, Length, LastWriteTime, CreationTime | Format-Table -AutoSize
Get-Item "src/Khadamat.WebAPI/wwwroot/downloads/khadamat.apk" | Select-Object Name, Length, LastWriteTime, CreationTime | Format-Table -AutoSize
