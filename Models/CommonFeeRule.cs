using System;
using LiteDB;

namespace RajasvSchoolManagement.Models
{
    public class CommonFeeRule
    {
        [BsonId]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string SessionId { get; set; } = string.Empty;
        public string SessionName { get; set; } = string.Empty;
        public string ClassSectionId { get; set; } = string.Empty;
        public string ClassSectionName { get; set; } = string.Empty;
        public string FeeHeadId { get; set; } = string.Empty;
        public string FeeHeadName { get; set; } = string.Empty;
        public string FeeMonth { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public DateTime DueDate { get; set; } = DateTime.Today.AddDays(30);
        public string Remarks { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public bool IsDeleted { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime? LastSyncedDate { get; set; }
        public DateTime? DeletedDate { get; set; }
    }
}
