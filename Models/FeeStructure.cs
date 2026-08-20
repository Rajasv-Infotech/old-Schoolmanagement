using System;
using LiteDB;

namespace RajasvSchoolManagement.Models
{
    public class FeeStructure
    {
        [BsonId]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Class { get; set; } = string.Empty;
        public string FeeType { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Frequency { get; set; } = "Monthly";
        public bool IsActive { get; set; } = true;
        public DateTime EffectiveFrom { get; set; } = DateTime.Now;
        public string Description { get; set; } = string.Empty;
    }
}
