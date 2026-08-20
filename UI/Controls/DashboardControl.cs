using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RajasvSchoolManagement.Data;
using RajasvSchoolManagement.Models;
using RajasvSchoolManagement.Services;
using RajasvSchoolManagement.UI;

namespace RajasvSchoolManagement.UI.Controls
{
    internal class DashboardControl : UserControl
    {
        private readonly DatabaseService _database;
        private readonly FeeService _fees;
        private readonly FlowLayoutPanel _cards;
        private readonly DataGridView _recent;

        public DashboardControl(DatabaseService database)
        {
            _database = database;
            _fees = new FeeService(database);
            Dock = DockStyle.Fill;
            Font = Ui.DefaultFont;
            Controls.Add(Ui.Header("Dashboard", "Offline school management overview"));

            Panel toolbar = new Panel { Dock = DockStyle.Top, Height = 48, Padding = new Padding(10, 5, 10, 5) };
            toolbar.Controls.Add(Ui.Button("Refresh", delegate { LoadData(); }, 90));
            Controls.Add(toolbar);
            toolbar.BringToFront();

            _cards = new FlowLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 130,
                Padding = new Padding(10),
                AutoScroll = true,
                WrapContents = false
            };
            Controls.Add(_cards);
            _cards.BringToFront();

            Panel recentPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) };
            Label recentTitle = new Label { Text = "Recent Fee Receipts", Dock = DockStyle.Top, Height = 30, Font = Ui.SubHeadingFont };
            _recent = Ui.Grid();
            _recent.Columns.Add(Ui.TextColumn("ReceiptNumber", "Receipt", 90));
            _recent.Columns.Add(Ui.TextColumn("ReceiptDate", "Date", 90));
            _recent.Columns.Add(Ui.TextColumn("StudentName", "Student", 150));
            _recent.Columns.Add(Ui.TextColumn("ClassSectionName", "Class", 80));
            _recent.Columns.Add(Ui.TextColumn("PaymentMethod", "Mode", 80));
            _recent.Columns.Add(Ui.TextColumn("ReceiptAmount", "Amount", 80));
            recentPanel.Controls.Add(_recent);
            recentPanel.Controls.Add(recentTitle);
            Controls.Add(recentPanel);
            recentPanel.BringToFront();

            LoadData();
        }

        private void LoadData()
        {
            try
            {
                _fees.RefreshOverdueStatuses();
                int students = _database.GetCollection<Student>("students").Count(x => x.IsActive);
                int teachers = _database.GetCollection<Teacher>("teachers").Count(x => x.IsActive);
                List<FeeRecord> feeRecords = _database.GetCollection<FeeRecord>("feerecords").FindAll().ToList();
                decimal outstanding = feeRecords.Sum(x => Math.Max(0, x.BalanceAmount));
                decimal overdue = feeRecords.Where(x => x.Status == "Overdue").Sum(x => Math.Max(0, x.BalanceAmount));
                DateTime monthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                List<FeePaymentReceipt> receipts = _database.GetCollection<FeePaymentReceipt>("receipts").FindAll().ToList();
                decimal collectedMonth = receipts.Where(x => x.ReceiptDate >= monthStart).Sum(x => x.ReceiptAmount);

                _cards.Controls.Clear();
                _cards.Controls.Add(Card("Active Students", students.ToString()));
                _cards.Controls.Add(Card("Active Teachers", teachers.ToString()));
                _cards.Controls.Add(Card("Outstanding Fees", outstanding.ToString("0.00")));
                _cards.Controls.Add(Card("Overdue Fees", overdue.ToString("0.00")));
                _cards.Controls.Add(Card("Collected This Month", collectedMonth.ToString("0.00")));

                _recent.DataSource = receipts.OrderByDescending(x => x.ReceiptDate).Take(20)
                    .Select(x => new
                    {
                        x.ReceiptNumber,
                        ReceiptDate = x.ReceiptDate.ToString("dd-MMM-yyyy HH:mm"),
                        x.StudentName,
                        x.ClassSectionName,
                        x.PaymentMethod,
                        ReceiptAmount = x.ReceiptAmount.ToString("0.00")
                    }).ToList();
            }
            catch (Exception ex) { Ui.Error("Dashboard load failed: " + ex.Message); }
        }

        private static Panel Card(string title, string value)
        {
            Panel panel = new Panel { Width = 205, Height = 95, Margin = new Padding(6), BorderStyle = BorderStyle.FixedSingle, Padding = new Padding(10) };
            Label t = new Label { Text = title, Dock = DockStyle.Top, Height = 26, ForeColor = SystemColors.GrayText };
            Label v = new Label { Text = value, Dock = DockStyle.Fill, Font = new Font("Segoe UI", 18F, FontStyle.Bold), TextAlign = ContentAlignment.MiddleLeft };
            panel.Controls.Add(v); panel.Controls.Add(t);
            return panel;
        }
    }
}
