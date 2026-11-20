$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$msbuild = 'C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe'
$csc35 = 'C:\Windows\Microsoft.NET\Framework\v3.5\csc.exe'

Write-Host "1. Building PharmaAidPro Release (x86, .NET 3.5 / v2.0.50727 runtime for Windows 7 SP1)..."
& $msbuild "$root\PharmaAidPro.sln" /t:Rebuild /p:Configuration=Release /p:Platform=x86 /v:minimal

$releaseDir = "$root\src\PharmaAidPro.WinForms\bin\x86\Release"
$installerDir = "$root\installer"
$portableDir = "$installerDir\PharmaAidPro_Portable"
$payloadPak = "$installerDir\payload.pak"

if (Test-Path $portableDir) { Remove-Item $portableDir -Recurse -Force }
if (Test-Path $payloadPak) { Remove-Item $payloadPak -Force }
New-Item -ItemType Directory -Force -Path $portableDir | Out-Null
New-Item -ItemType Directory -Force -Path "$portableDir\Assets" | Out-Null
New-Item -ItemType Directory -Force -Path "$portableDir\x86" | Out-Null

# Copy clean application binaries and assets (only .NET 2.0/3.5 dependencies!)
Copy-Item "$releaseDir\PharmaAidPro.exe" $portableDir -Force
Copy-Item "$releaseDir\PharmaAidPro.exe.config" $portableDir -Force
Copy-Item "$releaseDir\System.Data.SQLite.dll" $portableDir -Force
Copy-Item "$releaseDir\Assets\*" "$portableDir\Assets\" -Force
Copy-Item "$releaseDir\x86\*" "$portableDir\x86\" -Force

Write-Host "2. Creating .NET 2.0 GZipStream payload.pak..."
$files = Get-ChildItem $portableDir -Recurse | Where-Object { -not $_.PSIsContainer }
$fs = [System.IO.File]::Create($payloadPak)
$gz = New-Object System.IO.Compression.GZipStream($fs, [System.IO.Compression.CompressionMode]::Compress)
$bw = New-Object System.IO.BinaryWriter($gz)
$bw.Write([int]$files.Count)
foreach ($f in $files) {
    $rel = $f.FullName.Substring($portableDir.Length + 1).Replace('\', '/')
    $bytes = [System.IO.File]::ReadAllBytes($f.FullName)
    $bw.Write([string]$rel)
    $bw.Write([int]$bytes.Length)
    $bw.Write($bytes)
}
$bw.Flush()
$bw.Close()
$gz.Close()
$fs.Close()

$setupExe = "$installerDir\PharmaAidPro_Setup.exe"
$setupConfig = "$installerDir\PharmaAidPro_Setup.exe.config"
$iconPath = "$root\src\PharmaAidPro.WinForms\Assets\app.ico"
$logoPath = "$root\src\PharmaAidPro.WinForms\Assets\app_logo.jpg"

Write-Host "3. Compiling standalone x86 .NET 2.0/3.5 installer (PharmaAidPro_Setup.exe)..."
& $csc35 /nologo /target:winexe /platform:x86 /optimize+ `
    /out:"$setupExe" `
    /win32icon:"$iconPath" `
    /reference:System.dll `
    /reference:System.Drawing.dll `
    /reference:System.Windows.Forms.dll `
    /resource:"$payloadPak,payload.pak" `
    /resource:"$logoPath,app_logo.jpg" `
    "$root\src\PharmaAidPro.Setup\InstallerProgram.cs"

Remove-Item $payloadPak -Force

# Copy dual-runtime config next to PharmaAidPro_Setup.exe so it works on both .NET 2.0/3.5 (Win7) and .NET 4.x (Win10/11)
Copy-Item "$releaseDir\PharmaAidPro.exe.config" $setupConfig -Force

# Also create a 1-click Windows 7 Setup & Desktop Shortcut batch script
$batContent = @"
@echo off
title The Nethaji Medical Stores - PharmaAid Pro Setup
echo ============================================================
echo   The Nethaji Medical Stores - PharmaAid Pro Setup (Win 7)
echo ============================================================
cd /d "%~dp0"

REM Unblock files if copied from USB / Internet
if exist PharmaAidPro_Setup.exe (
    start "" "%~dp0PharmaAidPro_Setup.exe"
) else (
    start "" "%~dp0PharmaAidPro_Portable\PharmaAidPro.exe"
)
"@
[System.IO.File]::WriteAllText("$installerDir\Run_Setup_Windows7.bat", $batContent, [System.Text.Encoding]::ASCII)

Write-Host "4. Verifying installer and runtime versions..."
$setupAsm = [System.Reflection.Assembly]::ReflectionOnlyLoadFrom($setupExe)
$appAsm = [System.Reflection.Assembly]::ReflectionOnlyLoadFrom("$portableDir\PharmaAidPro.exe")
Write-Output "PharmaAidPro_Setup.exe Runtime: $($setupAsm.ImageRuntimeVersion)"
Write-Output "PharmaAidPro.exe Runtime:       $($appAsm.ImageRuntimeVersion)"
& $setupExe --verify-installer
& "$portableDir\PharmaAidPro.exe" --verify-db
Get-Item $setupExe | Select-Object FullName, Length
