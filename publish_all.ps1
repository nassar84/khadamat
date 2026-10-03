# ==============================================================================
# Publish Script for Khadamat: Builds APK + WebAPI (Blazor WASM) + Copies APK
# ==============================================================================

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host " 1. Building Android APK (Release)..." -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

# Remove any old APK files first to guarantee fresh build
Get-ChildItem -Path "src/Khadamat.MobileApp/bin/Release/net8.0-android" -Recurse -Filter "*.apk" -ErrorAction SilentlyContinue | Remove-Item -Force -ErrorAction SilentlyContinue

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

Write-Host "`n==========================================================" -ForegroundColor Cyan
Write-Host " 2. Publishing WebAPI & Blazor WASM to D:\maged\Khadamat..." -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

dotnet publish src/Khadamat.WebAPI/Khadamat.WebAPI.csproj -c Release -o D:\maged\Khadamat
if ($LASTEXITCODE -ne 0) {
    Write-Error "WebAPI publish failed!"
    exit $LASTEXITCODE
}

Write-Host "`n==========================================================" -ForegroundColor Cyan
Write-Host " 3. Copying APK to downloads..." -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

New-Item -ItemType Directory -Force -Path "D:\maged\Khadamat\wwwroot\downloads" | Out-Null
Copy-Item -Force $apkSource -Destination "D:\maged\Khadamat\wwwroot\downloads\khadamat.apk"
(Get-Item "D:\maged\Khadamat\wwwroot\downloads\khadamat.apk").LastWriteTime = (Get-Date)
Copy-Item -Force $apkSource -Destination "D:\maged\Khadamat\wwwroot\downloads\com.nassar84.khadamat-Signed.apk"
(Get-Item "D:\maged\Khadamat\wwwroot\downloads\com.nassar84.khadamat-Signed.apk").LastWriteTime = (Get-Date)

New-Item -ItemType Directory -Force -Path "src/Khadamat.WebAPI/wwwroot/downloads" | Out-Null
Copy-Item -Force $apkSource -Destination "src/Khadamat.WebAPI/wwwroot/downloads\khadamat.apk"
(Get-Item "src/Khadamat.WebAPI/wwwroot/downloads\khadamat.apk").LastWriteTime = (Get-Date)

Write-Host "`n==========================================================" -ForegroundColor Green
Write-Host " PUBLISH COMPLETE! VERIFYING APK TIMESTAMP:" -ForegroundColor Green
Write-Host "==========================================================" -ForegroundColor Green

Get-Item "D:\maged\Khadamat\wwwroot\downloads\khadamat.apk" | Select-Object Name, Length, LastWriteTime, CreationTime | Format-Table -AutoSize
Get-Item "src/Khadamat.WebAPI/wwwroot/downloads\khadamat.apk" | Select-Object Name, Length, LastWriteTime, CreationTime | Format-Table -AutoSize
