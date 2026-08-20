This folder intentionally does not contain Microsoft's large redistributable binary.

Before compiling the final installer, place this official Microsoft file here:

  ndp48-x86-x64-allos-enu.exe

Official .NET Framework 4.8 offline installer redirect:
  https://go.microsoft.com/fwlink/?linkid=2088631

Or run the project-root script:
  .\Download-Prerequisites.ps1

The NSIS script embeds this file inside RajasvSchoolSetup.exe.
The SCHOOL CLIENT does not need to download or install it separately.
