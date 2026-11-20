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