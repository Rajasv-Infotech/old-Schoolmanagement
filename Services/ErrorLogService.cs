using System;
using System.IO;
using System.Text;
using RajasvSchoolManagement.Data;

namespace RajasvSchoolManagement.Services
{
    public static class ErrorLogService
    {
        public static string Log(Exception exception, string context)
        {
            try
            {
                DataPaths.EnsureDirectories();
                string file = Path.Combine(DataPaths.LogDirectory, "app-" + DateTime.Today.ToString("yyyyMMdd") + ".log");
                StringBuilder text = new StringBuilder();
                text.AppendLine(new string('-', 80));
                text.AppendLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                text.AppendLine("Context: " + (context ?? string.Empty));
                text.AppendLine("Windows: " + Environment.OSVersion);
                text.AppendLine("64-bit OS: " + Environment.Is64BitOperatingSystem);
                text.AppendLine("64-bit process: " + Environment.Is64BitProcess);
                text.AppendLine(exception == null ? "Unknown error" : exception.ToString());
                File.AppendAllText(file, text.ToString(), Encoding.UTF8);
                return file;
            }
            catch
            {
                return string.Empty;
            }
        }
    }
}
