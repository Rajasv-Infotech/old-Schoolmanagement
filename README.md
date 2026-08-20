# Rajasv Infotech School Management - Offline Desktop Edition

This is a Windows Forms rewrite of the original .NET MAUI / Blazor Hybrid school management project.
It is designed for a **single school PC/laptop**, works **fully offline**, and stores data in a local **LiteDB** database (`school.db`).

## Why this rewrite exists

The original project targets .NET MAUI / Windows App SDK and therefore cannot reliably run on Windows 7/8.1. It also depends on Windows App Runtime / WebView2. This edition removes those dependencies.

## Target client systems

- Windows 11
- Windows 10
- Windows 8.1 (legacy / best-effort)
- Windows 7 **SP1** (legacy / best-effort)
- x86 and x64 PCs (the app is AnyCPU with Prefer32Bit)

Windows XP, Vista, and Windows 7 without SP1 are intentionally unsupported.

> Windows 7 and 8.1 are themselves out of Microsoft support. Compatibility is provided for legacy school PCs, but the OS should still be fully patched.

## Client deployment goal

The client receives only:

`RajasvSchoolSetup.exe`

The setup bundles the .NET Framework 4.8 offline runtime when you build the installer. LiteDB and Newtonsoft.Json are normal application DLLs inside the same setup; no database server is installed.

## Included functionality

- Dashboard
- Academic sessions
- Classes / sections
- Student admission CRUD
- Teacher CRUD
- Daily class-wise attendance
- Fee heads
- Fee structures
- Common class fee generation
- Individual student charges
- Pending / paid / partial / overdue fee records
- Partial fee payments
- Payment receipts + Windows print preview
- Fee / collection / attendance reports
- CSV export (opens in Excel / LibreOffice)
- Local LiteDB database
- Daily automatic backup (30 rolling automatic backups)
- Manual backup and restore
- Migration from the old JSON `SchoolData` folder
- Per-user writable data directory (not Program Files)

## Data location on the client PC

`%LOCALAPPDATA%\Rajasv Infotech\School Management\`

Important files:

- `Data\school.db` - live database
- `Backups\` - automatic/manual backups
- `Exports\` - CSV exports

The setup updates application files without overwriting `school.db`.

## Developer requirements (free)

1. Windows 10/11 development PC
2. Visual Studio 2022 Community **or** .NET SDK + .NET Framework 4.8 Developer Pack
3. NSIS 3.x (Nullsoft Scriptable Install System)
4. Internet only once on the developer PC to restore NuGet packages and download the .NET Framework offline redistributable

No paid hosting, DB license, server, or subscription is needed.

## Quick build

Open PowerShell in the project folder:

```powershell
.\Download-Prerequisites.ps1
.\build-release.ps1
```

The final client file is:

`artifacts\installer\RajasvSchoolSetup.exe`

See `Docs\SETUP_GUIDE.md` for complete steps and test checklist.

## Hindi build guide

For a step-by-step Hindi setup/build guide, open:

`Docs\SETUP_GUIDE_HINDI.md`
