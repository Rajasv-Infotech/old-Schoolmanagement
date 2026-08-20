using LiteDB;

namespace RajasvSchoolManagement.Models
{
    public class SchoolSettings
    {
        [BsonId]
        public string Id { get; set; } = "default";
        public string SchoolName { get; set; } = "THE BLUEBELLS INTERNATIONAL SCHOOL";
        public string Address { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string ReceiptPrefix { get; set; } = "RCPT";
        public string ReceivedBy { get; set; } = "Accounts";
    }
}
