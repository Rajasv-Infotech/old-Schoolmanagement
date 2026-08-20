using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using RajasvSchoolManagement.Models;

namespace RajasvSchoolManagement.UI
{
    internal class SessionDialog : EntityDialogBase
    {
        private readonly AcademicSession _item;
        private readonly TextBox _name;
        private readonly DateTimePicker _start;
        private readonly DateTimePicker _end;
        private readonly CheckBox _active;

        public SessionDialog(AcademicSession item) : base(item == null ? "Add Academic Session" : "Edit Academic Session", 520, 340)
        {
            _item = item ?? new AcademicSession();
            _name = AddText("Session name", _item.Name);
            _start = AddDate("Start date", _item.StartDate);
            _end = AddDate("End date", _item.EndDate);
            _active = AddCheck("Active", _item.IsActive);
        }

        public AcademicSession Item { get { return _item; } }
        protected override string ValidateValues()
        {
            if (string.IsNullOrWhiteSpace(_name.Text)) return "Session name is required.";
            if (_end.Value.Date < _start.Value.Date) return "End date cannot be before start date.";
            return string.Empty;
        }
        protected override void ApplyValues()
        {
            _item.Name = _name.Text.Trim();
            _item.StartDate = _start.Value.Date;
            _item.EndDate = _end.Value.Date;
            _item.IsActive = _active.Checked;
        }
    }

    internal class ClassSectionDialog : EntityDialogBase
    {
        private readonly ClassSection _item;
        private readonly ComboBox _session;
        private readonly TextBox _className;
        private readonly TextBox _sectionName;
        private readonly NumericUpDown _capacity;
        private readonly CheckBox _active;

        public ClassSectionDialog(ClassSection item, IList<AcademicSession> sessions) : base(item == null ? "Add Class / Section" : "Edit Class / Section", 560, 390)
        {
            _item = item ?? new ClassSection();
            _session = AddCombo("Academic session");
            foreach (AcademicSession session in sessions.OrderByDescending(x => x.StartDate))
                _session.Items.Add(new LookupItem { Id = session.Id, Text = session.Name, Tag = session });
            SelectLookup(_session, _item.SessionId);
            _className = AddText("Class", _item.ClassName);
            _sectionName = AddText("Section", _item.SectionName);
            _capacity = AddInteger("Capacity", _item.Capacity);
            _active = AddCheck("Active", _item.IsActive);
        }

        public ClassSection Item { get { return _item; } }
        protected override string ValidateValues()
        {
            if (_session.SelectedItem == null) return "Academic session is required.";
            if (string.IsNullOrWhiteSpace(_className.Text)) return "Class name is required.";
            return string.Empty;
        }
        protected override void ApplyValues()
        {
            LookupItem session = (LookupItem)_session.SelectedItem;
            _item.SessionId = session.Id;
            _item.SessionName = session.Text;
            _item.ClassName = _className.Text.Trim();
            _item.SectionName = _sectionName.Text.Trim();
            _item.Capacity = (int)_capacity.Value;
            _item.IsActive = _active.Checked;
        }

        internal static void SelectLookup(ComboBox combo, string id)
        {
            for (int i = 0; i < combo.Items.Count; i++)
            {
                LookupItem item = combo.Items[i] as LookupItem;
                if (item != null && string.Equals(item.Id, id, StringComparison.OrdinalIgnoreCase))
                {
                    combo.SelectedIndex = i;
                    return;
                }
            }
            if (combo.Items.Count > 0) combo.SelectedIndex = 0;
        }
    }

    internal class StudentDialog : EntityDialogBase
    {
        private readonly Student _item;
        private readonly IList<ClassSection> _sections;
        private readonly ComboBox _session;
        private readonly ComboBox _section;
        private readonly TextBox _fullName;
        private readonly TextBox _roll;
        private readonly TextBox _email;
        private readonly TextBox _phone;
        private readonly DateTimePicker _dob;
        private readonly DateTimePicker _admission;
        private readonly TextBox _address;
        private readonly TextBox _father;
        private readonly TextBox _fatherPhone;
        private readonly TextBox _mother;
        private readonly TextBox _motherPhone;
        private readonly TextBox _guardian;
        private readonly TextBox _guardianPhone;
        private readonly CheckBox _active;

        public StudentDialog(Student item, IList<AcademicSession> sessions, IList<ClassSection> sections)
            : base(item == null ? "New Student Admission" : "Edit Student Admission", 700, 720)
        {
            _item = item ?? new Student { EnrollmentDate = DateTime.Today, DateOfBirth = DateTime.Today.AddYears(-10) };
            _sections = sections;
            _fullName = AddText("Full name", DisplayName(_item));
            _session = AddCombo("Academic session");
            foreach (AcademicSession session in sessions.OrderByDescending(x => x.StartDate))
                _session.Items.Add(new LookupItem { Id = session.Id, Text = session.Name, Tag = session });
            _session.SelectedIndexChanged += delegate { LoadSections(); };
            ClassSectionDialog.SelectLookup(_session, _item.SessionId);

            _section = AddCombo("Class / Section");
            LoadSections();
            ClassSectionDialog.SelectLookup(_section, _item.ClassSectionId);
            _roll = AddText("Roll number", _item.RollNumber);
            _email = AddText("Email", _item.Email);
            _phone = AddText("Phone", _item.Phone);
            _dob = AddDate("Date of birth", SafeDate(_item.DateOfBirth, DateTime.Today.AddYears(-10)));
            _admission = AddDate("Admission date", SafeDate(_item.EnrollmentDate, DateTime.Today));
            _address = AddText("Address", _item.Address);
            _father = AddText("Father's name", _item.FatherName);
            _fatherPhone = AddText("Father's contact", _item.FatherContact);
            _mother = AddText("Mother's name", _item.MotherName);
            _motherPhone = AddText("Mother's contact", _item.MotherContact);
            _guardian = AddText("Guardian name", _item.GuardianName);
            _guardianPhone = AddText("Guardian contact", _item.GuardianContact);
            _active = AddCheck("Active", _item.IsActive);
        }

        public Student Item { get { return _item; } }
        protected override string ValidateValues()
        {
            if (string.IsNullOrWhiteSpace(_fullName.Text)) return "Student full name is required.";
            if (_session.SelectedItem == null) return "Academic session is required.";
            if (_section.SelectedItem == null) return "Class / section is required.";
            return string.Empty;
        }
        protected override void ApplyValues()
        {
            LookupItem session = (LookupItem)_session.SelectedItem;
            LookupItem sectionItem = (LookupItem)_section.SelectedItem;
            ClassSection section = (ClassSection)sectionItem.Tag;
            _item.FullName = _fullName.Text.Trim();
            _item.FirstName = _item.FullName;
            _item.LastName = string.Empty;
            _item.SessionId = session.Id;
            _item.SessionName = session.Text;
            _item.ClassSectionId = section.Id;
            _item.Class = section.ClassName;
            _item.Section = section.SectionName;
            _item.RollNumber = _roll.Text.Trim();
            _item.Email = _email.Text.Trim();
            _item.Phone = _phone.Text.Trim();
            _item.DateOfBirth = _dob.Value.Date;
            _item.EnrollmentDate = _admission.Value.Date;
            _item.Address = _address.Text.Trim();
            _item.FatherName = _father.Text.Trim();
            _item.FatherContact = _fatherPhone.Text.Trim();
            _item.MotherName = _mother.Text.Trim();
            _item.MotherContact = _motherPhone.Text.Trim();
            _item.GuardianName = _guardian.Text.Trim();
            _item.GuardianContact = _guardianPhone.Text.Trim();
            _item.ParentName = _item.FatherName;
            _item.ParentContact = _item.FatherContact;
            _item.IsActive = _active.Checked;
        }
        private void LoadSections()
        {
            if (_section == null) return;
            string current = (_section.SelectedItem as LookupItem)?.Id ?? _item.ClassSectionId;
            _section.Items.Clear();
            LookupItem selectedSession = _session.SelectedItem as LookupItem;
            if (selectedSession == null) return;
            foreach (ClassSection section in _sections.Where(x => x.SessionId == selectedSession.Id && x.IsActive).OrderBy(x => x.ClassName).ThenBy(x => x.SectionName))
                _section.Items.Add(new LookupItem { Id = section.Id, Text = section.DisplayName, Tag = section });
            ClassSectionDialog.SelectLookup(_section, current);
        }
        private static DateTime SafeDate(DateTime value, DateTime fallback)
        {
            return value < new DateTime(1753, 1, 1) || value > DateTimePicker.MaximumDateTime ? fallback : value;
        }
        private static string DisplayName(Student s)
        {
            return !string.IsNullOrWhiteSpace(s.FullName) ? s.FullName : (s.FirstName + " " + s.LastName).Trim();
        }
    }

    internal class TeacherDialog : EntityDialogBase
    {
        private readonly Teacher _item;
        private readonly TextBox _first, _last, _email, _phone, _subject, _qualification;
        private readonly DateTimePicker _joining;
        private readonly NumericUpDown _salary;
        private readonly CheckBox _active;

        public TeacherDialog(Teacher item) : base(item == null ? "Add Teacher" : "Edit Teacher", 570, 520)
        {
            _item = item ?? new Teacher();
            _first = AddText("First name", _item.FirstName);
            _last = AddText("Last name", _item.LastName);
            _email = AddText("Email", _item.Email);
            _phone = AddText("Phone", _item.Phone);
            _subject = AddText("Subject", _item.Subject);
            _qualification = AddText("Qualification", _item.Qualification);
            _joining = AddDate("Joining date", _item.JoiningDate);
            _salary = AddMoney("Salary", _item.Salary);
            _active = AddCheck("Active", _item.IsActive);
        }
        public Teacher Item { get { return _item; } }
        protected override string ValidateValues() { return string.IsNullOrWhiteSpace(_first.Text) ? "Teacher first name is required." : string.Empty; }
        protected override void ApplyValues()
        {
            _item.FirstName = _first.Text.Trim(); _item.LastName = _last.Text.Trim(); _item.Email = _email.Text.Trim();
            _item.Phone = _phone.Text.Trim(); _item.Subject = _subject.Text.Trim(); _item.Qualification = _qualification.Text.Trim();
            _item.JoiningDate = _joining.Value.Date; _item.Salary = _salary.Value; _item.IsActive = _active.Checked;
        }
    }

    internal class FeeHeadDialog : EntityDialogBase
    {
        private readonly FeeHead _item;
        private readonly TextBox _name, _code, _description;
        private readonly NumericUpDown _amount;
        private readonly ComboBox _frequency;
        private readonly CheckBox _active;

        public FeeHeadDialog(FeeHead item) : base(item == null ? "Add Fee Head" : "Edit Fee Head", 570, 470)
        {
            _item = item ?? new FeeHead();
            _name = AddText("Name", _item.Name);
            _code = AddText("Code", _item.Code);
            _amount = AddMoney("Default amount", _item.DefaultAmount);
            _frequency = AddCombo("Frequency");
            _frequency.Items.AddRange(new object[] { "Monthly", "Quarterly", "Yearly", "One-time" });
            _frequency.SelectedItem = _item.Frequency;
            if (_frequency.SelectedIndex < 0) _frequency.SelectedIndex = 0;
            _description = AddText("Description", _item.Description);
            _active = AddCheck("Active", _item.IsActive);
        }
        public FeeHead Item { get { return _item; } }
        protected override string ValidateValues() { return string.IsNullOrWhiteSpace(_name.Text) ? "Fee head name is required." : string.Empty; }
        protected override void ApplyValues()
        {
            _item.Name = _name.Text.Trim(); _item.Code = _code.Text.Trim(); _item.DefaultAmount = _amount.Value;
            _item.Frequency = Convert.ToString(_frequency.SelectedItem); _item.Description = _description.Text.Trim(); _item.IsActive = _active.Checked;
        }
    }

    internal class FeeStructureDialog : EntityDialogBase
    {
        private readonly FeeStructure _item;
        private readonly TextBox _class, _type, _description;
        private readonly NumericUpDown _amount;
        private readonly ComboBox _frequency;
        private readonly DateTimePicker _effective;
        private readonly CheckBox _active;

        public FeeStructureDialog(FeeStructure item) : base(item == null ? "Add Fee Structure" : "Edit Fee Structure", 580, 500)
        {
            _item = item ?? new FeeStructure();
            _class = AddText("Class", _item.Class);
            _type = AddText("Fee type", _item.FeeType);
            _amount = AddMoney("Amount", _item.Amount);
            _frequency = AddCombo("Frequency"); _frequency.Items.AddRange(new object[] { "Monthly", "Quarterly", "Yearly", "One-time" });
            _frequency.SelectedItem = _item.Frequency; if (_frequency.SelectedIndex < 0) _frequency.SelectedIndex = 0;
            _effective = AddDate("Effective from", _item.EffectiveFrom);
            _description = AddText("Description", _item.Description);
            _active = AddCheck("Active", _item.IsActive);
        }
        public FeeStructure Item { get { return _item; } }
        protected override string ValidateValues() { return string.IsNullOrWhiteSpace(_class.Text) || string.IsNullOrWhiteSpace(_type.Text) ? "Class and fee type are required." : string.Empty; }
        protected override void ApplyValues()
        {
            _item.Class = _class.Text.Trim(); _item.FeeType = _type.Text.Trim(); _item.Amount = _amount.Value;
            _item.Frequency = Convert.ToString(_frequency.SelectedItem); _item.EffectiveFrom = _effective.Value.Date;
            _item.Description = _description.Text.Trim(); _item.IsActive = _active.Checked;
        }
    }

    internal class CommonFeeRuleDialog : EntityDialogBase
    {
        private readonly CommonFeeRule _item;
        private readonly IList<ClassSection> _sections;
        private readonly ComboBox _session, _section, _head;
        private readonly TextBox _month, _remarks;
        private readonly NumericUpDown _amount;
        private readonly DateTimePicker _due;
        private readonly CheckBox _active;

        public CommonFeeRuleDialog(CommonFeeRule item, IList<AcademicSession> sessions, IList<ClassSection> sections, IList<FeeHead> heads)
            : base(item == null ? "Create Common Fee" : "Edit Common Fee", 620, 560)
        {
            _item = item ?? new CommonFeeRule(); _sections = sections;
            _session = AddCombo("Session"); foreach (AcademicSession x in sessions.OrderByDescending(x => x.StartDate)) _session.Items.Add(new LookupItem { Id = x.Id, Text = x.Name, Tag = x });
            _session.SelectedIndexChanged += delegate { LoadSections(); };
            ClassSectionDialog.SelectLookup(_session, _item.SessionId);
            _section = AddCombo("Class / Section"); LoadSections(); ClassSectionDialog.SelectLookup(_section, _item.ClassSectionId);
            _head = AddCombo("Fee head"); foreach (FeeHead x in heads.Where(x => x.IsActive).OrderBy(x => x.Name)) _head.Items.Add(new LookupItem { Id = x.Id, Text = x.Name, Tag = x });
            ClassSectionDialog.SelectLookup(_head, _item.FeeHeadId);
            _head.SelectedIndexChanged += delegate { FeeHead h = (_head.SelectedItem as LookupItem)?.Tag as FeeHead; if (h != null && _amount.Value == 0) _amount.Value = h.DefaultAmount; };
            _month = AddText("Fee month / period", _item.FeeMonth);
            _amount = AddMoney("Amount", _item.Amount);
            _due = AddDate("Due date", _item.DueDate);
            _remarks = AddText("Remarks", _item.Remarks);
            _active = AddCheck("Active", _item.IsActive);
        }
        public CommonFeeRule Item { get { return _item; } }
        protected override string ValidateValues()
        {
            if (_session.SelectedItem == null || _section.SelectedItem == null || _head.SelectedItem == null) return "Session, class/section and fee head are required.";
            if (_amount.Value <= 0) return "Amount must be greater than zero.";
            return string.Empty;
        }
        protected override void ApplyValues()
        {
            LookupItem session = (LookupItem)_session.SelectedItem; LookupItem section = (LookupItem)_section.SelectedItem; LookupItem head = (LookupItem)_head.SelectedItem;
            _item.SessionId = session.Id; _item.SessionName = session.Text; _item.ClassSectionId = section.Id; _item.ClassSectionName = section.Text;
            _item.FeeHeadId = head.Id; _item.FeeHeadName = head.Text; _item.FeeMonth = _month.Text.Trim(); _item.Amount = _amount.Value;
            _item.DueDate = _due.Value.Date; _item.Remarks = _remarks.Text.Trim(); _item.IsActive = _active.Checked; _item.IsDeleted = false;
        }
        private void LoadSections()
        {
            if (_section == null) return; string current = (_section.SelectedItem as LookupItem)?.Id ?? _item.ClassSectionId; _section.Items.Clear();
            LookupItem session = _session.SelectedItem as LookupItem; if (session == null) return;
            foreach (ClassSection x in _sections.Where(x => x.SessionId == session.Id && x.IsActive).OrderBy(x => x.ClassName).ThenBy(x => x.SectionName))
                _section.Items.Add(new LookupItem { Id = x.Id, Text = x.DisplayName, Tag = x });
            ClassSectionDialog.SelectLookup(_section, current);
        }
    }

    internal class IndividualFeeDialog : EntityDialogBase
    {
        private readonly ComboBox _student, _head;
        private readonly TextBox _month, _remarks;
        private readonly NumericUpDown _amount, _discount, _lateFee;
        private readonly DateTimePicker _due;
        public Student SelectedStudent { get; private set; }
        public FeeHead SelectedHead { get; private set; }
        public decimal Amount { get; private set; }
        public decimal Discount { get; private set; }
        public decimal LateFee { get; private set; }
        public DateTime DueDate { get; private set; }
        public string FeeMonth { get; private set; }
        public string Remarks { get; private set; }

        public IndividualFeeDialog(IList<Student> students, IList<FeeHead> heads) : base("Add Student Fee", 620, 540)
        {
            _student = AddCombo("Student"); foreach (Student x in students.Where(x => x.IsActive).OrderBy(x => x.FullName)) _student.Items.Add(new LookupItem { Id = x.Id, Text = DisplayStudent(x), Tag = x }); if (_student.Items.Count > 0) _student.SelectedIndex = 0;
            _head = AddCombo("Fee head"); foreach (FeeHead x in heads.Where(x => x.IsActive).OrderBy(x => x.Name)) _head.Items.Add(new LookupItem { Id = x.Id, Text = x.Name, Tag = x }); if (_head.Items.Count > 0) _head.SelectedIndex = 0;
            _amount = AddMoney("Amount", 0m); _head.SelectedIndexChanged += delegate { FeeHead h = (_head.SelectedItem as LookupItem)?.Tag as FeeHead; if (h != null) _amount.Value = h.DefaultAmount; };
            FeeHead first = (_head.SelectedItem as LookupItem)?.Tag as FeeHead; if (first != null) _amount.Value = first.DefaultAmount;
            _month = AddText("Fee month / period", DateTime.Today.ToString("MMM yyyy"));
            _due = AddDate("Due date", DateTime.Today.AddDays(30));
            _discount = AddMoney("Discount", 0m); _lateFee = AddMoney("Late fee", 0m); _remarks = AddText("Remarks", string.Empty);
        }
        protected override string ValidateValues() { return _student.SelectedItem == null || _head.SelectedItem == null || _amount.Value <= 0 ? "Student, fee head and valid amount are required." : string.Empty; }
        protected override void ApplyValues()
        {
            SelectedStudent = (Student)((LookupItem)_student.SelectedItem).Tag; SelectedHead = (FeeHead)((LookupItem)_head.SelectedItem).Tag;
            Amount = _amount.Value; Discount = _discount.Value; LateFee = _lateFee.Value; DueDate = _due.Value.Date; FeeMonth = _month.Text.Trim(); Remarks = _remarks.Text.Trim();
        }
        private static string DisplayStudent(Student s) { string name = !string.IsNullOrWhiteSpace(s.FullName) ? s.FullName : (s.FirstName + " " + s.LastName).Trim(); return name + " | " + s.Class + "-" + s.Section + " | Roll " + s.RollNumber; }
    }
}
