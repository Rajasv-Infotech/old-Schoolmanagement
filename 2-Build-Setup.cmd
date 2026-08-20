@echo off
setlocal
cd /d "%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0build-release.ps1"
if errorlevel 1 (
  echo.
  echo Build failed. Read the error above and Docs\SETUP_GUIDE.md.
  pause
  exit /b 1
)
echo.
echo Setup created at artifacts\installer\RajasvSchoolSetup.exe
pause
