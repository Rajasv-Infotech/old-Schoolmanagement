using RajasvSchoolManagement.Data;
using RajasvSchoolManagement.Models;

namespace RajasvSchoolManagement.Services
{
    public static class SeedService
    {
        public static void EnsureDefaults(DatabaseService database)
        {
            Repository<SchoolSettings> settings = new Repository<SchoolSettings>(database, "settings");
            if (settings.FindById("default") == null)
            {
                settings.Upsert(new SchoolSettings());
            }

            database.GetCollection<Student>("students").EnsureIndex(x => x.RollNumber);
            database.GetCollection<Student>("students").EnsureIndex(x => x.SessionId);
            database.GetCollection<Student>("students").EnsureIndex(x => x.ClassSectionId);
            database.GetCollection<ClassSection>("classsections").EnsureIndex(x => x.SessionId);
            database.GetCollection<FeeRecord>("feerecords").EnsureIndex(x => x.StudentId);
            database.GetCollection<FeeRecord>("feerecords").EnsureIndex(x => x.Status);
            database.GetCollection<FeePaymentReceipt>("receipts").EnsureIndex(x => x.ReceiptNumber);
            database.GetCollection<Attendance>("attendance").EnsureIndex(x => x.StudentId);
            database.GetCollection<Attendance>("attendance").EnsureIndex(x => x.Date);
        }
    }
}
