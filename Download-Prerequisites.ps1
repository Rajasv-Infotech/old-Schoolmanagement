$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$destDir = Join-Path $root 'Installer\Prerequisites'
$dest = Join-Path $destDir 'ndp48-x86-x64-allos-enu.exe'
New-Item -ItemType Directory -Force -Path $destDir | Out-Null
Write-Host 'Downloading official Microsoft .NET Framework 4.8 offline installer...'
Invoke-WebRequest -UseBasicParsing -Uri 'https://go.microsoft.com/fwlink/?linkid=2088631' -OutFile $dest
Write-Host "Saved: $dest"
