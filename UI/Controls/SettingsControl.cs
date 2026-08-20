using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using RajasvSchoolManagement.Data;
using RajasvSchoolManagement.Models;
using RajasvSchoolManagement.Services;
using RajasvSchoolManagement.UI;

namespace RajasvSchoolManagement.UI.Controls
{
    internal class SettingsControl : UserControl
    {
        private readonly DatabaseService _database;
        private readonly BackupService _backup;
        private readonly Repository<SchoolSettings> _settings;
        private readonly TextBox _schoolName, _address, _phone, _email, _receiptPrefix, _receivedBy;
        private readonly Label _stats;

        public SettingsControl(DatabaseService database, BackupService backup)
        {
            _database = database; _backup = backup; _settings = new Repository<SchoolSettings>(database, "settings");
            Dock = DockStyle.Fill; Font = Ui.DefaultFont; AutoScroll = true;
            Controls.Add(Ui.Header("Settings & Data Safety", "School profile, automatic local backups, restore and migration from the old JSON app"));

            Panel body = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(16) };
            GroupBox school = new GroupBox { Text = "School Profile", Dock = DockStyle.Top, Height = 245, Padding = new Padding(12) };
            TableLayoutPanel fields = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 7 }; fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150)); fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            SchoolSettings current = GetSettings();
            _schoolName = AddField(fields, 0, "School name", current.SchoolName); _address = AddField(fields, 1, "Address", current.Address); _phone = AddField(fields, 2, "Phone", current.Phone); _email = AddField(fields, 3, "Email", current.Email); _receiptPrefix = AddField(fields, 4, "Receipt prefix", current.ReceiptPrefix); _receivedBy = AddField(fields, 5, "Default received by", current.ReceivedBy);
            Button save = Ui.Button("Save School Profile", delegate { SaveProfile(); }, 150); fields.Controls.Add(save, 1, 6); school.Controls.Add(fields); body.Controls.Add(school);

            GroupBox data = new GroupBox { Text = "Database & Backups", Dock = DockStyle.Top, Height = 235, Padding = new Padding(12) };
            FlowLayoutPanel actions = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(8), AutoScroll = true };
            actions.Controls.Add(Ui.Button("Backup Now", delegate { BackupNow(); }, 120)); actions.Controls.Add(Ui.Button("Restore Backup", delegate { Restore(); }, 120)); actions.Controls.Add(Ui.Button("Open Data Folder", delegate { OpenFolder(DataPaths.RootDirectory); }, 130)); actions.Controls.Add(Ui.Button("Open Backup Folder", delegate { OpenFolder(DataPaths.BackupDirectory); }, 140));
            actions.SetFlowBreak(actions.Controls[actions.Controls.Count - 1], true);
            Label path = new Label { Text = "Database: " + DataPaths.DatabaseFile, AutoSize = true, MaximumSize = new System.Drawing.Size(800, 0), Padding = new Padding(5, 12, 5, 5) }; actions.Controls.Add(path); actions.SetFlowBreak(path, true);
            _stats = new Label { AutoSize = true, MaximumSize = new System.Drawing.Size(800, 0), Padding = new Padding(5) }; actions.Controls.Add(_stats); data.Controls.Add(actions); body.Controls.Add(data); data.BringToFront();

            GroupBox migration = new GroupBox { Text = "Import Data From Old MAUI JSON Version", Dock = DockStyle.Top, Height = 155, Padding = new Padding(12) };
            FlowLayoutPanel mig = new FlowLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(8) };
            mig.Controls.Add(new Label { Text = "Select the old SchoolData folder containing students.json, fees.json, teachers.json, etc. Existing IDs are upserted, so this can be rerun safely.", Width = 780, Height = 45 }); mig.SetFlowBreak(mig.Controls[mig.Controls.Count - 1], true);
            mig.Controls.Add(Ui.Button("Import Old SchoolData", delegate { ImportJson(); }, 170)); migration.Controls.Add(mig); body.Controls.Add(migration); migration.BringToFront();

            Controls.Add(body); body.BringToFront(); RefreshStats();
        }

        private static TextBox AddField(TableLayoutPanel panel, int row, string label, string value)
        {
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 31)); Label l = new Label { Text = label, Dock = DockStyle.Fill, TextAlign = System.Drawing.ContentAlignment.MiddleLeft }; TextBox box = new TextBox { Text = value ?? string.Empty, Dock = DockStyle.Top, Margin = new Padding(3, 4, 3, 3) }; panel.Controls.Add(l, 0, row); panel.Controls.Add(box, 1, row); return box;
        }
        private SchoolSettings GetSettings() { return _settings.FindById("default") ?? new SchoolSettings(); }
        private void SaveProfile()
        {
            if (string.IsNullOrWhiteSpace(_schoolName.Text)) { Ui.Error("School name is required."); return; }
            SchoolSettings s = GetSettings(); s.SchoolName = _schoolName.Text.Trim(); s.Address = _address.Text.Trim(); s.Phone = _phone.Text.Trim(); s.Email = _email.Text.Trim(); s.ReceiptPrefix = string.IsNullOrWhiteSpace(_receiptPrefix.Text) ? "RCPT" : _receiptPrefix.Text.Trim(); s.ReceivedBy = _receivedBy.Text.Trim(); _settings.Upsert(s); Ui.Info("School profile saved.");
        }
        private void BackupNow()
        {
            try { string file = _backup.CreateBackup("manual"); if (string.IsNullOrEmpty(file)) { Ui.Info("Database is new; there is nothing to back up yet."); return; } RefreshStats(); Ui.Info("Backup created:\r\n" + file); }
            catch (Exception ex) { Ui.Error("Backup failed: " + ex.Message); }
        }
        private void Restore()
        {
            using (OpenFileDialog dialog = new OpenFileDialog { Filter = "School database backup (*.db)|*.db|All files (*.*)|*.*", InitialDirectory = DataPaths.BackupDirectory })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return; if (!Ui.Confirm("Restore this backup?\r\n\r\nA safety copy of the current database will be created first. The app data will be replaced by the selected backup.")) return;
                try { _backup.RestoreBackup(dialog.FileName); SeedService.EnsureDefaults(_database); RefreshStats(); Ui.Info("Backup restored successfully. Navigate away and back, or restart the app, to refresh all screens."); } catch (Exception ex) { Ui.Error("Restore failed: " + ex.Message); }
            }
        }
        private void ImportJson()
        {
            using (FolderBrowserDialog dialog = new FolderBrowserDialog { Description = "Select the old SchoolData folder" })
            {
                if (dialog.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    string safety = _backup.CreateBackup("before-json-import"); JsonMigrationService migration = new JsonMigrationService(_database); MigrationResult result = migration.ImportFolder(dialog.SelectedPath); RefreshStats(); string warnings = result.Warnings.Count == 0 ? "None" : string.Join("\r\n", result.Warnings.Take(10)); Ui.Info("Import completed.\r\n\r\nImported/upserted records: " + result.Total + "\r\nWarnings: " + warnings + (string.IsNullOrEmpty(safety) ? "" : "\r\n\r\nPre-import backup: " + safety));
                }
                catch (Exception ex) { Ui.Error("Import failed: " + ex.Message); }
            }
        }
        private void RefreshStats()
        {
            try
            {
                long size = File.Exists(DataPaths.DatabaseFile) ? new FileInfo(DataPaths.DatabaseFile).Length : 0; int backups = Directory.Exists(DataPaths.BackupDirectory) ? Directory.GetFiles(DataPaths.BackupDirectory, "*.db").Length : 0;
                _stats.Text = "Database size: " + FormatBytes(size) + "    |    Students: " + _database.GetCollection<Student>("students").Count() + "    |    Fee records: " + _database.GetCollection<FeeRecord>("feerecords").Count() + "    |    Receipts: " + _database.GetCollection<FeePaymentReceipt>("receipts").Count() + "    |    Backups: " + backups;
            }
            catch { _stats.Text = "Statistics unavailable."; }
        }
        private static string FormatBytes(long bytes) { if (bytes < 1024) return bytes + " B"; if (bytes < 1024 * 1024) return (bytes / 1024d).ToString("0.0") + " KB"; return (bytes / 1024d / 1024d).ToString("0.0") + " MB"; }
        private static void OpenFolder(string folder) { try { Directory.CreateDirectory(folder); Process.Start(new ProcessStartInfo("explorer.exe", "\"" + folder + "\"") { UseShellExecute = true }); } catch (Exception ex) { Ui.Error(ex.Message); } }
    }
}
