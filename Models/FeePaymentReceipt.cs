using System;
using System.Collections.Generic;
using LiteDB;

namespace RajasvSchoolManagement.Models
{
    public class FeePaymentReceipt
    {
        [BsonId]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string ReceiptNumber { get; set; } = string.Empty;
        public DateTime ReceiptDate { get; set; } = DateTime.Now;
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public string RollNumber { get; set; } = string.Empty;
        public string SessionName { get; set; } = string.Empty;
        public string ClassSectionName { get; set; } = string.Empty;
        public decimal ReceiptAmount { get; set; }
        public string PaymentMethod { get; set; } = "Cash";
        public string TransactionId { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
        public string ReceivedBy { get; set; } = string.Empty;
        public List<FeePaymentReceiptItem> Items { get; set; } = new List<FeePaymentReceiptItem>();
    }

    public class FeePaymentReceiptItem
    {
        public string FeeRecordId { get; set; } = string.Empty;
        public string FeeHeadName { get; set; } = string.Empty;
        public string FeeMonth { get; set; } = string.Empty;
        public decimal DueAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal BalanceAfterPayment { get; set; }
    }
}
