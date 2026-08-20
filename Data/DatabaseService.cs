using System;
using LiteDB;

namespace RajasvSchoolManagement.Data
{
    public sealed class DatabaseService : IDisposable
    {
        private readonly object _sync = new object();
        private LiteDatabase _database;

        public LiteDatabase Database
        {
            get
            {
                lock (_sync)
                {
                    if (_database == null)
                    {
                        OpenInternal();
                    }
                    return _database;
                }
            }
        }

        private void OpenInternal()
        {
            DataPaths.EnsureDirectories();
            ConnectionString connection = new ConnectionString
            {
                Filename = DataPaths.DatabaseFile,
                Connection = ConnectionType.Direct,
                Upgrade = true
            };
            _database = new LiteDatabase(connection);
        }

        public ILiteCollection<T> GetCollection<T>(string collectionName)
        {
            return Database.GetCollection<T>(collectionName);
        }

        public void RunInTransaction(Action<LiteDatabase> work)
        {
            if (work == null) throw new ArgumentNullException("work");
            lock (_sync)
            {
                LiteDatabase db = Database;
                db.BeginTrans();
                try
                {
                    work(db);
                    db.Commit();
                }
                catch
                {
                    db.Rollback();
                    throw;
                }
            }
        }

        public void Checkpoint()
        {
            lock (_sync)
            {
                if (_database != null)
                {
                    _database.Checkpoint();
                }
            }
        }

        public void Close()
        {
            lock (_sync)
            {
                if (_database != null)
                {
                    try { _database.Checkpoint(); } catch { }
                    _database.Dispose();
                    _database = null;
                }
            }
        }

        public void Reopen()
        {
            lock (_sync)
            {
                Close();
                OpenInternal();
            }
        }

        public void Dispose()
        {
            Close();
        }
    }
}
