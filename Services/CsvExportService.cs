using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using RajasvSchoolManagement.Data;

namespace RajasvSchoolManagement.Services
{
    public static class CsvExportService
    {
        public static string Export<T>(IEnumerable<T> rows, string baseFileName, params string[] excludedProperties)
        {
            DataPaths.EnsureDirectories();
            string file = Path.Combine(
                DataPaths.ExportDirectory,
                string.Format("{0}-{1:yyyyMMdd-HHmmss}.csv", baseFileName, DateTime.Now));

            HashSet<string> excluded = new HashSet<string>(excludedProperties ?? new string[0], StringComparer.OrdinalIgnoreCase);
            PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Where(p => p.CanRead && !excluded.Contains(p.Name))
                .ToArray();

            using (StreamWriter writer = new StreamWriter(file, false, new UTF8Encoding(true)))
            {
                writer.WriteLine(string.Join(",", properties.Select(p => Escape(p.Name))));
                foreach (T row in rows)
                {
                    writer.WriteLine(string.Join(",", properties.Select(p => Escape(FormatValue(p.GetValue(row, null))))));
                }
            }
            return file;
        }

        private static string FormatValue(object value)
        {
            if (value == null) return string.Empty;
            if (value is DateTime) return ((DateTime)value).ToString("yyyy-MM-dd HH:mm");
            if (value is decimal) return ((decimal)value).ToString("0.00");
            return Convert.ToString(value);
        }

        private static string Escape(string value)
        {
            value = value ?? string.Empty;
            if (value.Contains("\"") || value.Contains(",") || value.Contains("\r") || value.Contains("\n"))
            {
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            }
            return value;
        }
    }
}
