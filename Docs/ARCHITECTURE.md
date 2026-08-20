# Architecture

## Desktop stack

- Windows Forms
- .NET Framework 4.8
- LiteDB embedded database
- Newtonsoft.Json only for one-time import of existing JSON files
- Native Windows printing (`PrintDocument` / `PrintPreviewDialog`)
- NSIS 3.x single-file offline installer

## No longer required

- .NET MAUI
- BlazorWebView
- WebView2 Runtime
- Windows App Runtime / Microsoft.WindowsAppRuntime.dll
- ASP.NET server
- SQL Server / MySQL
- Cloud database

## Collections inside school.db

- `settings`
- `sessions`
- `classsections`
- `students`
- `teachers`
- `attendance`
- `feeheads`
- `feestructures`
- `commonfeerules`
- `feerecords`
- `receipts`

## Accounting safety decisions

- Paid/partially-paid fee records cannot be deleted from the UI.
- Students with fee/attendance history cannot be deleted from the UI; mark them inactive.
- Academic sessions/classes in use cannot be deleted.
- Fee payments and fee-record updates are performed in one LiteDB transaction.
- Restore creates a safety backup first.
- Daily automatic backups are created when a database already exists.
