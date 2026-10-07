$files = Get-ChildItem -Path "d:\DEV\khadamat\src" -Recurse -Include *.cs,*.razor | Where-Object { $_.FullName -notmatch '\\(bin|obj|wwwroot\\lib)\\' }
foreach ($f in $files) {
    $text = [System.IO.File]::ReadAllText($f.FullName, [System.Text.Encoding]::UTF8)
    if ($text -match 'متجر' -or $text -match 'تطبيق' -and $text -match 'موقع') {
        $lines = $text -split "`r?`n"
        for ($i = 0; $i -lt $lines.Length; $i++) {
            if ($lines[$i] -match 'رابط') {
                Write-Host "$($f.Name):$($i+1): $($lines[$i].Trim())"
            }
        }
    }
}
