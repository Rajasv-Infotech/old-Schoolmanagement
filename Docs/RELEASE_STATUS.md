# Release status / verification note

This source package is a full architectural rewrite of the original MAUI/Blazor project into WinForms + .NET Framework 4.8 + LiteDB, including database migration, backup/restore, fees/payments, attendance, reports, receipt printing, and a single-file NSIS installer definition.

The generation environment used to assemble this package is Linux and does not contain Windows/.NET Framework build tools, so the final Windows EXE/setup could not be compiled inside this environment. The source was statically checked for file completeness/basic structural issues, but **you must build and perform the clean-PC checklist in `Docs/SETUP_GUIDE.md` before delivering production school data**.

The Windows release build itself is automated by `build-release.ps1`.
