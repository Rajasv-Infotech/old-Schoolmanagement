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
    internal class TeachersControl : UserControl
    {
        private readonly Repository<Teacher> _repository; private readonly DataGridView _grid; private readonly TextBox _search;
        public TeachersControl(DatabaseService database)
        {
            _repository = new Repository<Teacher>(database, "teachers"); Dock = DockStyle.Fill; Font = Ui.DefaultFont;
            Controls.Add(Ui.Header("Teachers", "Teacher contact, subject, qualification and salary records"));
            FlowLayoutPanel toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 52, Padding = new Padding(8) };
            toolbar.Controls.Add(Ui.Button("Add", delegate { EditItem(new Teacher()); }, 80)); toolbar.Controls.Add(Ui.Button("Edit", delegate { Teacher x = Selected(); if (x != null) EditItem(x); }, 80)); toolbar.Controls.Add(Ui.Button("Delete", delegate { Delete(); }, 80)); toolbar.Controls.Add(Ui.Button("Export CSV", delegate { Export(); }, 100)); toolbar.Controls.Add(new Label { Text = "Search:", AutoSize = true, Padding = new Padding(12, 8, 0, 0) });
            _search = new TextBox { Width = 200, Margin = new Padding(5) }; _search.TextChanged += delegate { LoadData(); }; toolbar.Controls.Add(_search); Controls.Add(toolbar); toolbar.BringToFront();
            _grid = Ui.Grid(); _grid.Columns.Add(Ui.TextColumn("Name", "Name", 130)); _grid.Columns.Add(Ui.TextColumn("Subject", "Subject", 110)); _grid.Columns.Add(Ui.TextColumn("Phone", "Phone", 90)); _grid.Columns.Add(Ui.TextColumn("Email", "Email", 130)); _grid.Columns.Add(Ui.TextColumn("Qualification", "Qualification", 110)); _grid.Columns.Add(Ui.TextColumn("Joining", "Joining", 75)); _grid.Columns.Add(Ui.TextColumn("Salary", "Salary", 70)); _grid.Columns.Add(Ui.TextColumn("Status", "Status", 60)); _grid.DoubleClick += delegate { Teacher x = Selected(); if (x != null) EditItem(x); };
            Panel body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) }; body.Controls.Add(_grid); Controls.Add(body); body.BringToFront(); LoadData();
        }
        private void LoadData() { string q = _search.Text.Trim(); _grid.DataSource = _repository.GetAll().Where(x => string.IsNullOrEmpty(q) || x.FullName.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0 || (x.Subject ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0 || (x.Phone ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0).OrderBy(x => x.FirstName).Select(x => new Row { Entity = x, Name = x.FullName, Subject = x.Subject, Phone = x.Phone, Email = x.Email, Qualification = x.Qualification, Joining = x.JoiningDate.ToString("dd-MMM-yyyy"), Salary = x.Salary.ToString("0.00"), Status = x.IsActive ? "Active" : "Inactive" }).ToList(); }
        private Teacher Selected() { Row r = _grid.CurrentRow == null ? null : _grid.CurrentRow.DataBoundItem as Row; return r == null ? null : r.Entity; }
        private void EditItem(Teacher item) { using (TeacherDialog dialog = new TeacherDialog(item)) if (dialog.ShowDialog(this) == DialogResult.OK) { _repository.Upsert(item); LoadData(); } }
        private void Delete() { Teacher item = Selected(); if (item != null && Ui.Confirm("Delete teacher '" + item.FullName + "'?")) { _repository.Delete(item.Id); LoadData(); } }
        private void Export() { try { string file = CsvExportService.Export(_repository.GetAll(), "teachers", "Id"); Ui.Info("Export created:\r\n" + file); try { Process.Start(new ProcessStartInfo(file) { UseShellExecute = true }); } catch { } } catch (Exception ex) { Ui.Error(ex.Message); } }
        private class Row { public Teacher Entity { get; set; } public string Name { get; set; } public string Subject { get; set; } public string Phone { get; set; } public string Email { get; set; } public string Qualification { get; set; } public string Joining { get; set; } public string Salary { get; set; } public string Status { get; set; } }
    }
}
