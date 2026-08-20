using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RajasvSchoolManagement.Models;

namespace RajasvSchoolManagement.UI
{
    internal class PaymentDialog : Form
    {
        private readonly IList<Student> _students;
        private readonly Func<string, List<FeeRecord>> _loadOutstanding;
        private readonly ComboBox _student;
        private readonly DataGridView _grid;
        private readonly ComboBox _method;
        private readonly TextBox _transaction;
        private readonly TextBox _remarks;
        private readonly TextBox _receivedBy;
        private readonly Label _total;
        private BindingList<PaymentRow> _rows = new BindingList<PaymentRow>();

        public string StudentId { get; private set; }
        public Dictionary<string, decimal> Allocations { get; private set; }
        public string PaymentMethod { get; private set; }
        public string TransactionId { get; private set; }
        public string Remarks { get; private set; }
        public string ReceivedBy { get; private set; }

        public PaymentDialog(IList<Student> students, Func<string, List<FeeRecord>> loadOutstanding, string defaultReceivedBy)
        {
            _students = students;
            _loadOutstanding = loadOutstanding;
            Text = "Receive Fee Payment";
            Width = 920;
            Height = 660;
            StartPosition = FormStartPosition.CenterParent;
            Font = Ui.DefaultFont;

            Panel top = new Panel { Dock = DockStyle.Top, Height = 96, Padding = new Padding(10) };
            top.Controls.Add(Ui.Label("Student", 70));
            _student = new ComboBox { Left = 80, Top = 10, Width = 500, DropDownStyle = ComboBoxStyle.DropDownList };
            foreach (Student s in students.Where(x => x.IsActive).OrderBy(x => x.FullName))
            {
                string name = !string.IsNullOrWhiteSpace(s.FullName) ? s.FullName : (s.FirstName + " " + s.LastName).Trim();
                _student.Items.Add(new LookupItem { Id = s.Id, Text = name + " | " + s.Class + "-" + s.Section + " | Roll " + s.RollNumber, Tag = s });
            }
            _student.SelectedIndexChanged += delegate { LoadRows(); };
            top.Controls.Add(_student);

            Label hint = new Label
            {
                Text = "Tick Pay and enter amount for each outstanding fee. Partial payment is supported.",
                Left = 80,
                Top = 45,
                Width = 700,
                Height = 30,
                ForeColor = SystemColors.GrayText
            };
            top.Controls.Add(hint);
            Controls.Add(top);

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AutoGenerateColumns = false,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            _grid.Columns.Add(new DataGridViewCheckBoxColumn { DataPropertyName = "Pay", HeaderText = "Pay", FillWeight = 35 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "FeeType", HeaderText = "Fee Head", ReadOnly = true, FillWeight = 120 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "FeeMonth", HeaderText = "Period", ReadOnly = true, FillWeight = 80 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "DueDate", HeaderText = "Due Date", ReadOnly = true, FillWeight = 75 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Balance", HeaderText = "Balance", ReadOnly = true, FillWeight = 75 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "PayAmount", HeaderText = "Pay Amount", FillWeight = 80 });
            _grid.CellValueChanged += delegate { UpdateTotal(); };
            _grid.CurrentCellDirtyStateChanged += delegate { if (_grid.IsCurrentCellDirty) _grid.CommitEdit(DataGridViewDataErrorContexts.Commit); };

            Panel bottom = new Panel { Dock = DockStyle.Bottom, Height = 155, Padding = new Padding(10) };
            Label methodLabel = new Label { Text = "Payment method", Left = 10, Top = 10, Width = 110 };
            _method = new ComboBox { Left = 125, Top = 8, Width = 140, DropDownStyle = ComboBoxStyle.DropDownList };
            _method.Items.AddRange(new object[] { "Cash", "Card", "Online", "Cheque", "UPI", "Bank Transfer" }); _method.SelectedIndex = 0;
            bottom.Controls.Add(methodLabel); bottom.Controls.Add(_method);

            bottom.Controls.Add(new Label { Text = "Transaction ID", Left = 285, Top = 10, Width = 100 });
            _transaction = new TextBox { Left = 390, Top = 8, Width = 180 }; bottom.Controls.Add(_transaction);
            bottom.Controls.Add(new Label { Text = "Received by", Left = 590, Top = 10, Width = 90 });
            _receivedBy = new TextBox { Left = 680, Top = 8, Width = 170, Text = defaultReceivedBy ?? string.Empty }; bottom.Controls.Add(_receivedBy);

            bottom.Controls.Add(new Label { Text = "Remarks", Left = 10, Top = 48, Width = 110 });
            _remarks = new TextBox { Left = 125, Top = 45, Width = 445 }; bottom.Controls.Add(_remarks);
            _total = new Label { Text = "Total: 0.00", Left = 590, Top = 45, Width = 260, Height = 28, Font = Ui.SubHeadingFont, TextAlign = ContentAlignment.MiddleRight }; bottom.Controls.Add(_total);

            Button cancel = Ui.Button("Cancel", delegate { DialogResult = DialogResult.Cancel; Close(); }, 90);
            cancel.Left = 665; cancel.Top = 92;
            Button save = Ui.Button("Receive Payment", delegate { SavePayment(); }, 150);
            save.Left = 760; save.Top = 92;
            bottom.Controls.Add(cancel); bottom.Controls.Add(save);
            Controls.Add(bottom);
            Controls.Add(_grid);

            if (_student.Items.Count > 0) _student.SelectedIndex = 0;
        }

        private void LoadRows()
        {
            LookupItem selected = _student.SelectedItem as LookupItem;
            _rows = new BindingList<PaymentRow>();
            if (selected != null)
            {
                foreach (FeeRecord r in _loadOutstanding(selected.Id).OrderBy(x => x.DueDate).ThenBy(x => x.FeeType))
                {
                    _rows.Add(new PaymentRow
                    {
                        FeeRecordId = r.Id,
                        FeeType = r.FeeType,
                        FeeMonth = r.FeeMonth,
                        DueDate = r.DueDate.ToString("dd-MMM-yyyy"),
                        Balance = r.BalanceAmount,
                        Pay = false,
                        PayAmount = r.BalanceAmount
                    });
                }
            }
            _grid.DataSource = _rows;
            UpdateTotal();
        }

        private void UpdateTotal()
        {
            if (_grid != null) _grid.EndEdit();
            decimal total = _rows.Where(x => x.Pay).Sum(x => Math.Max(0, Math.Min(x.PayAmount, x.Balance)));
            _total.Text = "Total: " + total.ToString("0.00");
        }

        private void SavePayment()
        {
            try
            {
                _grid.EndEdit();
                LookupItem selected = _student.SelectedItem as LookupItem;
                if (selected == null) { Ui.Error("Select a student."); return; }
                Dictionary<string, decimal> allocations = new Dictionary<string, decimal>();
                foreach (PaymentRow row in _rows.Where(x => x.Pay))
                {
                    decimal amount = Math.Min(row.PayAmount, row.Balance);
                    if (amount > 0) allocations[row.FeeRecordId] = amount;
                }
                if (allocations.Count == 0) { Ui.Error("Select at least one fee and enter a payment amount."); return; }

                StudentId = selected.Id;
                Allocations = allocations;
                PaymentMethod = Convert.ToString(_method.SelectedItem);
                TransactionId = _transaction.Text.Trim();
                Remarks = _remarks.Text.Trim();
                ReceivedBy = _receivedBy.Text.Trim();
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex) { Ui.Error(ex.Message); }
        }

        private class PaymentRow
        {
            public string FeeRecordId { get; set; }
            public bool Pay { get; set; }
            public string FeeType { get; set; }
            public string FeeMonth { get; set; }
            public string DueDate { get; set; }
            public decimal Balance { get; set; }
            public decimal PayAmount { get; set; }
        }
    }
}
