# ==============================================================================
# Script: deploy_to_server.ps1
# Description: Compiles WebAPI + Blazor WASM and deploys directly to SmarterASP
#              via WebDeploy (MsDeploy) without manual zip/unzip steps.
# ==============================================================================

param(
    [string]$Password = "EI`$_SM777",
    [switch]$SkipApk = $false
)

$ErrorActionPreference = "Stop"

$publishDir = "D:\maged\Khadamat"
if (!(Test-Path $publishDir)) {
    New-Item -ItemType Directory -Path $publishDir -Force | Out-Null
}

$now = Get-Date

# ==============================================================================
# 1. Build fresh Android APK (unless -SkipApk is passed)
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

    # Copy to WebAPI downloads with current timestamp
    New-Item -ItemType Directory -Force -Path "src/Khadamat.WebAPI/wwwroot/downloads" | Out-Null
    Copy-Item -Force $apkSource -Destination "src/Khadamat.WebAPI/wwwroot/downloads/khadamat.apk"
    (Get-Item "src/Khadamat.WebAPI/wwwroot/downloads/khadamat.apk").CreationTime = $now
    (Get-Item "src/Khadamat.WebAPI/wwwroot/downloads/khadamat.apk").LastWriteTime = $now
} else {
    Write-Host "Skipping APK build as requested (-SkipApk)." -ForegroundColor Yellow
}

# ==============================================================================
# 2. Publish WebAPI + Blazor WASM
# ==============================================================================
Write-Host "`n==========================================================" -ForegroundColor Cyan
Write-Host " 3. Publishing WebAPI (win-x64, Self-Contained)..." -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

dotnet publish src/Khadamat.WebAPI/Khadamat.WebAPI.csproj `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -o $publishDir

if ($LASTEXITCODE -ne 0) {
    Write-Error "WebAPI publish failed!"
    exit 1
}

# Exclude heavy uploaded image directories from publish output
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
        Write-Host "  [Excluded from sync] wwwroot/images/$dir (Stored safely in KhadamawyImages)" -ForegroundColor Yellow
    }
}

# ==============================================================================
# 3. Finalize APK in publish directory
# ==============================================================================
Write-Host "`n==========================================================" -ForegroundColor Cyan
Write-Host " 4. Ensuring strictly ONE APK in publish downloads..." -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

$webApiApk = "src/Khadamat.WebAPI/wwwroot/downloads/khadamat.apk"
New-Item -ItemType Directory -Force -Path "$publishDir\wwwroot\downloads" | Out-Null

# Clean any non-matching APKs
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
    Write-Error "Error: Expected exactly 1 APK file in publish directory, but found $($publishedApks.Count)!"
    exit 1
}

# Create backup ZIP package
if (Test-Path "D:\maged\Khadamat_publish.zip") {
    Remove-Item -Force "D:\maged\Khadamat_publish.zip" -ErrorAction SilentlyContinue
}
tar.exe -a -c -f "D:\maged\Khadamat_publish.zip" -C "$publishDir" .
Write-Host "ZIP package created successfully at: D:\maged\Khadamat_publish.zip" -ForegroundColor Green

# ==============================================================================
# 4. Syncing to SmarterASP via WebDeploy (MsDeploy)
# ==============================================================================
Write-Host "`n==========================================================" -ForegroundColor Cyan
Write-Host " 5. Syncing to SmarterASP via WebDeploy..." -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

$msDeployPath = "C:\Program Files\IIS\Microsoft Web Deploy V3\msdeploy.exe"
if (!(Test-Path $msDeployPath)) {
    Write-Error "msdeploy.exe not found at $msDeployPath"
    exit 1
}

$computerName = "https://win8119.site4now.net:8172/msdeploy.axd?site=magedtest11-001-subsite8"
$siteName = "magedtest11-001-subsite8"
$userName = "magedtest11-001"

# Skip user-uploaded directories and server-specific configs to protect permanent data
$skipRules = @(
    "-skip:objectName=dirPath,absolutePath=`".*KhadamawyImages.*`"",
    "-skip:objectName=dirPath,absolutePath=`".*wwwroot\\\\images\\\\ads.*`"",
    "-skip:objectName=dirPath,absolutePath=`".*wwwroot\\\\images\\\\categories.*`"",
    "-skip:objectName=dirPath,absolutePath=`".*wwwroot\\\\images\\\\maincategories.*`"",
    "-skip:objectName=dirPath,absolutePath=`".*wwwroot\\\\images\\\\subcategories.*`"",
    "-skip:objectName=dirPath,absolutePath=`".*wwwroot\\\\images\\\\Services.*`"",
    "-skip:objectName=dirPath,absolutePath=`".*wwwroot\\\\images\\\\users.*`"",
    "-skip:objectName=dirPath,absolutePath=`".*wwwroot\\\\images\\\\marketplace.*`"",
    "-skip:objectName=dirPath,absolutePath=`".*wwwroot\\\\images\\\\share_cards.*`"",
    "-skip:objectName=dirPath,absolutePath=`".*wwwroot\\\\images\\\\general.*`"",
    "-skip:objectName=filePath,absolutePath=`".*appsettings\.Secrets\.json.*`""
)

$destArg = "contentPath=`"$siteName`",computerName=`"$computerName`",userName=`"$userName`",password=`"$Password`",authtype=`"Basic`",includeAcls=`"False`""

$deployArgs = @(
    "-verb:sync",
    "-source:contentPath=`"$publishDir`"",
    "-dest:$destArg",
    "-enableRule:AppOffline",
    "-allowUntrusted"
) + $skipRules

& $msDeployPath $deployArgs

if ($LASTEXITCODE -eq 0) {
    Write-Host "`n==========================================================" -ForegroundColor Green
    Write-Host " DEPLOYMENT COMPLETED SUCCESSFULLY TO SERVER! 🚀" -ForegroundColor Green
    Write-Host " Target URL: https://khadamawy.eis-dev.com/" -ForegroundColor Green
    Write-Host "==========================================================" -ForegroundColor Green
    $publishedApks | Select-Object Name, Length, LastWriteTime, CreationTime | Format-Table -AutoSize
} else {
    Write-Error "WebDeploy failed with exit code $LASTEXITCODE"
    exit 1
}
