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
    internal class StudentsControl : UserControl
    {
        private readonly DatabaseService _database;
        private readonly Repository<Student> _repository;
        private readonly DataGridView _grid;
        private readonly TextBox _search;
        private readonly ComboBox _session;
        private readonly ComboBox _section;

        public StudentsControl(DatabaseService database)
        {
            _database = database; _repository = new Repository<Student>(database, "students");
            Dock = DockStyle.Fill; Font = Ui.DefaultFont;
            Controls.Add(Ui.Header("Student Admission", "Student master data is stored locally in school.db"));

            FlowLayoutPanel toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 86, Padding = new Padding(8), AutoScroll = true };
            toolbar.Controls.Add(Ui.Button("New Admission", delegate { Add(); }, 120)); toolbar.Controls.Add(Ui.Button("Edit", delegate { Edit(); }, 80)); toolbar.Controls.Add(Ui.Button("Delete", delegate { Delete(); }, 80));
            toolbar.Controls.Add(Ui.Button("Export CSV", delegate { Export(); }, 100)); toolbar.Controls.Add(Ui.Button("Refresh", delegate { ReloadFilters(); LoadData(); }, 80));
            toolbar.Controls.Add(new Label { Text = "Search:", AutoSize = true, Padding = new Padding(10, 8, 0, 0) });
            _search = new TextBox { Width = 180, Margin = new Padding(5) }; _search.TextChanged += delegate { LoadData(); }; toolbar.Controls.Add(_search);
            toolbar.SetFlowBreak(_search, true);
            toolbar.Controls.Add(new Label { Text = "Session:", AutoSize = true, Padding = new Padding(4, 8, 0, 0) });
            _session = new ComboBox { Width = 170, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(5) }; _session.SelectedIndexChanged += delegate { ReloadSections(); LoadData(); }; toolbar.Controls.Add(_session);
            toolbar.Controls.Add(new Label { Text = "Class:", AutoSize = true, Padding = new Padding(4, 8, 0, 0) });
            _section = new ComboBox { Width = 180, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(5) }; _section.SelectedIndexChanged += delegate { LoadData(); }; toolbar.Controls.Add(_section);
            Controls.Add(toolbar); toolbar.BringToFront();

            _grid = Ui.Grid(); _grid.Columns.Add(Ui.TextColumn("Roll", "Roll No", 70)); _grid.Columns.Add(Ui.TextColumn("Name", "Name", 135)); _grid.Columns.Add(Ui.TextColumn("Session", "Session", 90)); _grid.Columns.Add(Ui.TextColumn("Class", "Class", 70)); _grid.Columns.Add(Ui.TextColumn("Section", "Section", 60)); _grid.Columns.Add(Ui.TextColumn("Phone", "Phone", 90)); _grid.Columns.Add(Ui.TextColumn("Father", "Father", 110)); _grid.Columns.Add(Ui.TextColumn("Status", "Status", 65)); _grid.DoubleClick += delegate { Edit(); };
            Panel body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) }; body.Controls.Add(_grid); Controls.Add(body); body.BringToFront();
            ReloadFilters(); LoadData();
        }

        private void ReloadFilters()
        {
            string id = (_session.SelectedItem as LookupItem)?.Id ?? string.Empty; _session.Items.Clear(); _session.Items.Add(new LookupItem { Id = string.Empty, Text = "All sessions" });
            foreach (AcademicSession s in _database.GetCollection<AcademicSession>("sessions").FindAll().OrderByDescending(x => x.StartDate)) _session.Items.Add(new LookupItem { Id = s.Id, Text = s.Name, Tag = s });
            ClassSectionDialog.SelectLookup(_session, id); ReloadSections();
        }
        private void ReloadSections()
        {
            string old = (_section.SelectedItem as LookupItem)?.Id ?? string.Empty; string sessionId = (_session.SelectedItem as LookupItem)?.Id ?? string.Empty; _section.Items.Clear(); _section.Items.Add(new LookupItem { Id = string.Empty, Text = "All classes" });
            foreach (ClassSection c in _database.GetCollection<ClassSection>("classsections").FindAll().Where(x => string.IsNullOrEmpty(sessionId) || x.SessionId == sessionId).OrderBy(x => x.ClassName).ThenBy(x => x.SectionName)) _section.Items.Add(new LookupItem { Id = c.Id, Text = c.SessionName + " - " + c.DisplayName, Tag = c });
            ClassSectionDialog.SelectLookup(_section, old);
        }
        private void LoadData()
        {
            if (_grid == null) return;
            string q = (_search == null ? string.Empty : _search.Text).Trim(); string sessionId = (_session == null ? null : (_session.SelectedItem as LookupItem)?.Id) ?? string.Empty; string sectionId = (_section == null ? null : (_section.SelectedItem as LookupItem)?.Id) ?? string.Empty;
            var students = _repository.GetAll().Where(x => string.IsNullOrEmpty(sessionId) || x.SessionId == sessionId).Where(x => string.IsNullOrEmpty(sectionId) || x.ClassSectionId == sectionId)
                .Where(x => string.IsNullOrEmpty(q) || DisplayName(x).IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0 || (x.RollNumber ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0 || (x.FatherName ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0 || (x.Phone ?? "").IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0)
                .OrderBy(x => x.Class).ThenBy(x => x.Section).ThenBy(x => x.RollNumber).ToList();
            _grid.DataSource = students.Select(x => new Row { Entity = x, Roll = x.RollNumber, Name = DisplayName(x), Session = x.SessionName, Class = x.Class, Section = x.Section, Phone = x.Phone, Father = x.FatherName, Status = x.IsActive ? "Active" : "Inactive" }).ToList();
        }
        private Student Selected() { Row r = _grid.CurrentRow == null ? null : _grid.CurrentRow.DataBoundItem as Row; return r == null ? null : r.Entity; }
        private void Add() { EditItem(new Student { EnrollmentDate = DateTime.Today, DateOfBirth = DateTime.Today.AddYears(-10), IsActive = true }); }
        private void Edit() { Student item = Selected(); if (item != null) EditItem(item); }
        private void EditItem(Student item)
        {
            var sessions = _database.GetCollection<AcademicSession>("sessions").FindAll().ToList(); var sections = _database.GetCollection<ClassSection>("classsections").FindAll().ToList();
            if (sessions.Count == 0 || sections.Count == 0) { Ui.Error("Create an academic session and class/section before adding students."); return; }
            using (StudentDialog dialog = new StudentDialog(item, sessions, sections))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    if (!string.IsNullOrWhiteSpace(item.RollNumber) && _repository.GetAll().Any(x => x.Id != item.Id && x.SessionId == item.SessionId && x.ClassSectionId == item.ClassSectionId && string.Equals(x.RollNumber, item.RollNumber, StringComparison.OrdinalIgnoreCase))) { Ui.Error("This roll number already exists in the selected class."); return; }
                    _repository.Upsert(item); ReloadFilters(); LoadData();
                }
            }
        }
        private void Delete()
        {
            Student item = Selected(); if (item == null) return;
            int fees = _database.GetCollection<FeeRecord>("feerecords").Count(x => x.StudentId == item.Id); int attendance = _database.GetCollection<Attendance>("attendance").Count(x => x.StudentId == item.Id);
            if (fees > 0 || attendance > 0) { Ui.Error("This student has fee/attendance history. Mark the student inactive instead of deleting to preserve school records."); return; }
            if (Ui.Confirm("Delete student '" + DisplayName(item) + "'?")) { _repository.Delete(item.Id); LoadData(); }
        }
        private void Export()
        {
            try { string file = CsvExportService.Export(_repository.GetAll(), "students", "Id"); Ui.Info("Export created:\r\n" + file); TryOpen(file); } catch (Exception ex) { Ui.Error(ex.Message); }
        }
        private static void TryOpen(string file) { try { Process.Start(new ProcessStartInfo(file) { UseShellExecute = true }); } catch { } }
        private static string DisplayName(Student s) { return !string.IsNullOrWhiteSpace(s.FullName) ? s.FullName : (s.FirstName + " " + s.LastName).Trim(); }
        private class Row { public Student Entity { get; set; } public string Roll { get; set; } public string Name { get; set; } public string Session { get; set; } public string Class { get; set; } public string Section { get; set; } public string Phone { get; set; } public string Father { get; set; } public string Status { get; set; } }
    }
}
