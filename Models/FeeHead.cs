using System;
using LiteDB;

namespace RajasvSchoolManagement.Models
{
    public class FeeHead
    {
        [BsonId]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public decimal DefaultAmount { get; set; }
        public string Frequency { get; set; } = "Monthly";
        public bool IsActive { get; set; } = true;
        public string Description { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.Now;
    }
}
