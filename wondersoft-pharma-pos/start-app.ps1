# PharmaAid Pro PowerShell Launcher
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$htmlPath = Join-Path $scriptDir "index.html"
Write-Host "Starting PharmaAid Pro (Wondersoft Shopaid Remake)..." -ForegroundColor Cyan
Start-Process $htmlPath
