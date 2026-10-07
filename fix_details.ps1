$path = 'd:\DEV\khadamat\src\Khadamat.BlazorUI\Pages\ServiceDetails.razor'
$content = [System.IO.File]::ReadAllText($path, [System.Text.Encoding]::UTF8)
$old = 'btn-info rounded-pill px-4 py-2 text-white fw-bold shadow-sm" @onclick="OpenRequestEditModal"'
$new = 'btn-info rounded-pill px-3 py-1 text-white fw-bold shadow-sm flex-shrink-0" style="font-size:0.8rem; white-space:nowrap;" @onclick="OpenRequestEditModal"'
if ($content.Contains($old)) {
    $content2 = $content.Replace($old, $new)
    [System.IO.File]::WriteAllText($path, $content2, [System.Text.Encoding]::UTF8)
    Write-Host "OK - replaced"
} else {
    Write-Host "NOT FOUND"
    # Try simpler search
    if ($content.Contains('btn-info rounded-pill px-4 py-2')) {
        Write-Host "Found simpler pattern"
    } else {
        Write-Host "No match at all"
    }
}
