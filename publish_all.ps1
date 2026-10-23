# ==============================================================================
# Publish Script for Khadamat: Builds APK + WebAPI (Blazor WASM) + Copies APK
# Enforces strictly ONE single fresh APK with current timestamp
# ==============================================================================

param(
    [switch]$Deploy = $false,
    [switch]$SkipApk = $false
)

$publishDir = "D:\maged\Khadamat"
if (!(Test-Path $publishDir)) {
    New-Item -ItemType Directory -Path $publishDir -Force | Out-Null
}

$now = Get-Date

# ==============================================================================
# 1. Build Android APK (unless -SkipApk is specified)
# ==============================================================================
if (-not $SkipApk) {
    Write-Host "==========================================================" -ForegroundColor Cyan
    Write-Host " 1. Cleaning old APKs and preparing for fresh build..." -ForegroundColor Cyan
    Write-Host "==========================================================" -ForegroundColor Cyan

    Get-ChildItem -Path "src/Khadamat.MobileApp/bin/Release/net9.0-android" -Recurse -Filter "*.apk" -ErrorAction SilentlyContinue | Remove-Item -Force -ErrorAction SilentlyContinue
    Get-ChildItem -Path "src/Khadamat.WebAPI/wwwroot/downloads" -Filter "*.apk" -ErrorAction SilentlyContinue | Remove-Item -Force -ErrorAction SilentlyContinue
    if (Test-Path "$publishDir\wwwroot\downloads") {
        Get-ChildItem -Path "$publishDir\wwwroot\downloads" -Filter "*.apk" -ErrorAction SilentlyContinue | Remove-Item -Force -ErrorAction SilentlyContinue
    }

    Write-Host "`n==========================================================" -ForegroundColor Cyan
    Write-Host " 2. Building Android APK (Release - Single APK)..." -ForegroundColor Cyan
    Write-Host "==========================================================" -ForegroundColor Cyan

    dotnet publish src/Khadamat.MobileApp/Khadamat.MobileApp.csproj -f net9.0-android -c Release
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Android APK build failed!"
        exit $LASTEXITCODE
    }

    $apkSource = "src/Khadamat.MobileApp/bin/Release/net9.0-android/android-arm64/publish/com.nassar84.khadamawy-Signed.apk"
    if (-not (Test-Path $apkSource)) {
        $apkSource = "src/Khadamat.MobileApp/bin/Release/net9.0-android/android-arm64/com.nassar84.khadamawy-Signed.apk"
    }

    if (-not (Test-Path $apkSource)) {
        Write-Error "Could not find generated APK at: $apkSource"
        exit 1
    }

    Write-Host "APK Built successfully: $apkSource" -ForegroundColor Green

    # Copy to WebAPI downloads with fresh timestamp
    New-Item -ItemType Directory -Force -Path "src/Khadamat.WebAPI/wwwroot/downloads" | Out-Null
    Copy-Item -Force $apkSource -Destination "src/Khadamat.WebAPI/wwwroot/downloads/khadamat.apk"
    (Get-Item "src/Khadamat.WebAPI/wwwroot/downloads/khadamat.apk").CreationTime = $now
    (Get-Item "src/Khadamat.WebAPI/wwwroot/downloads/khadamat.apk").LastWriteTime = $now
} else {
    Write-Host "Skipping APK build (-SkipApk was specified or using existing APK)." -ForegroundColor Yellow
}

# ==============================================================================
# 2. Publish WebAPI + Blazor WASM
# ==============================================================================
Write-Host "`n==========================================================" -ForegroundColor Cyan
Write-Host " 3. Publishing WebAPI & Blazor WASM to $publishDir (Self-Contained win-x64)..." -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

dotnet publish src/Khadamat.WebAPI/Khadamat.WebAPI.csproj `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -o $publishDir

if ($LASTEXITCODE -ne 0) {
    Write-Error "WebAPI publish failed!"
    exit $LASTEXITCODE
}

# ==============================================================================
# 3. Exclude heavy uploaded image directories from publish output
# ==============================================================================
Write-Host "`n==========================================================" -ForegroundColor Cyan
Write-Host " 4. Excluding heavy uploaded image directories from publish output..." -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

$uploadDirsToExclude = @(
    "ads",
    "categories",
    "maincategories",
    "subcategories",
    "Services",
    "users",
    "marketplace",
    "share_cards",
    "general"
)

foreach ($dir in $uploadDirsToExclude) {
    $targetPath = Join-Path "$publishDir\wwwroot\images" $dir
    if (Test-Path $targetPath) {
        Remove-Item -Recurse -Force $targetPath -ErrorAction SilentlyContinue
        Write-Host "  [Excluded from ZIP] wwwroot/images/$dir (Stored safely in KhadamawyImages)" -ForegroundColor Yellow
    }
}

# ==============================================================================
# 4. Finalize APK in publish directory
# ==============================================================================
Write-Host "`n==========================================================" -ForegroundColor Cyan
Write-Host " 5. Finalizing publish directory (Strictly ONE APK with current timestamp)..." -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

$webApiApk = "src/Khadamat.WebAPI/wwwroot/downloads/khadamat.apk"
if (-not (Test-Path $webApiApk) -and (Test-Path "khadamat.apk")) {
    $webApiApk = "khadamat.apk"
}

New-Item -ItemType Directory -Force -Path "$publishDir\wwwroot\downloads" | Out-Null
Get-ChildItem -Path "$publishDir\wwwroot\downloads" -Filter "*.apk" -ErrorAction SilentlyContinue | Where-Object { $_.Name -ne "khadamat.apk" } | Remove-Item -Force -ErrorAction SilentlyContinue

if (Test-Path $webApiApk) {
    Copy-Item -Path $webApiApk -Destination "$publishDir\wwwroot\downloads\khadamat.apk" -Force
    (Get-Item "$publishDir\wwwroot\downloads\khadamat.apk").CreationTime = $now
    (Get-Item "$publishDir\wwwroot\downloads\khadamat.apk").LastWriteTime = $now

    # Update root convenience copy
    Copy-Item -Force $webApiApk -Destination "khadamat.apk"
    (Get-Item "khadamat.apk").CreationTime = $now
    (Get-Item "khadamat.apk").LastWriteTime = $now
}

$publishedApks = Get-ChildItem -Path "$publishDir\wwwroot\downloads" -Filter "*.apk"
if ($publishedApks.Count -ne 1) {
    Write-Warning "Warning: Expected 1 APK in downloads, found $($publishedApks.Count)."
}

# ==============================================================================
# 5. Create deployment package (ZIP)
# ==============================================================================
Write-Host "`n==========================================================" -ForegroundColor Cyan
Write-Host " 6. Creating deployment package (D:\maged\Khadamat_publish.zip)..." -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

if (Test-Path "D:\maged\Khadamat_publish.zip") {
    Remove-Item -Force "D:\maged\Khadamat_publish.zip" -ErrorAction SilentlyContinue
}
tar.exe -a -c -f "D:\maged\Khadamat_publish.zip" -C "$publishDir" .
Write-Host "ZIP package created successfully at: D:\maged\Khadamat_publish.zip" -ForegroundColor Green

Write-Host "`n==========================================================" -ForegroundColor Green
Write-Host " PUBLISH COMPLETE! Directory: $publishDir" -ForegroundColor Green
Write-Host "==========================================================" -ForegroundColor Green

if ($publishedApks.Count -gt 0) {
    $publishedApks | Select-Object Name, Length, LastWriteTime, CreationTime | Format-Table -AutoSize
}

if ($Deploy) {
    Write-Host "`n==========================================================" -ForegroundColor Cyan
    Write-Host " 7. Triggering automatic deployment to SmarterASP..." -ForegroundColor Cyan
    Write-Host "==========================================================" -ForegroundColor Cyan
    & .\deploy_to_server.ps1 -SkipApk
}
