using System;
using System.Drawing;
using System.Windows.Forms;

namespace RajasvSchoolManagement.UI
{
    internal abstract class EntityDialogBase : Form
    {
        protected readonly TableLayoutPanel Fields;
        private int _row;

        protected EntityDialogBase(string title, int width, int height)
        {
            Text = title;
            Width = width;
            Height = height;
            StartPosition = FormStartPosition.CenterParent;
            MinimizeBox = false;
            MaximizeBox = false;
            ShowInTaskbar = false;
            Font = Ui.DefaultFont;

            Panel buttons = new Panel { Dock = DockStyle.Bottom, Height = 52, Padding = new Padding(8) };
            Button cancel = Ui.Button("Cancel", delegate { DialogResult = DialogResult.Cancel; Close(); }, 90);
            Button save = Ui.Button("Save", delegate { SaveAndClose(); }, 90);
            save.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            cancel.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            buttons.Controls.Add(cancel);
            buttons.Controls.Add(save);
            buttons.Resize += delegate
            {
                save.Left = buttons.ClientSize.Width - save.Width - 10;
                cancel.Left = save.Left - cancel.Width - 8;
            };
            Controls.Add(buttons);

            Panel scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Padding = new Padding(12) };
            Fields = new TableLayoutPanel();
            Fields.Dock = DockStyle.Top;
            Fields.AutoSize = true;
            Fields.ColumnCount = 2;
            Fields.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 150));
            Fields.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            scroll.Controls.Add(Fields);
            Controls.Add(scroll);
        }

        protected T AddField<T>(string label, T control) where T : Control
        {
            control.Dock = DockStyle.Top;
            control.Margin = new Padding(3, 4, 3, 4);
            Label fieldLabel = new Label
            {
                Text = label,
                AutoSize = false,
                Height = 30,
                Dock = DockStyle.Top,
                TextAlign = ContentAlignment.MiddleLeft,
                Margin = new Padding(3, 4, 3, 4)
            };
            Fields.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            Fields.Controls.Add(fieldLabel, 0, _row);
            Fields.Controls.Add(control, 1, _row);
            _row++;
            return control;
        }

        protected TextBox AddText(string label, string value)
        {
            TextBox box = new TextBox { Text = value ?? string.Empty };
            return AddField(label, box);
        }

        protected DateTimePicker AddDate(string label, DateTime value)
        {
            DateTimePicker picker = new DateTimePicker { Format = DateTimePickerFormat.Short, Value = value };
            return AddField(label, picker);
        }

        protected NumericUpDown AddMoney(string label, decimal value)
        {
            NumericUpDown box = Ui.MoneyBox(220);
            box.Value = Math.Max(box.Minimum, Math.Min(box.Maximum, value));
            return AddField(label, box);
        }

        protected NumericUpDown AddInteger(string label, int value)
        {
            NumericUpDown box = new NumericUpDown { Minimum = 0, Maximum = 100000, Value = Math.Max(0, value) };
            return AddField(label, box);
        }

        protected CheckBox AddCheck(string label, bool value)
        {
            CheckBox check = new CheckBox { Checked = value, Text = "Yes", AutoSize = true };
            return AddField(label, check);
        }

        protected ComboBox AddCombo(string label)
        {
            ComboBox combo = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList };
            return AddField(label, combo);
        }

        private void SaveAndClose()
        {
            try
            {
                string validation = ValidateValues();
                if (!string.IsNullOrWhiteSpace(validation))
                {
                    Ui.Error(validation);
                    return;
                }
                ApplyValues();
                DialogResult = DialogResult.OK;
                Close();
            }
            catch (Exception ex)
            {
                Ui.Error(ex.Message);
            }
        }

        protected virtual string ValidateValues() { return string.Empty; }
        protected abstract void ApplyValues();
    }
}
