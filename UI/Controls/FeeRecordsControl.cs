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
    internal class FeeRecordsControl : UserControl
    {
        private readonly DatabaseService _database;
        private readonly FeeService _feeService;
        private readonly Repository<FeeRecord> _records;
        private readonly Repository<FeePaymentReceipt> _receipts;
        private readonly DataGridView _recordGrid;
        private readonly DataGridView _receiptGrid;
        private readonly TextBox _search;
        private readonly ComboBox _status;

        public FeeRecordsControl(DatabaseService database)
        {
            _database = database; _feeService = new FeeService(database); _records = new Repository<FeeRecord>(database, "feerecords"); _receipts = new Repository<FeePaymentReceipt>(database, "receipts");
            Dock = DockStyle.Fill; Font = Ui.DefaultFont;
            Controls.Add(Ui.Header("Fee Records & Payments", "Create individual charges, receive payments and print receipts"));
            TabControl tabs = new TabControl { Dock = DockStyle.Fill };

            TabPage recordsPage = new TabPage("Fee Records");
            FlowLayoutPanel bar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 88, Padding = new Padding(8), AutoScroll = true };
            bar.Controls.Add(Ui.Button("Add Student Fee", delegate { AddStudentFee(); }, 125)); bar.Controls.Add(Ui.Button("Receive Payment", delegate { ReceivePayment(); }, 125)); bar.Controls.Add(Ui.Button("Delete Unpaid", delegate { DeleteUnpaid(); }, 110)); bar.Controls.Add(Ui.Button("Export CSV", delegate { ExportFees(); }, 95)); bar.Controls.Add(Ui.Button("Refresh", delegate { LoadAll(); }, 80));
            bar.Controls.Add(new Label { Text = "Search:", AutoSize = true, Padding = new Padding(10, 8, 0, 0) }); _search = new TextBox { Width = 190, Margin = new Padding(5) }; _search.TextChanged += delegate { LoadRecords(); }; bar.Controls.Add(_search); bar.SetFlowBreak(_search, true);
            bar.Controls.Add(new Label { Text = "Status:", AutoSize = true, Padding = new Padding(4, 8, 0, 0) }); _status = new ComboBox { Width = 130, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(5) }; _status.Items.AddRange(new object[] { "All", "Pending", "Partial", "Paid", "Overdue" }); _status.SelectedIndex = 0; _status.SelectedIndexChanged += delegate { LoadRecords(); }; bar.Controls.Add(_status);
            recordsPage.Controls.Add(bar);
            _recordGrid = Ui.Grid(); _recordGrid.Columns.Add(Ui.TextColumn("Student", "Student", 130)); _recordGrid.Columns.Add(Ui.TextColumn("Roll", "Roll", 55)); _recordGrid.Columns.Add(Ui.TextColumn("Class", "Class", 65)); _recordGrid.Columns.Add(Ui.TextColumn("Fee", "Fee Head", 100)); _recordGrid.Columns.Add(Ui.TextColumn("Period", "Period", 70)); _recordGrid.Columns.Add(Ui.TextColumn("Total", "Total", 65)); _recordGrid.Columns.Add(Ui.TextColumn("Paid", "Paid", 65)); _recordGrid.Columns.Add(Ui.TextColumn("Balance", "Balance", 65)); _recordGrid.Columns.Add(Ui.TextColumn("Due", "Due", 75)); _recordGrid.Columns.Add(Ui.TextColumn("Status", "Status", 60));
            Panel recordBody = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) }; recordBody.Controls.Add(_recordGrid); recordsPage.Controls.Add(recordBody); recordBody.BringToFront();
            tabs.TabPages.Add(recordsPage);

            TabPage receiptsPage = new TabPage("Payment Receipts");
            FlowLayoutPanel receiptBar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 52, Padding = new Padding(8) }; receiptBar.Controls.Add(Ui.Button("Print Preview", delegate { PrintSelectedReceipt(); }, 110)); receiptBar.Controls.Add(Ui.Button("Export CSV", delegate { ExportReceipts(); }, 95)); receiptBar.Controls.Add(Ui.Button("Refresh", delegate { LoadReceipts(); }, 80)); receiptsPage.Controls.Add(receiptBar);
            _receiptGrid = Ui.Grid(); _receiptGrid.Columns.Add(Ui.TextColumn("Receipt", "Receipt No", 100)); _receiptGrid.Columns.Add(Ui.TextColumn("Date", "Date", 90)); _receiptGrid.Columns.Add(Ui.TextColumn("Student", "Student", 140)); _receiptGrid.Columns.Add(Ui.TextColumn("Roll", "Roll", 60)); _receiptGrid.Columns.Add(Ui.TextColumn("Class", "Class", 80)); _receiptGrid.Columns.Add(Ui.TextColumn("Mode", "Mode", 70)); _receiptGrid.Columns.Add(Ui.TextColumn("Amount", "Amount", 75)); _receiptGrid.DoubleClick += delegate { PrintSelectedReceipt(); };
            Panel receiptBody = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) }; receiptBody.Controls.Add(_receiptGrid); receiptsPage.Controls.Add(receiptBody); receiptBody.BringToFront(); tabs.TabPages.Add(receiptsPage);

            Controls.Add(tabs); tabs.BringToFront(); LoadAll();
        }

        private void LoadAll() { try { _feeService.RefreshOverdueStatuses(); LoadRecords(); LoadReceipts(); } catch (Exception ex) { Ui.Error(ex.Message); } }
        private void LoadRecords()
        {
            string q = _search.Text.Trim(); string status = Convert.ToString(_status.SelectedItem); var data = _records.GetAll().Where(x => status == "All" || x.Status == status).Where(x => string.IsNullOrEmpty(q) || (x.StudentName ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0 || (x.RollNumber ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0 || (x.FeeType ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0 || (x.ClassSectionName ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0).OrderByDescending(x => x.CreatedDate).ToList();
            _recordGrid.DataSource = data.Select(x => new FeeRow { Entity = x, Student = x.StudentName, Roll = x.RollNumber, Class = x.ClassSectionName, Fee = x.FeeType, Period = x.FeeMonth, Total = x.TotalAmount.ToString("0.00"), Paid = x.PaidAmount.ToString("0.00"), Balance = x.BalanceAmount.ToString("0.00"), Due = x.DueDate.ToString("dd-MMM-yyyy"), Status = x.Status }).ToList();
        }
        private void LoadReceipts() { _receiptGrid.DataSource = _receipts.GetAll().OrderByDescending(x => x.ReceiptDate).Select(x => new ReceiptRow { Entity = x, Receipt = x.ReceiptNumber, Date = x.ReceiptDate.ToString("dd-MMM-yyyy HH:mm"), Student = x.StudentName, Roll = x.RollNumber, Class = x.ClassSectionName, Mode = x.PaymentMethod, Amount = x.ReceiptAmount.ToString("0.00") }).ToList(); }
        private FeeRecord SelectedFee() { FeeRow r = _recordGrid.CurrentRow == null ? null : _recordGrid.CurrentRow.DataBoundItem as FeeRow; return r == null ? null : r.Entity; }
        private FeePaymentReceipt SelectedReceipt() { ReceiptRow r = _receiptGrid.CurrentRow == null ? null : _receiptGrid.CurrentRow.DataBoundItem as ReceiptRow; return r == null ? null : r.Entity; }

        private void AddStudentFee()
        {
            var students = _database.GetCollection<Student>("students").FindAll().ToList(); var heads = _database.GetCollection<FeeHead>("feeheads").FindAll().ToList(); if (students.Count == 0 || heads.Count == 0) { Ui.Error("Create students and fee heads first."); return; }
            using (IndividualFeeDialog dialog = new IndividualFeeDialog(students, heads)) if (dialog.ShowDialog(this) == DialogResult.OK) { try { _feeService.AddStudentFee(dialog.SelectedStudent, dialog.SelectedHead, dialog.Amount, dialog.FeeMonth, dialog.DueDate, dialog.Discount, dialog.LateFee, dialog.Remarks); LoadRecords(); Ui.Info("Student fee added."); } catch (Exception ex) { Ui.Error(ex.Message); } }
        }
        private void ReceivePayment()
        {
            var students = _database.GetCollection<Student>("students").FindAll().ToList(); if (students.Count == 0) { Ui.Error("No students found."); return; }
            SchoolSettings settings = _database.GetCollection<SchoolSettings>("settings").FindById("default") ?? new SchoolSettings();
            using (PaymentDialog dialog = new PaymentDialog(students, id => _records.GetAll().Where(x => x.StudentId == id && x.BalanceAmount > 0.009m).ToList(), settings.ReceivedBy))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        FeePaymentReceipt receipt = _feeService.TakePayment(dialog.StudentId, dialog.Allocations, dialog.PaymentMethod, dialog.TransactionId, dialog.Remarks, dialog.ReceivedBy); LoadAll();
                        DialogResult print = MessageBox.Show("Payment saved. Receipt: " + receipt.ReceiptNumber + "\r\nAmount: " + receipt.ReceiptAmount.ToString("0.00") + "\r\n\r\nOpen print preview?", "Payment Saved", MessageBoxButtons.YesNo, MessageBoxIcon.Information);
                        if (print == DialogResult.Yes) ReceiptPrintService.PreviewAndPrint(receipt, settings);
                    }
                    catch (Exception ex) { Ui.Error(ex.Message); }
                }
            }
        }
        private void DeleteUnpaid()
        {
            FeeRecord item = SelectedFee(); if (item == null) return; if (item.PaidAmount > 0 || item.Status == "Paid" || item.Status == "Partial") { Ui.Error("Paid or partially paid fee records cannot be deleted. This protects accounting history."); return; }
            if (Ui.Confirm("Delete selected unpaid fee record?")) { _records.Delete(item.Id); LoadRecords(); }
        }
        private void PrintSelectedReceipt() { FeePaymentReceipt receipt = SelectedReceipt(); if (receipt == null) return; SchoolSettings settings = _database.GetCollection<SchoolSettings>("settings").FindById("default") ?? new SchoolSettings(); try { ReceiptPrintService.PreviewAndPrint(receipt, settings); } catch (Exception ex) { Ui.Error(ex.Message); } }
        private void ExportFees() { try { string file = CsvExportService.Export(_records.GetAll(), "fee-records", "Id"); Ui.Info("Export created:\r\n" + file); TryOpen(file); } catch (Exception ex) { Ui.Error(ex.Message); } }
        private void ExportReceipts() { try { string file = CsvExportService.Export(_receipts.GetAll(), "fee-receipts", "Id", "Items"); Ui.Info("Export created:\r\n" + file); TryOpen(file); } catch (Exception ex) { Ui.Error(ex.Message); } }
        private static void TryOpen(string file) { try { Process.Start(new ProcessStartInfo(file) { UseShellExecute = true }); } catch { } }
        private class FeeRow { public FeeRecord Entity { get; set; } public string Student { get; set; } public string Roll { get; set; } public string Class { get; set; } public string Fee { get; set; } public string Period { get; set; } public string Total { get; set; } public string Paid { get; set; } public string Balance { get; set; } public string Due { get; set; } public string Status { get; set; } }
        private class ReceiptRow { public FeePaymentReceipt Entity { get; set; } public string Receipt { get; set; } public string Date { get; set; } public string Student { get; set; } public string Roll { get; set; } public string Class { get; set; } public string Mode { get; set; } public string Amount { get; set; } }
    }
}
