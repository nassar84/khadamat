$srcImages = "src/Khadamat.WebAPI/wwwroot/images"
$outZip = "D:\maged\KhadamawyImages_Restore.zip"

Write-Host "Checking images in: $srcImages"
$allFiles = Get-ChildItem -Path $srcImages -Recurse -File
Write-Host "Total images found: $($allFiles.Count)"

$totalBytes = ($allFiles | Measure-Object -Property Length -Sum).Sum
$totalMB = [math]::Round($totalBytes / 1MB, 2)
Write-Host "Total size: $totalMB MB"

if (Test-Path $outZip) {
    Remove-Item -Force $outZip
}

# Zip directly the contents of images into KhadamawyImages_Restore.zip
tar.exe -a -c -f $outZip -C $srcImages .
Write-Host "Created restore zip at: $outZip (Size: $([math]::Round((Get-Item $outZip).Length / 1MB, 2)) MB)"
