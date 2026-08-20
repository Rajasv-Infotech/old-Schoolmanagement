using System;
using System.Threading;
using System.Windows.Forms;
using RajasvSchoolManagement.Data;
using RajasvSchoolManagement.Services;
using RajasvSchoolManagement.UI;

namespace RajasvSchoolManagement
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            Application.ThreadException += delegate(object sender, ThreadExceptionEventArgs e)
            {
                ShowFatalError(e.Exception, "Windows Forms thread exception");
            };

            AppDomain.CurrentDomain.UnhandledException += delegate(object sender, UnhandledExceptionEventArgs e)
            {
                Exception ex = e.ExceptionObject as Exception ?? new Exception(Convert.ToString(e.ExceptionObject));
                ErrorLogService.Log(ex, "Unhandled AppDomain exception");
            };

            DatabaseService database = null;
            try
            {
                DataPaths.EnsureDirectories();
                database = new DatabaseService();
                BackupService backup = new BackupService(database);
                backup.TryDailyBackup();
                SeedService.EnsureDefaults(database);

                Application.Run(new MainForm(database, backup));
            }
            catch (Exception ex)
            {
                ShowFatalError(ex, "Application startup/runtime");
            }
            finally
            {
                if (database != null) database.Dispose();
            }
        }

        private static void ShowFatalError(Exception ex, string context)
        {
            string log = ErrorLogService.Log(ex, context);
            string details = ex == null ? "Unknown error." : ex.Message;
            if (!string.IsNullOrWhiteSpace(log))
            {
                details += "\r\n\r\nError log:\r\n" + log;
            }

            MessageBox.Show(
                "An unexpected error occurred.\r\n\r\n" + details,
                "Rajasv School Management",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
