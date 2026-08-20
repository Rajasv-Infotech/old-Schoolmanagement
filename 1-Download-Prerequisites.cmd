@echo off
setlocal
cd /d "%~dp0"
powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Download-Prerequisites.ps1"
if errorlevel 1 (
  echo.
  echo Prerequisite download failed.
  pause
  exit /b 1
)
echo.
echo Prerequisite download completed.
pause
