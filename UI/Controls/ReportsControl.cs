using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using RajasvSchoolManagement.Data;
using RajasvSchoolManagement.Models;
using RajasvSchoolManagement.Services;
using RajasvSchoolManagement.UI;

namespace RajasvSchoolManagement.UI.Controls
{
    internal class ReportsControl : UserControl
    {
        private readonly DatabaseService _database;
        private readonly FeeService _fees;
        private readonly DateTimePicker _from, _to;
        private readonly Label _summary;
        private readonly DataGridView _duesGrid, _collectionsGrid, _attendanceGrid;

        public ReportsControl(DatabaseService database)
        {
            _database = database; _fees = new FeeService(database); Dock = DockStyle.Fill; Font = Ui.DefaultFont;
            Controls.Add(Ui.Header("Reports", "Fee dues, collections and attendance reports. CSV exports open in Excel/LibreOffice."));
            FlowLayoutPanel filter = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 58, Padding = new Padding(8) };
            filter.Controls.Add(new Label { Text = "From:", AutoSize = true, Padding = new Padding(5, 8, 0, 0) }); _from = new DateTimePicker { Width = 120, Format = DateTimePickerFormat.Short, Value = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1), Margin = new Padding(5) }; filter.Controls.Add(_from);
            filter.Controls.Add(new Label { Text = "To:", AutoSize = true, Padding = new Padding(5, 8, 0, 0) }); _to = new DateTimePicker { Width = 120, Format = DateTimePickerFormat.Short, Value = DateTime.Today, Margin = new Padding(5) }; filter.Controls.Add(_to);
            filter.Controls.Add(Ui.Button("Refresh", delegate { LoadData(); }, 85)); filter.Controls.Add(Ui.Button("Export Current Tab", delegate { ExportCurrent(); }, 135)); Controls.Add(filter); filter.BringToFront();
            _summary = new Label { Dock = DockStyle.Top, Height = 42, Padding = new Padding(12, 8, 8, 8), Font = Ui.SubHeadingFont }; Controls.Add(_summary); _summary.BringToFront();

            TabControl tabs = new TabControl { Dock = DockStyle.Fill, Name = "reportTabs" };
            _duesGrid = AddTab(tabs, "Student Dues", new[] { new Col("Student", "Student", 140), new Col("Roll", "Roll", 55), new Col("Class", "Class", 70), new Col("Fee", "Fee Head", 100), new Col("Period", "Period", 70), new Col("DueDate", "Due Date", 80), new Col("Status", "Status", 60), new Col("Balance", "Balance", 75) });
            _collectionsGrid = AddTab(tabs, "Collections", new[] { new Col("Receipt", "Receipt No", 100), new Col("Date", "Date", 90), new Col("Student", "Student", 130), new Col("Class", "Class", 75), new Col("Mode", "Mode", 65), new Col("Amount", "Amount", 75) });
            _attendanceGrid = AddTab(tabs, "Attendance", new[] { new Col("Date", "Date", 80), new Col("Student", "Student", 140), new Col("Status", "Status", 70), new Col("Remarks", "Remarks", 160) });
            Controls.Add(tabs); tabs.BringToFront(); LoadData();
        }

        private DataGridView AddTab(TabControl tabs, string title, Col[] cols)
        {
            TabPage page = new TabPage(title); DataGridView grid = Ui.Grid(); foreach (Col c in cols) grid.Columns.Add(Ui.TextColumn(c.Property, c.Header, c.Weight)); Panel body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) }; body.Controls.Add(grid); page.Controls.Add(body); tabs.TabPages.Add(page); return grid;
        }
        private void LoadData()
        {
            try
            {
                _fees.RefreshOverdueStatuses(); DateTime from = _from.Value.Date; DateTime to = _to.Value.Date.AddDays(1).AddTicks(-1);
                var feeRecords = _database.GetCollection<FeeRecord>("feerecords").FindAll().ToList(); var receipts = _database.GetCollection<FeePaymentReceipt>("receipts").FindAll().Where(x => x.ReceiptDate >= from && x.ReceiptDate <= to).OrderByDescending(x => x.ReceiptDate).ToList();
                decimal outstanding = feeRecords.Sum(x => Math.Max(0, x.BalanceAmount)); decimal overdue = feeRecords.Where(x => x.Status == "Overdue").Sum(x => Math.Max(0, x.BalanceAmount)); decimal collected = receipts.Sum(x => x.ReceiptAmount);
                _summary.Text = "Outstanding: " + outstanding.ToString("0.00") + "    |    Overdue: " + overdue.ToString("0.00") + "    |    Collected in range: " + collected.ToString("0.00");
                _duesGrid.DataSource = feeRecords.Where(x => x.BalanceAmount > 0.009m).OrderBy(x => x.DueDate).Select(x => new DueRow { Student = x.StudentName, Roll = x.RollNumber, Class = x.ClassSectionName, Fee = x.FeeType, Period = x.FeeMonth, DueDate = x.DueDate.ToString("dd-MMM-yyyy"), Status = x.Status, Balance = x.BalanceAmount.ToString("0.00") }).ToList();
                _collectionsGrid.DataSource = receipts.Select(x => new CollectionRow { Receipt = x.ReceiptNumber, Date = x.ReceiptDate.ToString("dd-MMM-yyyy HH:mm"), Student = x.StudentName, Class = x.ClassSectionName, Mode = x.PaymentMethod, Amount = x.ReceiptAmount.ToString("0.00") }).ToList();
                _attendanceGrid.DataSource = _database.GetCollection<Attendance>("attendance").FindAll().Where(x => x.Date.Date >= from && x.Date.Date <= to.Date).OrderByDescending(x => x.Date).ThenBy(x => x.StudentName).Select(x => new AttendanceRow { Date = x.Date.ToString("dd-MMM-yyyy"), Student = x.StudentName, Status = x.Status, Remarks = x.Remarks }).ToList();
            }
            catch (Exception ex) { Ui.Error(ex.Message); }
        }
        private void ExportCurrent()
        {
            try
            {
                TabControl tabs = Controls.Find("reportTabs", true).FirstOrDefault() as TabControl; if (tabs == null) return; string file;
                if (tabs.SelectedIndex == 0) file = ExportGrid(_duesGrid, "student-dues"); else if (tabs.SelectedIndex == 1) file = ExportGrid(_collectionsGrid, "collections"); else file = ExportGrid(_attendanceGrid, "attendance-report");
                Ui.Info("Export created:\r\n" + file); try { Process.Start(new ProcessStartInfo(file) { UseShellExecute = true }); } catch { }
            }
            catch (Exception ex) { Ui.Error(ex.Message); }
        }
        private static string ExportGrid(DataGridView grid, string name)
        {
            var rows = grid.Rows.Cast<DataGridViewRow>().Where(r => !r.IsNewRow).Select(r => grid.Columns.Cast<DataGridViewColumn>().ToDictionary(c => c.HeaderText, c => Convert.ToString(r.Cells[c.Index].Value))).ToList();
            DataPaths.EnsureDirectories(); string path = System.IO.Path.Combine(DataPaths.ExportDirectory, name + "-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".csv");
            using (System.IO.StreamWriter writer = new System.IO.StreamWriter(path, false, new System.Text.UTF8Encoding(true)))
            {
                var headers = grid.Columns.Cast<DataGridViewColumn>().Select(c => c.HeaderText).ToList(); writer.WriteLine(string.Join(",", headers.Select(Escape)));
                foreach (var row in rows) writer.WriteLine(string.Join(",", headers.Select(h => Escape(row[h]))));
            }
            return path;
        }
        private static string Escape(string s) { s = s ?? string.Empty; return s.Contains(",") || s.Contains("\"") || s.Contains("\n") ? "\"" + s.Replace("\"", "\"\"") + "\"" : s; }
        private class Col { public string Property, Header; public int Weight; public Col(string p, string h, int w) { Property = p; Header = h; Weight = w; } }
        private class DueRow { public string Student { get; set; } public string Roll { get; set; } public string Class { get; set; } public string Fee { get; set; } public string Period { get; set; } public string DueDate { get; set; } public string Status { get; set; } public string Balance { get; set; } }
        private class CollectionRow { public string Receipt { get; set; } public string Date { get; set; } public string Student { get; set; } public string Class { get; set; } public string Mode { get; set; } public string Amount { get; set; } }
        private class AttendanceRow { public string Date { get; set; } public string Student { get; set; } public string Status { get; set; } public string Remarks { get; set; } }
    }
}
