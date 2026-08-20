using System;
using LiteDB;

namespace RajasvSchoolManagement.Models
{
    public class Attendance
    {
        [BsonId]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string StudentId { get; set; } = string.Empty;
        public string StudentName { get; set; } = string.Empty;
        public DateTime Date { get; set; } = DateTime.Today;
        public string Status { get; set; } = "Present";
        public string Remarks { get; set; } = string.Empty;
    }
}
