# ==============================================================================
# Script: deploy_to_server.ps1
# Description: Compiles WebAPI + Blazor WASM and deploys directly to SmarterASP
#              via WebDeploy (MsDeploy) without manual zip/unzip steps.
# ==============================================================================

param(
    [string]$Password = "EI`$_SM777"
)

$ErrorActionPreference = "Stop"

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host " 1. Publishing WebAPI (win-x64, Self-Contained)..." -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

$publishDir = "D:\maged\Khadamat"
if (!(Test-Path $publishDir)) {
    New-Item -ItemType Directory -Path $publishDir -Force | Out-Null
}

dotnet publish src/Khadamat.WebAPI/Khadamat.WebAPI.csproj `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -o $publishDir

if ($LASTEXITCODE -ne 0) {
    Write-Error "WebAPI publish failed!"
    exit 1
}

Write-Host "`n==========================================================" -ForegroundColor Cyan
Write-Host " 2. Ensuring APK file is in wwwroot/downloads..." -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

$apkSource = "src/Khadamat.WebAPI/wwwroot/downloads/khadamat.apk"
$apkDest = "$publishDir\wwwroot\downloads\khadamat.apk"
if (Test-Path $apkSource) {
    if (!(Test-Path "$publishDir\wwwroot\downloads")) {
        New-Item -ItemType Directory -Path "$publishDir\wwwroot\downloads" -Force | Out-Null
    }
    Copy-Item -Path $apkSource -Destination $apkDest -Force
    Write-Host "Copied khadamat.apk to publish downloads directory." -ForegroundColor Green
}

Write-Host "`n==========================================================" -ForegroundColor Cyan
Write-Host " 3. Syncing to SmarterASP via WebDeploy..." -ForegroundColor Cyan
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
} else {
    Write-Error "WebDeploy failed with exit code $LASTEXITCODE"
    exit 1
}
