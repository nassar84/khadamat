# ==============================================================================
# Script: restore_images_to_server.ps1
# Description: Deploys the complete KhadamawyImages package to SmarterASP
# ==============================================================================

param(
    [string]$Password = "EI`$_SM777"
)

$sourceDir = "D:\maged\KhadamawyImages"
$zipPath = "D:\maged\KhadamawyImages_Full_Restore.zip"

if (-not (Test-Path $sourceDir)) {
    Write-Error "Source directory $sourceDir not found!"
    exit 1
}

$fileCount = (Get-ChildItem -Path $sourceDir -Recurse -File).Count
Write-Host "Found $fileCount images ready to restore from: $sourceDir" -ForegroundColor Cyan

$msDeployPath = "C:\Program Files\IIS\Microsoft Web Deploy V3\msdeploy.exe"
if (Test-Path $msDeployPath) {
    Write-Host "`nAttempting direct WebDeploy to SmarterASP..." -ForegroundColor Cyan
    $computerName = "https://win8119.site4now.net:8172/msdeploy.axd?site=magedtest11-001-subsite8"
    $siteName = "magedtest11-001-subsite8"
    $userName = "magedtest11-001"

    $destArg = "contentPath=`"$siteName/KhadamawyImages`",computerName=`"$computerName`",userName=`"$userName`",password=`"$Password`",authtype=`"Basic`",includeAcls=`"False`""

    $deployArgs = @(
        "-verb:sync",
        "-source:contentPath=`"$sourceDir`"",
        "-dest:$destArg",
        "-allowUntrusted"
    )

    try {
        & $msDeployPath $deployArgs
        if ($LASTEXITCODE -eq 0) {
            Write-Host "Successfully restored images via WebDeploy! 🚀" -ForegroundColor Green
            exit 0
        }
    } catch {
        Write-Warning "WebDeploy failed: $_"
    }
}

Write-Host "`nIf WebDeploy port 8172 is not open or reachable, use SmarterASP File Manager:" -ForegroundColor Yellow
Write-Host " 1. Open SmarterASP Control Panel -> File Manager" -ForegroundColor Yellow
Write-Host " 2. Navigate to: /khadamawy/KhadamawyImages" -ForegroundColor Yellow
Write-Host " 3. Upload file: $zipPath" -ForegroundColor Yellow
Write-Host " 4. Extract / Unzip into /khadamawy/KhadamawyImages" -ForegroundColor Yellow
