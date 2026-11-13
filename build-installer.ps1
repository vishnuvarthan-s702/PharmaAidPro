$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$msbuild = 'C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe'
$csc = 'C:\Windows\Microsoft.NET\Framework\v4.0.30319\csc.exe'

Write-Host "1. Building PharmaAidPro Release (x86)..."
& $msbuild "$root\PharmaAidPro.sln" /p:Configuration=Release /p:Platform=x86 /v:minimal

$releaseDir = "$root\src\PharmaAidPro.WinForms\bin\x86\Release"
$installerDir = "$root\installer"
$stagingDir = "$installerDir\staging"
$payloadZip = "$installerDir\payload.zip"

if (Test-Path $stagingDir) { Remove-Item $stagingDir -Recurse -Force }
if (Test-Path $payloadZip) { Remove-Item $payloadZip -Force }
New-Item -ItemType Directory -Force -Path $stagingDir | Out-Null
New-Item -ItemType Directory -Force -Path "$stagingDir\Assets" | Out-Null
New-Item -ItemType Directory -Force -Path "$stagingDir\x86" | Out-Null

# Copy clean application binaries and assets (excluding pdb/xml/local test db)
Copy-Item "$releaseDir\*.exe" $stagingDir -Force
Copy-Item "$releaseDir\*.config" $stagingDir -Force
Copy-Item "$releaseDir\*.dll" $stagingDir -Force
Copy-Item "$releaseDir\Assets\*" "$stagingDir\Assets\" -Force
Copy-Item "$releaseDir\x86\*" "$stagingDir\x86\" -Force

Write-Host "2. Creating compressed payload.zip..."
Add-Type -AssemblyName System.IO.Compression.FileSystem
[System.IO.Compression.ZipFile]::CreateFromDirectory($stagingDir, $payloadZip, [System.IO.Compression.CompressionLevel]::Optimal, $false)
Remove-Item $stagingDir -Recurse -Force

$setupExe = "$installerDir\PharmaAidPro_Setup.exe"
$iconPath = "$root\src\PharmaAidPro.WinForms\Assets\app.ico"

Write-Host "3. Compiling standalone x86 installer (PharmaAidPro_Setup.exe)..."
& $csc /nologo /target:winexe /platform:x86 /optimize+ `
    /out:"$setupExe" `
    /win32icon:"$iconPath" `
    /reference:System.dll `
    /reference:System.Drawing.dll `
    /reference:System.Windows.Forms.dll `
    /reference:System.IO.Compression.dll `
    /reference:System.IO.Compression.FileSystem.dll `
    /resource:"$payloadZip,payload.zip" `
    "$root\src\PharmaAidPro.Setup\InstallerProgram.cs"

Remove-Item $payloadZip -Force

Write-Host "4. Verifying installer..."
& $setupExe --verify-installer
Get-Item $setupExe | Select-Object FullName, Length
