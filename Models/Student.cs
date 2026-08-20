using System;
using LiteDB;

namespace RajasvSchoolManagement.Models
{
    public class Student
    {
        [BsonId]
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string FullName { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; } = DateTime.Now.AddYears(-10);
        public string SessionId { get; set; } = string.Empty;
        public string SessionName { get; set; } = string.Empty;
        public string ClassSectionId { get; set; } = string.Empty;
        public string Class { get; set; } = string.Empty;
        public string Section { get; set; } = string.Empty;
        public string RollNumber { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string FatherName { get; set; } = string.Empty;
        public string MotherName { get; set; } = string.Empty;
        public string FatherContact { get; set; } = string.Empty;
        public string MotherContact { get; set; } = string.Empty;
        public string GuardianName { get; set; } = string.Empty;
        public string GuardianContact { get; set; } = string.Empty;
        public string ParentName { get; set; } = string.Empty;
        public string ParentContact { get; set; } = string.Empty;
        public DateTime EnrollmentDate { get; set; } = DateTime.Now;
        public bool IsActive { get; set; } = true;
    }
}
