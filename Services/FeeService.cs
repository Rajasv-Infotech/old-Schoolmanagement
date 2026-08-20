using System;
using System.Collections.Generic;
using System.Linq;
using LiteDB;
using RajasvSchoolManagement.Data;
using RajasvSchoolManagement.Models;

namespace RajasvSchoolManagement.Services
{
    public class FeeService
    {
        private readonly DatabaseService _database;

        public FeeService(DatabaseService database)
        {
            _database = database;
        }

        public int ApplyCommonFeeRule(CommonFeeRule rule)
        {
            if (rule == null) throw new ArgumentNullException("rule");
            if (string.IsNullOrWhiteSpace(rule.ClassSectionId)) throw new InvalidOperationException("Class section is required.");
            if (string.IsNullOrWhiteSpace(rule.FeeHeadId)) throw new InvalidOperationException("Fee head is required.");
            if (rule.Amount <= 0) throw new InvalidOperationException("Fee amount must be greater than zero.");

            int created = 0;
            _database.RunInTransaction(delegate(LiteDatabase db)
            {
                ILiteCollection<CommonFeeRule> rules = db.GetCollection<CommonFeeRule>("commonfeerules");
                ILiteCollection<Student> students = db.GetCollection<Student>("students");
                ILiteCollection<FeeRecord> records = db.GetCollection<FeeRecord>("feerecords");

                rules.Upsert(rule);
                List<Student> targets = students.Find(s => s.ClassSectionId == rule.ClassSectionId && s.IsActive).ToList();
                foreach (Student student in targets)
                {
                    bool exists = records.Exists(x => x.StudentId == student.Id &&
                                                       x.CommonFeeRuleId == rule.Id &&
                                                       x.FeeHeadId == rule.FeeHeadId &&
                                                       x.FeeMonth == rule.FeeMonth);
                    if (exists) continue;

                    FeeRecord record = new FeeRecord
                    {
                        StudentId = student.Id,
                        StudentName = StudentDisplayName(student),
                        StudentClass = student.Class,
                        RollNumber = student.RollNumber,
                        SessionId = student.SessionId,
                        SessionName = student.SessionName,
                        ClassSectionId = student.ClassSectionId,
                        ClassSectionName = string.IsNullOrWhiteSpace(student.Section) ? student.Class : student.Class + " - " + student.Section,
                        AcademicYear = student.SessionName,
                        FeeHeadId = rule.FeeHeadId,
                        FeeType = rule.FeeHeadName,
                        FeeMonth = rule.FeeMonth,
                        ChargeType = "Common",
                        CommonFeeRuleId = rule.Id,
                        Amount = rule.Amount,
                        DueDate = rule.DueDate,
                        Status = "Pending",
                        Remarks = rule.Remarks,
                        CreatedDate = DateTime.Now
                    };
                    records.Insert(record);
                    created++;
                }
            });

            return created;
        }

        public FeeRecord AddStudentFee(Student student, FeeHead head, decimal amount, string feeMonth, DateTime dueDate, decimal discount, decimal lateFee, string remarks)
        {
            if (student == null) throw new ArgumentNullException("student");
            if (head == null) throw new ArgumentNullException("head");
            if (amount <= 0) throw new InvalidOperationException("Amount must be greater than zero.");
            if (discount < 0 || lateFee < 0) throw new InvalidOperationException("Discount and late fee cannot be negative.");
            if (discount > amount + lateFee) throw new InvalidOperationException("Discount cannot exceed the total charge.");

            FeeRecord record = new FeeRecord
            {
                StudentId = student.Id,
                StudentName = StudentDisplayName(student),
                StudentClass = student.Class,
                RollNumber = student.RollNumber,
                SessionId = student.SessionId,
                SessionName = student.SessionName,
                ClassSectionId = student.ClassSectionId,
                ClassSectionName = string.IsNullOrWhiteSpace(student.Section) ? student.Class : student.Class + " - " + student.Section,
                AcademicYear = student.SessionName,
                FeeHeadId = head.Id,
                FeeType = head.Name,
                FeeMonth = feeMonth ?? string.Empty,
                ChargeType = "Student",
                Amount = amount,
                Discount = discount,
                LateFee = lateFee,
                DueDate = dueDate,
                Status = "Pending",
                Remarks = remarks ?? string.Empty,
                CreatedDate = DateTime.Now
            };
            _database.GetCollection<FeeRecord>("feerecords").Insert(record);
            return record;
        }

        public FeePaymentReceipt TakePayment(
            string studentId,
            IDictionary<string, decimal> allocations,
            string paymentMethod,
            string transactionId,
            string remarks,
            string receivedBy)
        {
            if (string.IsNullOrWhiteSpace(studentId)) throw new InvalidOperationException("Student is required.");
            if (allocations == null || allocations.Count == 0) throw new InvalidOperationException("Select at least one fee record.");

            FeePaymentReceipt receipt = null;
            _database.RunInTransaction(delegate(LiteDatabase db)
            {
                ILiteCollection<Student> students = db.GetCollection<Student>("students");
                ILiteCollection<FeeRecord> records = db.GetCollection<FeeRecord>("feerecords");
                ILiteCollection<FeePaymentReceipt> receipts = db.GetCollection<FeePaymentReceipt>("receipts");
                SchoolSettings settings = db.GetCollection<SchoolSettings>("settings").FindById("default") ?? new SchoolSettings();
                Student student = students.FindById(studentId);
                if (student == null) throw new InvalidOperationException("Student not found.");

                string receiptNumber = BuildReceiptNumber(settings.ReceiptPrefix);
                receipt = new FeePaymentReceipt
                {
                    ReceiptNumber = receiptNumber,
                    ReceiptDate = DateTime.Now,
                    StudentId = student.Id,
                    StudentName = StudentDisplayName(student),
                    RollNumber = student.RollNumber,
                    SessionName = student.SessionName,
                    ClassSectionName = string.IsNullOrWhiteSpace(student.Section) ? student.Class : student.Class + " - " + student.Section,
                    PaymentMethod = string.IsNullOrWhiteSpace(paymentMethod) ? "Cash" : paymentMethod,
                    TransactionId = transactionId ?? string.Empty,
                    Remarks = remarks ?? string.Empty,
                    ReceivedBy = string.IsNullOrWhiteSpace(receivedBy) ? settings.ReceivedBy : receivedBy
                };

                decimal total = 0m;
                foreach (KeyValuePair<string, decimal> allocation in allocations)
                {
                    if (allocation.Value <= 0) continue;
                    FeeRecord record = records.FindById(allocation.Key);
                    if (record == null || record.StudentId != studentId) continue;

                    decimal beforeBalance = record.BalanceAmount;
                    if (beforeBalance <= 0) continue;
                    decimal amount = Math.Min(allocation.Value, beforeBalance);

                    record.PaidAmount += amount;
                    record.PaymentDate = DateTime.Now;
                    record.PaymentMethod = receipt.PaymentMethod;
                    record.TransactionId = receipt.TransactionId;
                    record.ReceiptNumber = receiptNumber;
                    record.LastUpdated = DateTime.Now;
                    record.Status = record.BalanceAmount <= 0.009m ? "Paid" : "Partial";
                    records.Update(record);

                    receipt.Items.Add(new FeePaymentReceiptItem
                    {
                        FeeRecordId = record.Id,
                        FeeHeadName = record.FeeType,
                        FeeMonth = record.FeeMonth,
                        DueAmount = beforeBalance,
                        PaidAmount = amount,
                        BalanceAfterPayment = record.BalanceAmount
                    });
                    total += amount;
                }

                if (total <= 0) throw new InvalidOperationException("No valid payment amount was entered.");
                receipt.ReceiptAmount = total;
                receipts.Insert(receipt);
            });
            return receipt;
        }

        public void RefreshOverdueStatuses()
        {
            ILiteCollection<FeeRecord> records = _database.GetCollection<FeeRecord>("feerecords");
            List<FeeRecord> due = records.Find(x => x.DueDate < DateTime.Today && (x.Status == "Pending" || x.Status == "Partial" || x.Status == "Overdue")).ToList();
            foreach (FeeRecord item in due)
            {
                if (item.BalanceAmount > 0.009m)
                {
                    item.Status = item.PaidAmount > 0 ? "Partial" : "Overdue";
                    records.Update(item);
                }
            }
        }

        private static string StudentDisplayName(Student student)
        {
            if (!string.IsNullOrWhiteSpace(student.FullName)) return student.FullName.Trim();
            return (student.FirstName + " " + student.LastName).Trim();
        }

        private static string BuildReceiptNumber(string prefix)
        {
            prefix = string.IsNullOrWhiteSpace(prefix) ? "RCPT" : prefix.Trim().ToUpperInvariant();
            return prefix + "-" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
        }
    }
}
