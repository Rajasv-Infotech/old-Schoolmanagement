using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using RajasvSchoolManagement.Data;
using RajasvSchoolManagement.Models;
using RajasvSchoolManagement.UI;

namespace RajasvSchoolManagement.UI.Controls
{
    internal class AttendanceControl : UserControl
    {
        private readonly DatabaseService _database;
        private readonly Repository<Attendance> _repository;
        private readonly ComboBox _classSection;
        private readonly DateTimePicker _date;
        private readonly DataGridView _grid;
        private BindingList<Row> _rows = new BindingList<Row>();

        public AttendanceControl(DatabaseService database)
        {
            _database = database; _repository = new Repository<Attendance>(database, "attendance");
            Dock = DockStyle.Fill; Font = Ui.DefaultFont;
            Controls.Add(Ui.Header("Attendance", "Mark daily student attendance class-wise"));
            FlowLayoutPanel toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 58, Padding = new Padding(8) };
            toolbar.Controls.Add(new Label { Text = "Date:", AutoSize = true, Padding = new Padding(5, 8, 0, 0) }); _date = new DateTimePicker { Width = 120, Format = DateTimePickerFormat.Short, Margin = new Padding(5) }; _date.ValueChanged += delegate { LoadRows(); }; toolbar.Controls.Add(_date);
            toolbar.Controls.Add(new Label { Text = "Class:", AutoSize = true, Padding = new Padding(5, 8, 0, 0) }); _classSection = new ComboBox { Width = 220, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(5) }; _classSection.SelectedIndexChanged += delegate { LoadRows(); }; toolbar.Controls.Add(_classSection);
            toolbar.Controls.Add(Ui.Button("Save Attendance", delegate { Save(); }, 130)); toolbar.Controls.Add(Ui.Button("Present All", delegate { SetAll("Present"); }, 100)); toolbar.Controls.Add(Ui.Button("Refresh", delegate { LoadClasses(); LoadRows(); }, 80));
            Controls.Add(toolbar); toolbar.BringToFront();

            _grid = new DataGridView { Dock = DockStyle.Fill, AutoGenerateColumns = false, AllowUserToAddRows = false, AllowUserToDeleteRows = false, RowHeadersVisible = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Roll", HeaderText = "Roll No", ReadOnly = true, FillWeight = 60 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "StudentName", HeaderText = "Student", ReadOnly = true, FillWeight = 140 });
            DataGridViewComboBoxColumn status = new DataGridViewComboBoxColumn { DataPropertyName = "Status", HeaderText = "Status", FillWeight = 70 }; status.Items.AddRange("Present", "Absent", "Late", "Leave"); _grid.Columns.Add(status);
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Remarks", HeaderText = "Remarks", FillWeight = 150 });
            Panel body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) }; body.Controls.Add(_grid); Controls.Add(body); body.BringToFront();
            LoadClasses(); LoadRows();
        }

        private void LoadClasses()
        {
            string old = (_classSection.SelectedItem as LookupItem)?.Id ?? string.Empty; _classSection.Items.Clear();
            foreach (ClassSection c in _database.GetCollection<ClassSection>("classsections").FindAll().Where(x => x.IsActive).OrderByDescending(x => x.SessionName).ThenBy(x => x.ClassName).ThenBy(x => x.SectionName)) _classSection.Items.Add(new LookupItem { Id = c.Id, Text = c.SessionName + " - " + c.DisplayName, Tag = c });
            ClassSectionDialog.SelectLookup(_classSection, old);
        }
        private void LoadRows()
        {
            if (_grid == null || _classSection.SelectedItem == null) { if (_grid != null) _grid.DataSource = null; return; }
            string classId = ((LookupItem)_classSection.SelectedItem).Id; DateTime day = _date.Value.Date;
            var existing = _repository.GetAll().Where(x => x.Date.Date == day).ToDictionary(x => x.StudentId, x => x);
            _rows = new BindingList<Row>(_database.GetCollection<Student>("students").Find(x => x.ClassSectionId == classId && x.IsActive).OrderBy(x => x.RollNumber).Select(s =>
            {
                Attendance a; existing.TryGetValue(s.Id, out a);
                return new Row { RecordId = a == null ? null : a.Id, StudentId = s.Id, Roll = s.RollNumber, StudentName = DisplayName(s), Status = a == null ? "Present" : a.Status, Remarks = a == null ? string.Empty : a.Remarks };
            }).ToList());
            _grid.DataSource = _rows;
        }
        private void SetAll(string status) { foreach (Row row in _rows) row.Status = status; _grid.Refresh(); }
        private void Save()
        {
            if (_classSection.SelectedItem == null) { Ui.Error("Select a class."); return; }
            _grid.EndEdit(); DateTime day = _date.Value.Date;
            foreach (Row row in _rows)
            {
                Attendance item = string.IsNullOrEmpty(row.RecordId) ? new Attendance { StudentId = row.StudentId, StudentName = row.StudentName, Date = day } : _repository.FindById(row.RecordId);
                if (item == null) item = new Attendance { StudentId = row.StudentId, StudentName = row.StudentName, Date = day };
                item.StudentName = row.StudentName; item.Date = day; item.Status = string.IsNullOrWhiteSpace(row.Status) ? "Present" : row.Status; item.Remarks = row.Remarks ?? string.Empty; _repository.Upsert(item); row.RecordId = item.Id;
            }
            Ui.Info("Attendance saved for " + _rows.Count + " students.");
        }
        private static string DisplayName(Student s) { return !string.IsNullOrWhiteSpace(s.FullName) ? s.FullName : (s.FirstName + " " + s.LastName).Trim(); }
        private class Row : INotifyPropertyChanged
        {
            private string _status; private string _remarks;
            public string RecordId { get; set; } public string StudentId { get; set; } public string Roll { get; set; } public string StudentName { get; set; }
            public string Status { get { return _status; } set { _status = value; OnChanged("Status"); } }
            public string Remarks { get { return _remarks; } set { _remarks = value; OnChanged("Remarks"); } }
            public event PropertyChangedEventHandler PropertyChanged; private void OnChanged(string name) { PropertyChangedEventHandler h = PropertyChanged; if (h != null) h(this, new PropertyChangedEventArgs(name)); }
        }
    }
}
