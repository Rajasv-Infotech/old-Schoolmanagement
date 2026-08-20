# Windows Setup banane ka Hindi guide

Is project ko school client ke liye ek hi offline installer file me package kiya gaya hai:

`RajasvSchoolSetup.exe`

Client ko LiteDB, WebView2, Windows App Runtime, SQL Server ya MySQL alag se install nahi karna hai. Agar .NET Framework 4.8 missing ho, final setup uska official offline installer andar se run karega.

## 1. Developer PC par free tools install karein

Ye tools sirf aapke build wale PC par chahiye, client PC par nahi:

1. Visual Studio 2022 Community install karein.
2. Visual Studio Installer me `.NET desktop development` workload select karein.
3. `.NET Framework 4.8 targeting pack/developer pack` selected hona chahiye.
4. NSIS 3.x install karein.

## 2. Project open aur test karein

Visual Studio me ye file open karein:

`RajasvSchoolManagement.sln`

Configuration:

- `Release`
- `Any CPU`

Pehle `Build > Rebuild Solution` chalayein. NuGet automatically LiteDB aur Newtonsoft.Json restore karega.

## 3. Bundled .NET Framework installer download karein

Project root me ye file double-click karein:

`1-Download-Prerequisites.cmd`

Ya PowerShell me:

```powershell
.\Download-Prerequisites.ps1
```

Successful hone ke baad ye official Microsoft file available hogi:

`Installer\Prerequisites\ndp48-x86-x64-allos-enu.exe`

Ye file final setup ke andar embed hogi; client ko alag se nahi deni.

## 4. Single setup EXE banayein

Project root me:

`2-Build-Setup.cmd`

double-click karein.

Ya PowerShell me:

```powershell
.\build-release.ps1
```

Final client file:

`artifacts\installer\RajasvSchoolSetup.exe`

Client ko sirf isi ek file ko copy/share karna hai.

## 5. Client PC minimum requirement

Supported target:

- Windows 7 SP1, fully patched: legacy/best-effort
- Windows 8.1: legacy/best-effort
- Windows 10
- Windows 11
- 32-bit aur 64-bit

Windows XP, Vista aur Windows 7 without SP1 supported nahi hain.

Bahut purane/unpatched Windows 7 me Microsoft ke SHA-2 ya servicing updates missing hon to bundled .NET installer bhi fail ho sakta hai. Aisi machine ko pehle Windows Update/required Microsoft updates dene padenge.

## 6. School me first-use order

1. `Settings / Backup` me school name, address, phone aur receipt prefix set karein.
2. `Academic Sessions` me current session create karein.
3. `Classes & Sections` me classes create karein.
4. `Students` me admissions add/import karein.
5. `Teachers` me teacher data add karein.
6. `Fee Setup` me fee heads aur common class fees create karein.
7. `Fee Records & Payments` me payment receive aur receipt print karein.
8. `Attendance` me daily attendance mark karein.
9. `Settings / Backup` se manual backup bana kar pen drive par copy karein.

## 7. Purane JSON data ko import karna

New app me:

`Settings / Backup > Import Old SchoolData`

Old folder select karein jisme files hon:

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

Data LiteDB ki single file `school.db` me upsert hoga.

## 8. Client data kahan save hoga

```text
%LOCALAPPDATA%\Rajasv Infotech\School Management\
```

Important folders:

```text
Data\school.db
Backups\
Exports\
Logs\
```

Application uninstall/reinstall karne par program directory remove hoti hai, lekin LocalAppData wala school database preserve kiya gaya hai.

## 9. Delivery se pehle compulsory test

Ek clean Windows PC/VM par, internet disconnect karke test karein:

1. Sirf `RajasvSchoolSetup.exe` se install.
2. App open.
3. Session, class aur student create.
4. App close/reopen; data present hona chahiye.
5. Fee create.
6. Partial payment receive.
7. Remaining payment receive; status `Paid` hona chahiye.
8. Receipt print preview.
9. Attendance save.
10. Manual backup create.
11. Kuch test data change karke backup restore.
12. App uninstall/reinstall karke data behavior verify.

## Important release note

Source complete diya gaya hai, lekin final Windows EXE/setup ko Windows build machine par compile aur clean-PC test karna zaroori hai. Production school data dene se pehle kam se kam ek Windows 7 SP1 machine (agar support claim kar rahe hain), ek Windows 10 aur ek Windows 11 machine/VM par test karein.
