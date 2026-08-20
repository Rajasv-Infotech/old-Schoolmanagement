param(
    [switch]$SkipInstaller
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $root

$appOut = Join-Path $root 'artifacts\app'
$installerOut = Join-Path $root 'artifacts\installer'
Remove-Item $appOut -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item $installerOut -Recurse -Force -ErrorAction SilentlyContinue
New-Item -ItemType Directory -Force -Path $appOut, $installerOut | Out-Null

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw 'dotnet CLI was not found. Install Visual Studio 2022 with .NET desktop development or a compatible .NET SDK plus .NET Framework 4.8 Developer Pack.'
}

Write-Host 'Restoring NuGet packages...'
dotnet restore .\RajasvSchoolManagement.csproj
if ($LASTEXITCODE -ne 0) { throw "dotnet restore failed with exit code $LASTEXITCODE" }

Write-Host 'Building Release (AnyCPU, .NET Framework 4.8)...'
dotnet build .\RajasvSchoolManagement.csproj -c Release -o $appOut --no-restore
if ($LASTEXITCODE -ne 0) { throw "dotnet build failed with exit code $LASTEXITCODE" }

if (-not (Test-Path (Join-Path $appOut 'RajasvSchoolManagement.exe'))) {
    throw "Build completed but RajasvSchoolManagement.exe was not found in $appOut"
}

if ($SkipInstaller) {
    Write-Host "Application build ready: $appOut"
    exit 0
}

$prereq = Join-Path $root 'Installer\Prerequisites\ndp48-x86-x64-allos-enu.exe'
if (-not (Test-Path $prereq)) {
    throw "Missing bundled prerequisite: $prereq`nRun .\Download-Prerequisites.ps1 first."
}

$nsisCandidates = @(
    "${env:ProgramFiles(x86)}\NSIS\makensis.exe",
    "$env:ProgramFiles\NSIS\makensis.exe"
) | Where-Object { $_ -and (Test-Path $_) }

if ($nsisCandidates.Count -eq 0) {
    throw 'NSIS compiler (makensis.exe) was not found. Install NSIS 3.x, then run this script again.'
}

$nsi = Join-Path $root 'Installer\SchoolManagementSetup.nsi'
Write-Host 'Compiling single-file offline setup with NSIS...'
& $nsisCandidates[0] $nsi
if ($LASTEXITCODE -ne 0) { throw "NSIS failed with exit code $LASTEXITCODE" }

$finalSetup = Join-Path $installerOut 'RajasvSchoolSetup.exe'
if (-not (Test-Path $finalSetup)) {
    throw "NSIS reported success but the final setup was not found: $finalSetup"
}

Write-Host "DONE: $finalSetup"
