using System;
using System.IO;

namespace RajasvSchoolManagement.Data
{
    public static class DataPaths
    {
        public static readonly string RootDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Rajasv Infotech",
            "School Management");

        public static readonly string DatabaseFile = Path.Combine(RootDirectory, "Data", "school.db");
        public static readonly string BackupDirectory = Path.Combine(RootDirectory, "Backups");
        public static readonly string ExportDirectory = Path.Combine(RootDirectory, "Exports");
        public static readonly string LogDirectory = Path.Combine(RootDirectory, "Logs");
        public static readonly string MigrationDirectory = Path.Combine(RootDirectory, "MigratedJson");

        public static void EnsureDirectories()
        {
            Directory.CreateDirectory(RootDirectory);
            Directory.CreateDirectory(Path.GetDirectoryName(DatabaseFile));
            Directory.CreateDirectory(BackupDirectory);
            Directory.CreateDirectory(ExportDirectory);
            Directory.CreateDirectory(LogDirectory);
            Directory.CreateDirectory(MigrationDirectory);
        }
    }
}
