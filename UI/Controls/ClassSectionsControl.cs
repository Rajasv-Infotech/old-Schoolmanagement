using System;
using System.Linq;
using System.Windows.Forms;
using RajasvSchoolManagement.Data;
using RajasvSchoolManagement.Models;
using RajasvSchoolManagement.UI;

namespace RajasvSchoolManagement.UI.Controls
{
    internal class ClassSectionsControl : UserControl
    {
        private readonly DatabaseService _database;
        private readonly Repository<ClassSection> _repository;
        private readonly DataGridView _grid;
        private readonly ComboBox _sessionFilter;

        public ClassSectionsControl(DatabaseService database)
        {
            _database = database; _repository = new Repository<ClassSection>(database, "classsections");
            Dock = DockStyle.Fill; Font = Ui.DefaultFont;
            Controls.Add(Ui.Header("Classes & Sections", "Create class-section combinations for each academic session"));
            FlowLayoutPanel toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 52, Padding = new Padding(8) };
            toolbar.Controls.Add(Ui.Button("Add", delegate { Add(); }, 80)); toolbar.Controls.Add(Ui.Button("Edit", delegate { Edit(); }, 80)); toolbar.Controls.Add(Ui.Button("Delete", delegate { Delete(); }, 80));
            toolbar.Controls.Add(new Label { Text = "Session:", AutoSize = true, Padding = new Padding(10, 8, 0, 0) });
            _sessionFilter = new ComboBox { Width = 180, DropDownStyle = ComboBoxStyle.DropDownList, Margin = new Padding(5) }; _sessionFilter.SelectedIndexChanged += delegate { LoadData(); };
            toolbar.Controls.Add(_sessionFilter); toolbar.Controls.Add(Ui.Button("Refresh", delegate { ReloadFilter(); LoadData(); }, 80)); Controls.Add(toolbar); toolbar.BringToFront();
            _grid = Ui.Grid(); _grid.Columns.Add(Ui.TextColumn("Session", "Session", 110)); _grid.Columns.Add(Ui.TextColumn("Class", "Class", 100)); _grid.Columns.Add(Ui.TextColumn("Section", "Section", 80)); _grid.Columns.Add(Ui.TextColumn("Capacity", "Capacity", 70)); _grid.Columns.Add(Ui.TextColumn("Status", "Status", 70)); _grid.DoubleClick += delegate { Edit(); };
            Panel body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) }; body.Controls.Add(_grid); Controls.Add(body); body.BringToFront();
            ReloadFilter(); LoadData();
        }

        private void ReloadFilter()
        {
            string selected = (_sessionFilter.SelectedItem as LookupItem)?.Id ?? string.Empty; _sessionFilter.Items.Clear(); _sessionFilter.Items.Add(new LookupItem { Id = string.Empty, Text = "All sessions" });
            foreach (AcademicSession s in _database.GetCollection<AcademicSession>("sessions").FindAll().OrderByDescending(x => x.StartDate)) _sessionFilter.Items.Add(new LookupItem { Id = s.Id, Text = s.Name, Tag = s });
            ClassSectionDialog.SelectLookup(_sessionFilter, selected);
        }
        private void LoadData()
        {
            string sessionId = (_sessionFilter.SelectedItem as LookupItem)?.Id ?? string.Empty;
            _grid.DataSource = _repository.GetAll().Where(x => string.IsNullOrEmpty(sessionId) || x.SessionId == sessionId).OrderByDescending(x => x.SessionName).ThenBy(x => x.ClassName).ThenBy(x => x.SectionName)
                .Select(x => new Row { Entity = x, Session = x.SessionName, Class = x.ClassName, Section = x.SectionName, Capacity = x.Capacity, Status = x.IsActive ? "Active" : "Inactive" }).ToList();
        }
        private ClassSection Selected() { Row r = _grid.CurrentRow == null ? null : _grid.CurrentRow.DataBoundItem as Row; return r == null ? null : r.Entity; }
        private void Add() { EditItem(new ClassSection()); }
        private void Edit() { ClassSection item = Selected(); if (item != null) EditItem(item); }
        private void EditItem(ClassSection item)
        {
            var sessions = _database.GetCollection<AcademicSession>("sessions").FindAll().ToList(); if (sessions.Count == 0) { Ui.Error("Create an academic session first."); return; }
            using (ClassSectionDialog dialog = new ClassSectionDialog(item, sessions)) if (dialog.ShowDialog(this) == DialogResult.OK) { _repository.Upsert(item); ReloadFilter(); LoadData(); }
        }
        private void Delete()
        {
            ClassSection item = Selected(); if (item == null) return;
            int students = _database.GetCollection<Student>("students").Count(x => x.ClassSectionId == item.Id); if (students > 0) { Ui.Error("Students are assigned to this class. Mark it inactive instead of deleting it."); return; }
            if (Ui.Confirm("Delete " + item.DisplayName + "?")) { _repository.Delete(item.Id); LoadData(); }
        }
        private class Row { public ClassSection Entity { get; set; } public string Session { get; set; } public string Class { get; set; } public string Section { get; set; } public int Capacity { get; set; } public string Status { get; set; } }
    }
}
