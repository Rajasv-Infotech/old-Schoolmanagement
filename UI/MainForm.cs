using System;
using System.Drawing;
using System.Windows.Forms;
using RajasvSchoolManagement.Data;
using RajasvSchoolManagement.Models;
using RajasvSchoolManagement.Services;
using RajasvSchoolManagement.UI.Controls;

namespace RajasvSchoolManagement.UI
{
    public class MainForm : Form
    {
        private readonly DatabaseService _database;
        private readonly BackupService _backup;
        private readonly Panel _content;
        private readonly Label _title;

        public MainForm(DatabaseService database, BackupService backup)
        {
            _database = database;
            _backup = backup;

            Text = "Rajasv Infotech - School Management";
            WindowState = FormWindowState.Maximized;
            MinimumSize = new Size(1024, 650);
            StartPosition = FormStartPosition.CenterScreen;
            Font = Ui.DefaultFont;

            // Use an explicit table layout instead of mixing Left/Top/Fill controls
            // directly on the Form. The old Dock/z-order combination allowed the
            // content panel to extend underneath the sidebar, which hid the first
            // part of every page (page title, first card, first grid column, etc.).
            TableLayoutPanel shell = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 3,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                BackColor = SystemColors.Control
            };
            shell.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 215F));
            shell.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 66F));
            shell.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
            shell.RowStyles.Add(new RowStyle(SizeType.Absolute, 24F));
            Controls.Add(shell);

            FlowLayoutPanel sidebar = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                Padding = new Padding(8),
                AutoScroll = true,
                BackColor = SystemColors.ControlLightLight,
                FlowDirection = FlowDirection.TopDown,
                WrapContents = false
            };
            AddNav(sidebar, "Dashboard", delegate { ShowControl(new DashboardControl(_database)); });
            AddNav(sidebar, "Academic Sessions", delegate { ShowControl(new SessionsControl(_database)); });
            AddNav(sidebar, "Classes & Sections", delegate { ShowControl(new ClassSectionsControl(_database)); });
            AddNav(sidebar, "Students", delegate { ShowControl(new StudentsControl(_database)); });
            AddNav(sidebar, "Teachers", delegate { ShowControl(new TeachersControl(_database)); });
            AddNav(sidebar, "Attendance", delegate { ShowControl(new AttendanceControl(_database)); });
            AddNav(sidebar, "Fee Setup", delegate { ShowControl(new FeeSetupControl(_database)); });
            AddNav(sidebar, "Fee Records & Payments", delegate { ShowControl(new FeeRecordsControl(_database)); });
            AddNav(sidebar, "Reports", delegate { ShowControl(new ReportsControl(_database)); });
            AddNav(sidebar, "Settings / Backup", delegate { ShowControl(new SettingsControl(_database, _backup)); });
            shell.Controls.Add(sidebar, 0, 0);
            shell.SetRowSpan(sidebar, 2);

            Panel top = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                Padding = new Padding(18, 10, 10, 6),
                BackColor = SystemColors.ControlLight
            };
            _title = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                Location = new Point(18, 8)
            };
            Label brand = new Label
            {
                Text = "Rajasv Infotech • Desktop Offline Edition",
                AutoSize = true,
                ForeColor = SystemColors.GrayText,
                Location = new Point(20, 38)
            };
            top.Controls.Add(_title);
            top.Controls.Add(brand);
            shell.Controls.Add(top, 1, 0);

            _content = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                BackColor = SystemColors.Control
            };
            shell.Controls.Add(_content, 1, 1);

            StatusStrip status = new StatusStrip
            {
                Dock = DockStyle.Fill,
                Margin = Padding.Empty,
                SizingGrip = false
            };
            status.Items.Add(new ToolStripStatusLabel("Offline / Local Database"));
            status.Items.Add(new ToolStripStatusLabel { Spring = true });
            status.Items.Add(new ToolStripStatusLabel(DataPaths.DatabaseFile));
            shell.Controls.Add(status, 0, 2);
            shell.SetColumnSpan(status, 2);

            RefreshSchoolTitle();
            ShowControl(new DashboardControl(_database));
        }

        private void AddNav(FlowLayoutPanel sidebar, string text, EventHandler click)
        {
            Button button = new Button
            {
                Text = text,
                Width = 190,
                Height = 42,
                Margin = new Padding(2, 2, 2, 3),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(10, 0, 0, 0),
                FlatStyle = FlatStyle.System
            };
            button.Click += click;
            sidebar.Controls.Add(button);
        }

        private void ShowControl(Control control)
        {
            _content.SuspendLayout();
            try
            {
                while (_content.Controls.Count > 0)
                {
                    Control old = _content.Controls[0];
                    _content.Controls.RemoveAt(0);
                    old.Dispose();
                }

                control.Dock = DockStyle.Fill;
                control.Margin = Padding.Empty;
                _content.Controls.Add(control);
            }
            finally
            {
                _content.ResumeLayout(true);
            }

            RefreshSchoolTitle();
        }

        private void RefreshSchoolTitle()
        {
            try
            {
                SchoolSettings settings = _database.GetCollection<SchoolSettings>("settings").FindById("default") ?? new SchoolSettings();
                _title.Text = settings.SchoolName;
                Text = settings.SchoolName + " - School Management";
            }
            catch
            {
                _title.Text = "School Management";
            }
        }
    }
}
