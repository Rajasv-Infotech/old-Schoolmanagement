using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using RajasvSchoolManagement.Data;
using RajasvSchoolManagement.Models;

namespace RajasvSchoolManagement.Services
{
    public class MigrationResult
    {
        public int Sessions { get; set; }
        public int ClassSections { get; set; }
        public int Students { get; set; }
        public int Teachers { get; set; }
        public int FeeHeads { get; set; }
        public int CommonFeeRules { get; set; }
        public int FeeRecords { get; set; }
        public int Receipts { get; set; }
        public int Attendance { get; set; }
        public int FeeStructures { get; set; }
        public List<string> Warnings { get; private set; } = new List<string>();

        public int Total
        {
            get
            {
                return Sessions + ClassSections + Students + Teachers + FeeHeads + CommonFeeRules +
                       FeeRecords + Receipts + Attendance + FeeStructures;
            }
        }
    }

    public class JsonMigrationService
    {
        private readonly DatabaseService _database;

        public JsonMigrationService(DatabaseService database)
        {
            _database = database;
        }

        public MigrationResult ImportFolder(string folder)
        {
            if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
                throw new DirectoryNotFoundException("Selected SchoolData folder does not exist.");

            MigrationResult result = new MigrationResult();
            _database.RunInTransaction(delegate(LiteDB.LiteDatabase db)
            {
                result.Sessions = Import<AcademicSession>(folder, "sessions.json", db.GetCollection<AcademicSession>("sessions"), result);
                result.ClassSections = Import<ClassSection>(folder, "classsections.json", db.GetCollection<ClassSection>("classsections"), result);
                result.Students = Import<Student>(folder, "students.json", db.GetCollection<Student>("students"), result);
                result.Teachers = Import<Teacher>(folder, "teachers.json", db.GetCollection<Teacher>("teachers"), result);
                result.FeeHeads = Import<FeeHead>(folder, "feeheads.json", db.GetCollection<FeeHead>("feeheads"), result);
                result.CommonFeeRules = Import<CommonFeeRule>(folder, "commonfeerules.json", db.GetCollection<CommonFeeRule>("commonfeerules"), result);
                result.FeeRecords = Import<FeeRecord>(folder, "fees.json", db.GetCollection<FeeRecord>("feerecords"), result);
                result.Receipts = Import<FeePaymentReceipt>(folder, "feepaymentreceipts.json", db.GetCollection<FeePaymentReceipt>("receipts"), result);
                result.Attendance = Import<Attendance>(folder, "attendance.json", db.GetCollection<Attendance>("attendance"), result);
                result.FeeStructures = Import<FeeStructure>(folder, "feestructure.json", db.GetCollection<FeeStructure>("feestructures"), result);
            });

            SeedService.EnsureDefaults(_database);
            return result;
        }

        private static int Import<T>(string folder, string fileName, LiteDB.ILiteCollection<T> collection, MigrationResult result)
            where T : class
        {
            string path = Path.Combine(folder, fileName);
            if (!File.Exists(path)) return 0;

            try
            {
                string json = File.ReadAllText(path);
                List<T> items = JsonConvert.DeserializeObject<List<T>>(json) ?? new List<T>();
                if (items.Count > 0)
                {
                    collection.Upsert(items);
                }
                return items.Count;
            }
            catch (Exception ex)
            {
                result.Warnings.Add(fileName + ": " + ex.Message);
                return 0;
            }
        }
    }
}
