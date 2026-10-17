# ==============================================================================
# Script: build_mobile_release.ps1
# Description: Builds the Khadamat Android app (APK and/or AAB for Google Play),
#              signed with the RELEASE keystore (khadamawy_release.keystore).
#
# Why this script exists:
#   Running `dotnet publish` without signing properties makes .NET Android fall
#   back to the shared Android DEBUG key (CN=Android Debug). That produces an
#   APK that cannot be updated by (or over) the Play Store build, which is signed
#   with the release key. This script always signs with the release key.
#
# Usage:
#   powershell -ExecutionPolicy Bypass -File .\build_mobile_release.ps1 -Target apk
#   powershell -ExecutionPolicy Bypass -File .\build_mobile_release.ps1 -Target aab
#   powershell -ExecutionPolicy Bypass -File .\build_mobile_release.ps1 -Target all
# ==============================================================================

param(
    [ValidateSet("apk", "aab", "all")]
    [string]$Target = "all",
    [switch]$BumpVersion
)

$ErrorActionPreference = "Stop"

$repoRoot  = $PSScriptRoot
$project   = Join-Path $repoRoot "src\Khadamat.MobileApp\Khadamat.MobileApp.csproj"
$keystore  = Join-Path $repoRoot "khadamawy_release.keystore"
$keyAlias  = "khadamawy"
$keyPass   = "khadamawy2026"
$storePass = "khadamawy2026"
$androidOut = Join-Path $repoRoot "src\Khadamat.MobileApp\bin\Release\net9.0-android36.0"

if (-not (Test-Path $keystore)) {
    Write-Error "Release keystore not found at: $keystore"
    exit 1
}
if (-not (Test-Path $project)) {
    Write-Error "Project not found at: $project"
    exit 1
}

# AndroidKeyStore=true is REQUIRED: without it .NET Android ignores the signing
# properties below and silently falls back to the shared Android debug key.
$signingArgs = @(
    "-p:AndroidKeyStore=true",
    "-p:AndroidSigningKeyStore=$keystore",
    "-p:AndroidSigningKeyAlias=$keyAlias",
    "-p:AndroidSigningKeyPass=$keyPass",
    "-p:AndroidSigningStorePass=$storePass"
)

function Invoke-Publish([string]$packageFormat) {
    Write-Host "==========================================================" -ForegroundColor Cyan
    Write-Host " Building $($packageFormat.ToUpper()) (Release, signed with release keystore)..." -ForegroundColor Cyan
    Write-Host "==========================================================" -ForegroundColor Cyan

    # Delete recursively: the signed APK/AAB lives in an ABI sub-folder
    # (…\android-arm64\ and …\android-arm64\publish\). If it is left behind,
    # MSBuild treats signing as up-to-date and skips it, leaving a stale
    # signature on the "fresh" artifact.
    Get-ChildItem $androidOut -Recurse -Include "*.apk", "*.apk.idsig", "*.aab" -File -ErrorAction SilentlyContinue |
        Remove-Item -Force -ErrorAction SilentlyContinue

    dotnet publish $project `
        -f net9.0-android36.0 `
        -c Release `
        -p:AndroidPackageFormat=$packageFormat `
        @signingArgs

    if ($LASTEXITCODE -ne 0) {
        Write-Error "$packageFormat build failed (exit code $LASTEXITCODE)"
        exit $LASTEXITCODE
    }
}

# ------------------------------------------------------------------------------
# AAB (Google Play upload artifact)
#
# IMPORTANT ORDER: the AAB is always built BEFORE the APK.
# Building a bundle also emits an extra `*-Signed.apk` (the bundle's input APK),
# which is a different (smaller, non-AOT) binary. The csproj `RenameAndCopyApk`
# target copies whichever APK it finds into wwwroot/downloads/khadamat.apk, so
# the APK must be built LAST for the AOT-enabled Release APK to win.
# ------------------------------------------------------------------------------
if ($Target -eq "aab" -or $Target -eq "all") {
    Invoke-Publish "aab"

    $aab = Get-ChildItem $androidOut -Recurse -Filter "*-Signed.aab" -ErrorAction SilentlyContinue |
           Sort-Object LastWriteTime -Descending | Select-Object -First 1
    if (-not $aab) {
        Write-Error "Signed AAB not found under $androidOut"
        exit 1
    }

    $stamp = Get-Date -Format "yyyyMMdd-HHmmss"
    if (Test-Path (Join-Path $repoRoot "Khadamawy.aab")) {
        Copy-Item (Join-Path $repoRoot "Khadamawy.aab") (Join-Path $repoRoot "Khadamawy.aab.bak-$stamp") -Force
    }
    Copy-Item $aab.FullName (Join-Path $repoRoot "Khadamawy.aab") -Force
    Write-Host "AAB ready for Google Play: $(Join-Path $repoRoot 'Khadamawy.aab')" -ForegroundColor Green
}

# ------------------------------------------------------------------------------
# APK (direct website download)
# ------------------------------------------------------------------------------
if ($Target -eq "apk" -or $Target -eq "all") {
    Invoke-Publish "apk"

    $apk = Get-ChildItem $androidOut -Recurse -Filter "*-Signed.apk" -ErrorAction SilentlyContinue |
           Sort-Object LastWriteTime -Descending | Select-Object -First 1
    if (-not $apk) {
        Write-Error "Signed APK not found under $androidOut"
        exit 1
    }
    # The csproj target also copies it to wwwroot/downloads/khadamat.apk
    Copy-Item $apk.FullName (Join-Path $repoRoot "khadamat.apk") -Force
    Write-Host "APK ready: $($apk.FullName)" -ForegroundColor Green
}

Write-Host "`nDone. Verify the signature matches:" -ForegroundColor Yellow
Write-Host "  SHA-256: 14:89:3D:E4:DA:58:F3:DA:30:27:6C:75:0A:8A:20:CE:76:4A:E3:6D:71:14:1B:6A:A0:93:7C:8B:2C:52:D7:9B"
