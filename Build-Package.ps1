# Build, pack, and patch the XrmToolBox plugin nupkg.
#
# NuGet pack v5+ silently strips the <group targetFramework="any"> wrapper and
# normalizes bracket version ranges in the embedded nuspec. The xrmtoolbox.com
# validator requires the v2 OData Dependencies field to have 3 colon-separated
# parts (id:version-range:framework). That only happens when the source nuspec
# keeps the group wrapper. This script post-patches the embedded nuspec inside
# the .nupkg to restore the required structure.

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Definition
$proj = Join-Path $root "CopilotStudioHealthMonitor\CopilotStudioHealthMonitor.csproj"
$spec = Join-Path $root "CopilotStudioHealthMonitor\CopilotStudioHealthMonitor.nuspec"
$out  = Join-Path $root "NuGetPackages"

# 1. Build Release
& "D:\VS\MSBuild\Current\Bin\MSBuild.exe" $proj /p:Configuration=Release /t:Rebuild /nologo /verbosity:minimal

# 2. Read version from nuspec
[xml]$x = Get-Content $spec
$version = $x.package.metadata.version
$nupkg = Join-Path $out "CopilotStudioHealthMonitor.$version.nupkg"

# 3. Pack
Push-Location (Split-Path $spec)
C:\Tools\nuget.exe pack (Split-Path $spec -Leaf) -OutputDirectory $out
Pop-Location

# 4. Patch embedded nuspec inside .nupkg to restore the <group> wrapper
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip = [System.IO.Compression.ZipFile]::Open($nupkg, [System.IO.Compression.ZipArchiveMode]::Update)
$entry = $zip.Entries | Where-Object { $_.Name -like "*.nuspec" }
$reader = New-Object System.IO.StreamReader($entry.Open())
$content = $reader.ReadToEnd()
$reader.Close()

$xtbVersion = $x.package.metadata.dependencies.group.dependency.version -replace '[\[\(\)\] ,]', ''
if (-not $xtbVersion) { $xtbVersion = $x.package.metadata.dependencies.dependency.version }
$flatPattern = "<dependency id=`"XrmToolBox`" version=`"$xtbVersion`" />"
$replacement = "<group targetFramework=`"any`">`r`n        <dependency id=`"XrmToolBox`" version=`"[$xtbVersion, )`" />`r`n      </group>"
$patched = $content -replace [regex]::Escape($flatPattern), $replacement

if ($patched -eq $content) {
  Write-Warning "Pattern '$flatPattern' not found - nuspec may already be patched or version mismatch"
} else {
  $entry.Delete()
  $newEntry = $zip.CreateEntry("CopilotStudioHealthMonitor.nuspec")
  $writer = New-Object System.IO.StreamWriter($newEntry.Open())
  $writer.Write($patched)
  $writer.Close()
  Write-Host "Patched dependency to use <group targetFramework='any'> with range [$xtbVersion, )"
}
$zip.Dispose()

Write-Host "Built: $nupkg"
Write-Host "Upload with: C:\Tools\nuget.exe push `"$nupkg`" -ApiKey YOUR_KEY -Source https://api.nuget.org/v3/index.json"
