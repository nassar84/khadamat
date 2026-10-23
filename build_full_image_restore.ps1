Add-Type -AssemblyName System.IO.Compression.FileSystem

$prodZip = "D:\maged\khadamat_production.zip"
$stageDir = "D:\maged\KhadamawyImages_Restore_Stage"
$outZip = "D:\maged\KhadamawyImages_Full_Restore.zip"

Write-Host "Creating clean staging directory: $stageDir"
if (Test-Path $stageDir) {
    Remove-Item -Recurse -Force $stageDir -ErrorAction SilentlyContinue
}
New-Item -ItemType Directory -Path $stageDir -Force | Out-Null

$extractedCount = 0

# 1. Extract from Khadamat_Release.zip first
if (Test-Path "D:\maged\Khadamat_Release.zip") {
    Write-Host "Extracting from Khadamat_Release.zip..."
    $archive = [System.IO.Compression.ZipFile]::OpenRead("D:\maged\Khadamat_Release.zip")
    foreach ($entry in $archive.Entries) {
        $norm = $entry.FullName.Replace('\', '/')
        if ($norm.StartsWith("wwwroot/images/") -and -not $norm.EndsWith("/")) {
            $relativeName = $norm.Substring("wwwroot/images/".Length)
            $destinationPath = [System.IO.Path]::Combine($stageDir, $relativeName.Replace('/', '\'))
            $destinationDir = [System.IO.Path]::GetDirectoryName($destinationPath)
            if (-not (Test-Path $destinationDir)) {
                New-Item -ItemType Directory -Path $destinationDir -Force | Out-Null
            }
            [System.IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $destinationPath, $true)
            $extractedCount++
        }
    }
    $archive.Dispose()
    Write-Host "Extracted from Khadamat_Release.zip. Current total: $extractedCount"
}

# 2. Extract from production zip (overwriting / adding)
Write-Host "Opening production zip: $prodZip..."
$archive = [System.IO.Compression.ZipFile]::OpenRead($prodZip)
foreach ($entry in $archive.Entries) {
    $norm = $entry.FullName.Replace('\', '/')
    if ($norm.StartsWith("wwwroot/images/") -and -not $norm.EndsWith("/")) {
        $relativeName = $norm.Substring("wwwroot/images/".Length)
        $destinationPath = [System.IO.Path]::Combine($stageDir, $relativeName.Replace('/', '\'))
        $destinationDir = [System.IO.Path]::GetDirectoryName($destinationPath)
        if (-not (Test-Path $destinationDir)) {
            New-Item -ItemType Directory -Path $destinationDir -Force | Out-Null
        }
        [System.IO.Compression.ZipFileExtensions]::ExtractToFile($entry, $destinationPath, $true)
        $extractedCount++
    }
}
$archive.Dispose()
Write-Host "Extracted from production backup. Current total processed: $extractedCount"

# 3. Overlay local development images (latest icons, themes, categories)
$localImages = "src/Khadamat.WebAPI/wwwroot/images"
if (Test-Path $localImages) {
    Write-Host "Overlaying local development images from $localImages..."
    robocopy $localImages $stageDir /E /XO /NP /NFL /NDL /NJH /NJS | Out-Null
}

$allFiles = Get-ChildItem -Path $stageDir -Recurse -File
$fileCount = $allFiles.Count
$totalSize = [math]::Round((($allFiles | Measure-Object -Property Length -Sum).Sum / 1MB), 2)
Write-Host "Total unique files in staging: $fileCount ($totalSize MB)"

if (Test-Path $outZip) {
    Remove-Item -Force $outZip
}

Write-Host "Creating final restore ZIP: $outZip..."
tar.exe -a -c -f $outZip -C $stageDir .

$zipSize = [math]::Round((Get-Item $outZip).Length / 1MB, 2)
Write-Host "SUCCESS! Restore ZIP ready at: $outZip ($zipSize MB)"

# Clean up staging
Remove-Item -Recurse -Force $stageDir -ErrorAction SilentlyContinue
