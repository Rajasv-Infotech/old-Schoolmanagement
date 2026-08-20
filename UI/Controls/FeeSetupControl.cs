using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using RajasvSchoolManagement.Data;
using RajasvSchoolManagement.Models;
using RajasvSchoolManagement.Services;
using RajasvSchoolManagement.UI;

namespace RajasvSchoolManagement.UI.Controls
{
    internal class FeeSetupControl : UserControl
    {
        private readonly DatabaseService _database;
        private readonly FeeService _feeService;
        private readonly Repository<FeeHead> _heads;
        private readonly Repository<FeeStructure> _structures;
        private readonly Repository<CommonFeeRule> _rules;
        private readonly DataGridView _headGrid;
        private readonly DataGridView _structureGrid;
        private readonly DataGridView _ruleGrid;

        public FeeSetupControl(DatabaseService database)
        {
            _database = database; _feeService = new FeeService(database);
            _heads = new Repository<FeeHead>(database, "feeheads"); _structures = new Repository<FeeStructure>(database, "feestructures"); _rules = new Repository<CommonFeeRule>(database, "commonfeerules");
            Dock = DockStyle.Fill; Font = Ui.DefaultFont;
            Controls.Add(Ui.Header("Fee Setup", "Configure fee heads, class fee structures and common class charges"));
            TabControl tabs = new TabControl { Dock = DockStyle.Fill, Padding = new Point(12, 5) };
            _headGrid = BuildHeadTab(tabs); _structureGrid = BuildStructureTab(tabs); _ruleGrid = BuildRuleTab(tabs);
            Controls.Add(tabs); tabs.BringToFront();
            LoadHeads(); LoadStructures(); LoadRules();
        }

        private DataGridView BuildHeadTab(TabControl tabs)
        {
            TabPage page = new TabPage("Fee Heads"); FlowLayoutPanel bar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 52, Padding = new Padding(8) };
            bar.Controls.Add(Ui.Button("Add", delegate { EditHead(new FeeHead()); }, 80)); bar.Controls.Add(Ui.Button("Edit", delegate { FeeHead x = SelectedHead(); if (x != null) EditHead(x); }, 80)); bar.Controls.Add(Ui.Button("Delete", delegate { DeleteHead(); }, 80)); bar.Controls.Add(Ui.Button("Refresh", delegate { LoadHeads(); }, 80)); page.Controls.Add(bar);
            DataGridView grid = Ui.Grid(); grid.Columns.Add(Ui.TextColumn("Name", "Name", 120)); grid.Columns.Add(Ui.TextColumn("Code", "Code", 70)); grid.Columns.Add(Ui.TextColumn("Amount", "Default Amount", 80)); grid.Columns.Add(Ui.TextColumn("Frequency", "Frequency", 80)); grid.Columns.Add(Ui.TextColumn("Status", "Status", 60)); grid.Columns.Add(Ui.TextColumn("Description", "Description", 150)); grid.DoubleClick += delegate { FeeHead x = SelectedHead(); if (x != null) EditHead(x); };
            Panel body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) }; body.Controls.Add(grid); page.Controls.Add(body); body.BringToFront(); tabs.TabPages.Add(page); return grid;
        }
        private DataGridView BuildStructureTab(TabControl tabs)
        {
            TabPage page = new TabPage("Fee Structures"); FlowLayoutPanel bar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 52, Padding = new Padding(8) };
            bar.Controls.Add(Ui.Button("Add", delegate { EditStructure(new FeeStructure()); }, 80)); bar.Controls.Add(Ui.Button("Edit", delegate { FeeStructure x = SelectedStructure(); if (x != null) EditStructure(x); }, 80)); bar.Controls.Add(Ui.Button("Delete", delegate { DeleteStructure(); }, 80)); page.Controls.Add(bar);
            DataGridView grid = Ui.Grid(); grid.Columns.Add(Ui.TextColumn("Class", "Class", 90)); grid.Columns.Add(Ui.TextColumn("FeeType", "Fee Type", 110)); grid.Columns.Add(Ui.TextColumn("Amount", "Amount", 80)); grid.Columns.Add(Ui.TextColumn("Frequency", "Frequency", 80)); grid.Columns.Add(Ui.TextColumn("Effective", "Effective From", 90)); grid.Columns.Add(Ui.TextColumn("Status", "Status", 60)); grid.Columns.Add(Ui.TextColumn("Description", "Description", 130)); grid.DoubleClick += delegate { FeeStructure x = SelectedStructure(); if (x != null) EditStructure(x); };
            Panel body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) }; body.Controls.Add(grid); page.Controls.Add(body); body.BringToFront(); tabs.TabPages.Add(page); return grid;
        }
        private DataGridView BuildRuleTab(TabControl tabs)
        {
            TabPage page = new TabPage("Common Class Fees"); FlowLayoutPanel bar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 52, Padding = new Padding(8) };
            bar.Controls.Add(Ui.Button("Create & Apply", delegate { CreateRule(); }, 120)); bar.Controls.Add(Ui.Button("Edit Rule", delegate { EditRule(); }, 90)); bar.Controls.Add(Ui.Button("Apply Missing", delegate { ApplyRule(); }, 110)); bar.Controls.Add(Ui.Button("Deactivate", delegate { DeactivateRule(); }, 100)); page.Controls.Add(bar);
            DataGridView grid = Ui.Grid(); grid.Columns.Add(Ui.TextColumn("Session", "Session", 90)); grid.Columns.Add(Ui.TextColumn("ClassSection", "Class / Section", 100)); grid.Columns.Add(Ui.TextColumn("FeeHead", "Fee Head", 110)); grid.Columns.Add(Ui.TextColumn("Period", "Period", 80)); grid.Columns.Add(Ui.TextColumn("Amount", "Amount", 70)); grid.Columns.Add(Ui.TextColumn("Due", "Due Date", 80)); grid.Columns.Add(Ui.TextColumn("Status", "Status", 60));
            Panel body = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10) }; body.Controls.Add(grid); page.Controls.Add(body); body.BringToFront(); tabs.TabPages.Add(page); return grid;
        }

        private void LoadHeads() { _headGrid.DataSource = _heads.GetAll().OrderBy(x => x.Name).Select(x => new HeadRow { Entity = x, Name = x.Name, Code = x.Code, Amount = x.DefaultAmount.ToString("0.00"), Frequency = x.Frequency, Status = x.IsActive ? "Active" : "Inactive", Description = x.Description }).ToList(); }
        private FeeHead SelectedHead() { HeadRow r = _headGrid.CurrentRow == null ? null : _headGrid.CurrentRow.DataBoundItem as HeadRow; return r == null ? null : r.Entity; }
        private void EditHead(FeeHead item) { using (FeeHeadDialog dialog = new FeeHeadDialog(item)) if (dialog.ShowDialog(this) == DialogResult.OK) { if (_heads.GetAll().Any(x => x.Id != item.Id && string.Equals(x.Name, item.Name, StringComparison.OrdinalIgnoreCase))) { Ui.Error("A fee head with this name already exists."); return; } _heads.Upsert(item); LoadHeads(); } }
        private void DeleteHead() { FeeHead x = SelectedHead(); if (x == null) return; if (_database.GetCollection<FeeRecord>("feerecords").Exists(r => r.FeeHeadId == x.Id) || _database.GetCollection<CommonFeeRule>("commonfeerules").Exists(r => r.FeeHeadId == x.Id)) { Ui.Error("This fee head is already used. Mark it inactive instead of deleting."); return; } if (Ui.Confirm("Delete fee head '" + x.Name + "'?")) { _heads.Delete(x.Id); LoadHeads(); } }

        private void LoadStructures() { _structureGrid.DataSource = _structures.GetAll().OrderBy(x => x.Class).ThenBy(x => x.FeeType).Select(x => new StructureRow { Entity = x, Class = x.Class, FeeType = x.FeeType, Amount = x.Amount.ToString("0.00"), Frequency = x.Frequency, Effective = x.EffectiveFrom.ToString("dd-MMM-yyyy"), Status = x.IsActive ? "Active" : "Inactive", Description = x.Description }).ToList(); }
        private FeeStructure SelectedStructure() { StructureRow r = _structureGrid.CurrentRow == null ? null : _structureGrid.CurrentRow.DataBoundItem as StructureRow; return r == null ? null : r.Entity; }
        private void EditStructure(FeeStructure item) { using (FeeStructureDialog dialog = new FeeStructureDialog(item)) if (dialog.ShowDialog(this) == DialogResult.OK) { _structures.Upsert(item); LoadStructures(); } }
        private void DeleteStructure() { FeeStructure x = SelectedStructure(); if (x != null && Ui.Confirm("Delete selected fee structure?")) { _structures.Delete(x.Id); LoadStructures(); } }

        private void LoadRules() { _ruleGrid.DataSource = _rules.GetAll().Where(x => !x.IsDeleted).OrderByDescending(x => x.CreatedDate).Select(x => new RuleRow { Entity = x, Session = x.SessionName, ClassSection = x.ClassSectionName, FeeHead = x.FeeHeadName, Period = x.FeeMonth, Amount = x.Amount.ToString("0.00"), Due = x.DueDate.ToString("dd-MMM-yyyy"), Status = x.IsActive ? "Active" : "Inactive" }).ToList(); }
        private CommonFeeRule SelectedRule() { RuleRow r = _ruleGrid.CurrentRow == null ? null : _ruleGrid.CurrentRow.DataBoundItem as RuleRow; return r == null ? null : r.Entity; }
        private void CreateRule()
        {
            var sessions = _database.GetCollection<AcademicSession>("sessions").FindAll().ToList(); var sections = _database.GetCollection<ClassSection>("classsections").FindAll().ToList(); var heads = _heads.GetAll();
            if (sessions.Count == 0 || sections.Count == 0 || heads.Count == 0) { Ui.Error("Create sessions, class/sections and fee heads first."); return; }
            CommonFeeRule item = new CommonFeeRule(); using (CommonFeeRuleDialog dialog = new CommonFeeRuleDialog(item, sessions, sections, heads)) if (dialog.ShowDialog(this) == DialogResult.OK) { int count = _feeService.ApplyCommonFeeRule(item); LoadRules(); Ui.Info("Common fee saved. " + count + " student fee record(s) created."); }
        }
        private void EditRule()
        {
            CommonFeeRule item = SelectedRule(); if (item == null) return; var sessions = _database.GetCollection<AcademicSession>("sessions").FindAll().ToList(); var sections = _database.GetCollection<ClassSection>("classsections").FindAll().ToList(); var heads = _heads.GetAll();
            using (CommonFeeRuleDialog dialog = new CommonFeeRuleDialog(item, sessions, sections, heads)) if (dialog.ShowDialog(this) == DialogResult.OK) { _rules.Upsert(item); LoadRules(); Ui.Info("Rule updated. Existing generated fee records were not changed. Use 'Apply Missing' only to add missing student records."); }
        }
        private void ApplyRule() { CommonFeeRule item = SelectedRule(); if (item == null) return; try { int count = _feeService.ApplyCommonFeeRule(item); LoadRules(); Ui.Info(count + " missing fee record(s) created."); } catch (Exception ex) { Ui.Error(ex.Message); } }
        private void DeactivateRule() { CommonFeeRule item = SelectedRule(); if (item == null) return; item.IsActive = false; item.LastSyncedDate = DateTime.Now; _rules.Upsert(item); LoadRules(); }

        private class HeadRow { public FeeHead Entity { get; set; } public string Name { get; set; } public string Code { get; set; } public string Amount { get; set; } public string Frequency { get; set; } public string Status { get; set; } public string Description { get; set; } }
        private class StructureRow { public FeeStructure Entity { get; set; } public string Class { get; set; } public string FeeType { get; set; } public string Amount { get; set; } public string Frequency { get; set; } public string Effective { get; set; } public string Status { get; set; } public string Description { get; set; } }
        private class RuleRow { public CommonFeeRule Entity { get; set; } public string Session { get; set; } public string ClassSection { get; set; } public string FeeHead { get; set; } public string Period { get; set; } public string Amount { get; set; } public string Due { get; set; } public string Status { get; set; } }
    }
}
