; Rajasv Infotech School Management - single-file offline installer
; Build with NSIS 3.x (makensis.exe). NSIS is free/open source.

Unicode true
RequestExecutionLevel admin
SetCompressor /SOLID lzma
SetCompressorDictSize 32
CRCCheck on
XPStyle on
ShowInstDetails show
ShowUninstDetails show

!include "MUI2.nsh"
!include "LogicLib.nsh"
!include "WinVer.nsh"
!include "x64.nsh"

; Make all compile-time paths deterministic, independent of where makensis.exe is launched.
!cd "${__FILEDIR__}\.."

!define APP_NAME "Rajasv Infotech School Management"
!define APP_VERSION "2.0.2"
!define APP_PUBLISHER "Rajasv Infotech"
!define APP_EXE "RajasvSchoolManagement.exe"
!define APP_DIR "Rajasv Infotech\School Management"
!define DOTNET_INSTALLER "ndp48-x86-x64-allos-enu.exe"
!define UNINSTALL_KEY "Software\Microsoft\Windows\CurrentVersion\Uninstall\RajasvSchoolManagement"

Name "${APP_NAME}"
Caption "${APP_NAME} Setup"
BrandingText "Rajasv Infotech"
OutFile "artifacts\installer\RajasvSchoolSetup.exe"
InstallDir "$PROGRAMFILES\${APP_DIR}"
InstallDirRegKey HKLM "${UNINSTALL_KEY}" "InstallLocation"

VIProductVersion "2.0.2.0"
VIAddVersionKey /LANG=1033 "ProductName" "${APP_NAME}"
VIAddVersionKey /LANG=1033 "CompanyName" "${APP_PUBLISHER}"
VIAddVersionKey /LANG=1033 "FileDescription" "${APP_NAME} Setup"
VIAddVersionKey /LANG=1033 "FileVersion" "2.0.2.0"
VIAddVersionKey /LANG=1033 "ProductVersion" "${APP_VERSION}"
VIAddVersionKey /LANG=1033 "LegalCopyright" "Copyright (c) Rajasv Infotech"

!define MUI_ABORTWARNING
!define MUI_ICON "${NSISDIR}\Contrib\Graphics\Icons\modern-install.ico"
!define MUI_UNICON "${NSISDIR}\Contrib\Graphics\Icons\modern-uninstall.ico"

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!define MUI_FINISHPAGE_NOAUTOCLOSE
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES

!insertmacro MUI_LANGUAGE "English"

; Compile-time paths below are project-root relative because of !cd above.

Function .onInit
    ${IfNot} ${AtLeastWin7}
        MessageBox MB_ICONSTOP|MB_OK "Windows 7 SP1 or newer is required.$\r$\n$\r$\nWindows XP and Vista are not supported."
        Abort
    ${EndIf}

    ; .NET Framework 4.8 requires Windows 7 SP1. WinVer.nsh provides the
    ; service-pack macro used here, so Windows 7 RTM gets a clear message.
    ${If} ${IsWin7}
    ${AndIfNot} ${AtLeastServicePack} 1
        MessageBox MB_ICONSTOP|MB_OK "Windows 7 Service Pack 1 is required.$\r$\n$\r$\nPlease install SP1 before installing this application."
        Abort
    ${EndIf}
FunctionEnd

; Sets $R0 to "1" when .NET Framework 4.8 or newer is installed, otherwise "0".
Function CheckDotNet48
    StrCpy $R0 "0"

    SetRegView 32
    ClearErrors
    ReadRegDWORD $0 HKLM "SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" "Release"
    IfErrors check_dotnet_64
    IntCmp $0 528040 dotnet_found check_dotnet_64 dotnet_found

check_dotnet_64:
    ${If} ${RunningX64}
        SetRegView 64
        ClearErrors
        ReadRegDWORD $0 HKLM "SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full" "Release"
        IfErrors dotnet_done
        IntCmp $0 528040 dotnet_found dotnet_done dotnet_found
    ${EndIf}
    Goto dotnet_done

dotnet_found:
    StrCpy $R0 "1"

dotnet_done:
    SetRegView 32
FunctionEnd

Section "Application" SecMain
    SectionIn RO

    Call CheckDotNet48
    ${If} $R0 != "1"
        DetailPrint "Microsoft .NET Framework 4.8 not found. Installing bundled offline runtime..."
        SetOutPath "$TEMP\RajasvSchoolPrerequisite"
        File /oname=${DOTNET_INSTALLER} "Installer\Prerequisites\${DOTNET_INSTALLER}"

        ExecWait '"$TEMP\RajasvSchoolPrerequisite\${DOTNET_INSTALLER}" /q /norestart' $0

        Delete "$TEMP\RajasvSchoolPrerequisite\${DOTNET_INSTALLER}"
        RMDir "$TEMP\RajasvSchoolPrerequisite"

        ${If} $0 == 3010
            DetailPrint ".NET Framework 4.8 installed. A reboot is required."
            SetRebootFlag true
        ${ElseIf} $0 == 1641
            DetailPrint ".NET Framework 4.8 installed and Windows requested a reboot."
            SetRebootFlag true
        ${ElseIf} $0 != 0
            MessageBox MB_ICONSTOP|MB_OK "Microsoft .NET Framework 4.8 installation failed with exit code $0.$\r$\n$\r$\nThe PC must be Windows 7 SP1 or newer. On very old Windows 7 installations, required Windows servicing/SHA-2 updates may also be needed."
            Abort
        ${EndIf}
    ${Else}
        DetailPrint "Microsoft .NET Framework 4.8 or newer is already installed."
    ${EndIf}

    SetOutPath "$INSTDIR"
    File /r "artifacts\app\*.*"

    WriteUninstaller "$INSTDIR\Uninstall.exe"

    WriteRegStr HKLM "${UNINSTALL_KEY}" "DisplayName" "${APP_NAME}"
    WriteRegStr HKLM "${UNINSTALL_KEY}" "DisplayVersion" "${APP_VERSION}"
    WriteRegStr HKLM "${UNINSTALL_KEY}" "Publisher" "${APP_PUBLISHER}"
    WriteRegStr HKLM "${UNINSTALL_KEY}" "InstallLocation" "$INSTDIR"
    WriteRegStr HKLM "${UNINSTALL_KEY}" "DisplayIcon" "$INSTDIR\${APP_EXE}"
    WriteRegStr HKLM "${UNINSTALL_KEY}" "UninstallString" '"$INSTDIR\Uninstall.exe"'
    WriteRegDWORD HKLM "${UNINSTALL_KEY}" "NoModify" 1
    WriteRegDWORD HKLM "${UNINSTALL_KEY}" "NoRepair" 1

    ; Setup is elevated and installs per-machine, so create all-users shortcuts.
    ; This avoids the old "C:\Users\Public\Desktop ... Access is denied" issue.
    SetShellVarContext all
    CreateDirectory "$SMPROGRAMS\Rajasv Infotech"
    CreateShortcut "$SMPROGRAMS\Rajasv Infotech\School Management.lnk" "$INSTDIR\${APP_EXE}" "" "$INSTDIR\${APP_EXE}" 0
    CreateShortcut "$DESKTOP\Rajasv Infotech School Management.lnk" "$INSTDIR\${APP_EXE}" "" "$INSTDIR\${APP_EXE}" 0

    SetAutoClose false
SectionEnd

Section "Uninstall"
    SetShellVarContext all

    Delete "$DESKTOP\Rajasv Infotech School Management.lnk"
    Delete "$SMPROGRAMS\Rajasv Infotech\School Management.lnk"
    RMDir "$SMPROGRAMS\Rajasv Infotech"

    DeleteRegKey HKLM "${UNINSTALL_KEY}"

    ; Only remove installed program files. User/school data intentionally lives under
    ; %LOCALAPPDATA%\Rajasv Infotech\School Management and is preserved on uninstall.
    RMDir /r "$INSTDIR"
SectionEnd
