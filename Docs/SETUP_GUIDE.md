# Build and Setup Guide

## 1. Install developer tools

On **your development PC** (not on the school client PC):

1. Install **Visual Studio 2022 Community**.
2. In Visual Studio Installer enable **.NET desktop development**.
3. Ensure **.NET Framework 4.8 targeting/developer pack** is present.
4. Install **NSIS 3.x** (Nullsoft Scriptable Install System).

All of these can be used without a paid license for this type of small development workflow (check the applicable Visual Studio Community terms for your organization/team size).

## 2. Open and build the project

Open:

`RajasvSchoolManagement.sln`

Select `Release` + `Any CPU` and build once.

The project uses NuGet packages:

- LiteDB 5.0.21
- Newtonsoft.Json 13.0.3

They are restored on the developer computer and copied beside the application EXE. The school computer does not use NuGet.

## 3. Download the one prerequisite that gets embedded into setup

Run from PowerShell:

```powershell
.\Download-Prerequisites.ps1
```

This downloads Microsoft's official .NET Framework 4.8 **offline installer** into:

`Installer\Prerequisites\ndp48-x86-x64-allos-enu.exe`

This is a developer/build-time step. The final school user will **not** receive this as a separate file; NSIS packs it into `RajasvSchoolSetup.exe`.

If PowerShell downloading is blocked, download the .NET Framework 4.8 offline Runtime from Microsoft's .NET Framework 4.8 download page and rename/copy it to the exact filename above.

## 4. Build the final one-file installer

Run:

```powershell
.\build-release.ps1
```

The script performs:

1. `dotnet restore`
2. Release build into `artifacts\app`
3. Checks the .NET Framework 4.8 offline installer exists
4. Finds `makensis.exe` from NSIS 3.x
5. Compiles the installer

Final output:

`artifacts\installer\RajasvSchoolSetup.exe`

**This is the only file you send to the client.**

## 5. What happens on the client PC

When `RajasvSchoolSetup.exe` starts:

- Requires Windows 7 SP1 or newer.
- Requests Administrator permission because an old PC may need .NET Framework installed.
- Checks the registry for .NET Framework 4.8 or newer.
- If already available, it skips the framework installation.
- If missing, it silently runs the bundled Microsoft offline installer.
- Installs the app files.
- Creates Start Menu + current-user Desktop shortcuts.
- Does **not** install WebView2.
- Does **not** install Windows App SDK.
- Does **not** install SQL Server/MySQL.
- Does **not** require internet.

If .NET Framework installation requests a reboot (exit code 3010/1641), reboot the computer before first use.

## 6. Old Windows notes

The app targets .NET Framework 4.8 because Microsoft provides it for Windows 7 SP1, Windows 8.1 and modern Windows.

For Windows 7:

- It must be **Windows 7 Service Pack 1**.
- It should have Windows updates including SHA-2/code-signing support.
- A severely unpatched Windows 7 install may fail to install modern Microsoft redistributables even when they are bundled offline.

There is no safe way to promise one modern application will run on every unpatched Windows installation from XP through Windows 11. This project intentionally draws the line at Windows 7 SP1.

## 7. First-use workflow at school

Recommended order:

1. Settings / Backup -> set school name/address/phone.
2. Academic Sessions -> create current session and keep it Active.
3. Classes & Sections -> create classes for that session.
4. Students -> admit/import students.
5. Teachers -> enter teacher records.
6. Fee Setup -> create Fee Heads.
7. Fee Setup -> create Common Class Fees or use individual fees.
8. Fee Records & Payments -> receive payments and print receipts.
9. Attendance -> mark daily attendance.
10. Settings / Backup -> create a manual backup and copy it to a pen drive periodically.

## 8. Migrating data from the old MAUI JSON app

In the new app open:

`Settings / Backup -> Import Old SchoolData`

Select the folder containing files such as:

- `students.json`
- `teachers.json`
- `sessions.json`
- `classsections.json`
- `fees.json`
- `feeheads.json`
- `commonfeerules.json`
- `feepaymentreceipts.json`
- `attendance.json`
- `feestructure.json`

The importer uses the existing string IDs and upserts records into LiteDB. A pre-import backup is created when a database already exists.

## 9. Database and backups

Live DB:

`%LOCALAPPDATA%\Rajasv Infotech\School Management\Data\school.db`

Automatic backups:

`%LOCALAPPDATA%\Rajasv Infotech\School Management\Backups\`

The application keeps approximately the latest 30 automatic backups plus manual backups.

For real school data, local backups alone are not enough against laptop theft/disk failure. Use a free physical backup routine: copy the latest `.db` backup to a pen drive/external disk at least weekly.

## 10. Clean-PC release test (important)

Before delivering to a school, test the generated `RajasvSchoolSetup.exe` on a PC/VM that does **not** have Visual Studio installed.

Test:

- Install from the single setup file while internet is disconnected.
- Start app.
- Create session/class/student.
- Close and reopen; data must remain.
- Create fee and receive partial payment.
- Receive remaining payment; status must become Paid.
- Print-preview receipt.
- Mark attendance.
- Create manual backup.
- Restore that backup.
- Uninstall/reinstall app and confirm the LocalAppData database behavior you want.
- Test on the oldest Windows version you intend to support.

## 11. Updating the application later

Increment version in:

- `RajasvSchoolManagement.csproj`
- `Installer\SchoolManagementSetup.iss`

Build a new setup with the same uninstall/installation registry key and install directory. NSIS upgrades the application files in place. Database files live in LocalAppData and are outside the install directory.
