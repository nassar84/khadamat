param(
    [string]$IntermediateDir
)

if ([string]::IsNullOrWhiteSpace($IntermediateDir)) {
    $IntermediateDir = Join-Path $PSScriptRoot "obj\Release\net10.0-android"
} else {
    $IntermediateDir = $IntermediateDir.Trim('"').Trim("'").TrimEnd('\')
    if (-not [System.IO.Path]::IsPathRooted($IntermediateDir)) {
        $IntermediateDir = Join-Path $PSScriptRoot $IntermediateDir
    }
}

$refFile = Join-Path $IntermediateDir "reference_paths.txt"

$subDirs = @($IntermediateDir)
$arm64 = Join-Path $IntermediateDir "android-arm64"
if (Test-Path $arm64) { $subDirs += $arm64 }

if (Test-Path $refFile) {
    $lines = Get-Content $refFile
    $count = 0

    foreach ($line in $lines) {
        $path = $line.Trim().Trim('"')
        if ($path -and (Test-Path $path)) {
            $leaf = Split-Path $path -Leaf
            
            # Skip Microsoft.NETCore.App.Ref System.Runtime.InteropServices so it doesn't shadow Mono runtime
            if ($leaf -eq "System.Runtime.InteropServices.dll" -and $path -like "*Microsoft.NETCore.App.Ref*") {
                continue
            }

            foreach ($dir in $subDirs) {
                $dest = Join-Path $dir $leaf
                if (-not (Test-Path $dest)) {
                    Copy-Item $path -Destination $dest -Force -ErrorAction SilentlyContinue
                }
            }
            $count++
        }
    }
    Write-Output "filter_assemblies: Synchronized $count reference assemblies to $IntermediateDir and sub-ABIs."
}

# Now guarantee Mono Android runtime pack assemblies are in place for MarshalMethods
$monoBase = "D:\dotnet10\packs\Microsoft.NETCore.App.Runtime.Mono.android-arm64\10.0.14\runtimes\android-arm64"
$monoInterop = Join-Path $monoBase "lib\net10.0\System.Runtime.InteropServices.dll"
$monoCoreLib = Join-Path $monoBase "native\System.Private.CoreLib.dll"

foreach ($dir in $subDirs) {
    if (Test-Path $monoInterop) {
        Copy-Item $monoInterop -Destination (Join-Path $dir "System.Runtime.InteropServices.dll") -Force
    }
    if (Test-Path $monoCoreLib) {
        Copy-Item $monoCoreLib -Destination (Join-Path $dir "System.Private.CoreLib.dll") -Force
    }
}
Write-Output "filter_assemblies: Deployed Mono android-arm64 runtime assemblies (System.Runtime.InteropServices & System.Private.CoreLib)."
