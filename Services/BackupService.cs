using System;
using System.IO;
using System.Linq;
using RajasvSchoolManagement.Data;

namespace RajasvSchoolManagement.Services
{
    public class BackupService
    {
        private readonly DatabaseService _database;

        public BackupService(DatabaseService database)
        {
            _database = database;
        }

        public string CreateBackup(string label)
        {
            DataPaths.EnsureDirectories();
            if (!File.Exists(DataPaths.DatabaseFile))
            {
                return string.Empty;
            }

            string safeLabel = string.IsNullOrWhiteSpace(label) ? "manual" : label.Replace(" ", "-");
            string path = Path.Combine(
                DataPaths.BackupDirectory,
                string.Format("school-{0}-{1:yyyyMMdd-HHmmss}.db", safeLabel, DateTime.Now));

            _database.Close();
            try
            {
                File.Copy(DataPaths.DatabaseFile, path, false);
            }
            finally
            {
                _database.Reopen();
            }

            return path;
        }

        public void TryDailyBackup()
        {
            try
            {
                if (!File.Exists(DataPaths.DatabaseFile)) return;

                string todayPrefix = "school-auto-" + DateTime.Today.ToString("yyyyMMdd");
                bool alreadyDone = Directory.GetFiles(DataPaths.BackupDirectory, todayPrefix + "*.db").Any();
                if (!alreadyDone)
                {
                    CreateBackup("auto");
                }
                DeleteOldAutomaticBackups(30);
            }
            catch
            {
                // A backup failure must never prevent the application from starting.
            }
        }

        public void RestoreBackup(string backupFile)
        {
            if (string.IsNullOrWhiteSpace(backupFile) || !File.Exists(backupFile))
                throw new FileNotFoundException("Backup file not found.", backupFile);

            string safetyCopy = string.Empty;
            if (File.Exists(DataPaths.DatabaseFile))
            {
                safetyCopy = CreateBackup("before-restore");
            }

            _database.Close();
            try
            {
                string logFile = DataPaths.DatabaseFile + "-log";
                if (File.Exists(logFile)) File.Delete(logFile);
                File.Copy(backupFile, DataPaths.DatabaseFile, true);
            }
            catch
            {
                if (!string.IsNullOrEmpty(safetyCopy) && File.Exists(safetyCopy))
                {
                    File.Copy(safetyCopy, DataPaths.DatabaseFile, true);
                }
                throw;
            }
            finally
            {
                _database.Reopen();
            }
        }

        private void DeleteOldAutomaticBackups(int keepCount)
        {
            var files = new DirectoryInfo(DataPaths.BackupDirectory)
                .GetFiles("school-auto-*.db")
                .OrderByDescending(f => f.CreationTimeUtc)
                .Skip(keepCount)
                .ToList();

            foreach (FileInfo file in files)
            {
                try { file.Delete(); } catch { }
            }
        }
    }
}
