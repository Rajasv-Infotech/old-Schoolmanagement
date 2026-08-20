using System;
using System.Linq;
using System.Windows.Forms;
using RajasvSchoolManagement.Data;
using RajasvSchoolManagement.Models;
using RajasvSchoolManagement.UI;

namespace RajasvSchoolManagement.UI.Controls
{
    internal class SessionsControl : UserControl
    {
        private readonly DatabaseService _database;
        private readonly Repository<AcademicSession> _repository;
        private readonly DataGridView _grid;

        public SessionsControl(DatabaseService database)
        {
            _database = database;
            _repository = new Repository<AcademicSession>(database, "sessions");
            Dock = DockStyle.Fill; Font = Ui.DefaultFont;
            Controls.Add(Ui.Header("Academic Sessions", "Manage school academic years / sessions"));

            FlowLayoutPanel toolbar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 48, Padding = new Padding(8), FlowDirection = FlowDirection.LeftToRight };
            toolbar.Controls.Add(Ui.Button("Add", delegate { Add(); }, 80));
            toolbar.Controls.Add(Ui.Button("Edit", delegate { Edit(); }, 80));
            toolbar.Controls.Add(Ui.Button("Delete", delegate { Delete(); }, 80));
            toolbar.Controls.Add(Ui.Button("Refresh", delegate { LoadData(); }, 80));
            Controls.Add(toolbar); toolbar.BringToFront();

            _grid = Ui.Grid();
            _grid.Columns.Add(Ui.TextColumn("Name", "Session", 140));
            _grid.Columns.Add(Ui.TextColumn("Start", "Start Date", 90));
            _grid.Columns.Add(Ui.TextColumn("End", "End Date", 90));
            _grid.Columns.Add(Ui.TextColumn("Status", "Status", 70));
            _grid.DoubleClick += delegate { Edit(); };
            Panel body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) }; body.Controls.Add(_grid); Controls.Add(body); body.BringToFront();
            LoadData();
        }

        private void LoadData()
        {
            _grid.DataSource = _repository.GetAll().OrderByDescending(x => x.StartDate).Select(x => new SessionRow
            {
                Entity = x, Name = x.Name, Start = x.StartDate.ToString("dd-MMM-yyyy"), End = x.EndDate.ToString("dd-MMM-yyyy"), Status = x.IsActive ? "Active" : "Inactive"
            }).ToList();
        }

        private AcademicSession Selected()
        {
            SessionRow row = _grid.CurrentRow == null ? null : _grid.CurrentRow.DataBoundItem as SessionRow;
            return row == null ? null : row.Entity;
        }

        private void Add()
        {
            AcademicSession item = new AcademicSession();
            using (SessionDialog dialog = new SessionDialog(item))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    if (item.IsActive)
                    {
                        foreach (AcademicSession old in _repository.GetAll().Where(x => x.IsActive)) { old.IsActive = false; _repository.Upsert(old); }
                    }
                    _repository.Upsert(item); LoadData();
                }
            }
        }

        private void Edit()
        {
            AcademicSession item = Selected(); if (item == null) return;
            using (SessionDialog dialog = new SessionDialog(item))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    if (item.IsActive)
                    {
                        foreach (AcademicSession old in _repository.GetAll().Where(x => x.IsActive && x.Id != item.Id)) { old.IsActive = false; _repository.Upsert(old); }
                    }
                    _repository.Upsert(item); LoadData();
                }
            }
        }

        private void Delete()
        {
            AcademicSession item = Selected(); if (item == null) return;
            int classes = _database.GetCollection<ClassSection>("classsections").Count(x => x.SessionId == item.Id);
            int students = _database.GetCollection<Student>("students").Count(x => x.SessionId == item.Id);
            if (classes > 0 || students > 0) { Ui.Error("This session is already used by classes/students. Mark it inactive instead of deleting it."); return; }
            if (Ui.Confirm("Delete session '" + item.Name + "'?")) { _repository.Delete(item.Id); LoadData(); }
        }

        private class SessionRow
        {
            public AcademicSession Entity { get; set; }
            public string Name { get; set; }
            public string Start { get; set; }
            public string End { get; set; }
            public string Status { get; set; }
        }
    }
}
