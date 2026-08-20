using System;
using LiteDB;

namespace RajasvSchoolManagement.Models
{
    public class FeeRecord
    {
        [BsonId]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string StudentClass { get; set; } = string.Empty;
        public string RollNumber { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;
        public string SessionName { get; set; } = string.Empty;
        public string ClassSectionId { get; set; } = string.Empty;
        public string ClassSectionName { get; set; } = string.Empty;
        public string AcademicYear { get; set; } = DateTime.Now.Year.ToString();
        public string FeeHeadId { get; set; } = string.Empty;
        public string FeeType { get; set; } = string.Empty;
        public string FeeMonth { get; set; } = string.Empty;
        public string ChargeType { get; set; } = "Student";
        public string CommonFeeRuleId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public decimal LateFee { get; set; }
        public decimal Discount { get; set; }
        public DateTime DueDate { get; set; } = DateTime.Today.AddDays(30);
        public DateTime? PaymentDate { get; set; }
        public string Status { get; set; } = "Pending";
        public string PaymentMethod { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public string ReceiptNumber { get; set; } = string.Empty;
        public decimal PaidAmount { get; set; }
        public string Remarks { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? LastUpdated { get; set; }

        [BsonIgnore]
        public decimal TotalAmount { get { return Amount + LateFee - Discount; } }

        [BsonIgnore]
        public decimal BalanceAmount { get { return TotalAmount - PaidAmount; } }
    }
}
