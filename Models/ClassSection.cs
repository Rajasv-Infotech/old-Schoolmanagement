using System;
using LiteDB;

namespace RajasvSchoolManagement.Models
{
    public class ClassSection
    {
        [BsonId]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string SessionId { get; set; } = string.Empty;
        public string SessionName { get; set; } = string.Empty;
        public string ClassName { get; set; } = string.Empty;
        public string SectionName { get; set; } = string.Empty;
        public int Capacity { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [BsonIgnore]
        public string DisplayName
        {
            get { return string.IsNullOrWhiteSpace(SectionName) ? ClassName : ClassName + " - " + SectionName; }
        }
    }
}
