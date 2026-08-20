using System;
using System.Drawing;
using System.Windows.Forms;

namespace RajasvSchoolManagement.UI
{
    internal static class Ui
    {
        public static readonly Font DefaultFont = new Font("Segoe UI", 9F);
        public static readonly Font HeadingFont = new Font("Segoe UI", 16F, FontStyle.Bold);
        public static readonly Font SubHeadingFont = new Font("Segoe UI", 11F, FontStyle.Bold);

        public static Button Button(string text, EventHandler click, int width)
        {
            Button button = new Button();
            button.Text = text;
            button.Width = width;
            button.Height = 34;
            button.FlatStyle = FlatStyle.System;
            button.Margin = new Padding(4);
            if (click != null) button.Click += click;
            return button;
        }

        public static Label Label(string text, int width)
        {
            Label label = new Label();
            label.Text = text;
            label.Width = width;
            label.Height = 26;
            label.TextAlign = ContentAlignment.MiddleLeft;
            return label;
        }

        public static TextBox TextBox(int width)
        {
            TextBox box = new TextBox();
            box.Width = width;
            return box;
        }

        public static NumericUpDown MoneyBox(int width)
        {
            NumericUpDown box = new NumericUpDown();
            box.Width = width;
            box.DecimalPlaces = 2;
            box.Maximum = 100000000;
            box.ThousandsSeparator = true;
            return box;
        }

        public static DataGridView Grid()
        {
            DataGridView grid = new DataGridView();
            grid.Dock = DockStyle.Fill;
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AutoGenerateColumns = false;
            grid.MultiSelect = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.RowHeadersVisible = false;
            grid.BackgroundColor = SystemColors.Window;
            grid.BorderStyle = BorderStyle.Fixed3D;
            return grid;
        }

        public static DataGridViewTextBoxColumn TextColumn(string property, string header, int fillWeight)
        {
            return new DataGridViewTextBoxColumn
            {
                DataPropertyName = property,
                Name = property,
                HeaderText = header,
                FillWeight = fillWeight,
                SortMode = DataGridViewColumnSortMode.Automatic
            };
        }

        public static Panel Header(string title, string subtitle)
        {
            Panel panel = new Panel();
            panel.Dock = DockStyle.Top;
            panel.Height = string.IsNullOrWhiteSpace(subtitle) ? 60 : 78;
            panel.Padding = new Padding(12, 8, 12, 5);

            Label titleLabel = new Label();
            titleLabel.Text = title;
            titleLabel.Font = HeadingFont;
            titleLabel.AutoSize = true;
            titleLabel.Location = new Point(12, 8);
            panel.Controls.Add(titleLabel);

            if (!string.IsNullOrWhiteSpace(subtitle))
            {
                Label subtitleLabel = new Label();
                subtitleLabel.Text = subtitle;
                subtitleLabel.AutoSize = true;
                subtitleLabel.ForeColor = SystemColors.GrayText;
                subtitleLabel.Location = new Point(14, 40);
                panel.Controls.Add(subtitleLabel);
            }
            return panel;
        }

        public static void Error(string message)
        {
            MessageBox.Show(message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public static void Info(string message)
        {
            MessageBox.Show(message, "Rajasv School Management", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public static bool Confirm(string message)
        {
            return MessageBox.Show(message, "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }
    }

    internal class LookupItem
    {
        public string Id { get; set; }
        public string Text { get; set; }
        public object Tag { get; set; }
        public override string ToString() { return Text ?? string.Empty; }
    }
}
