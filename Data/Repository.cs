using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using LiteDB;

namespace RajasvSchoolManagement.Data
{
    public class Repository<T> where T : class
    {
        private readonly DatabaseService _database;
        private readonly string _collectionName;

        public Repository(DatabaseService database, string collectionName)
        {
            _database = database;
            _collectionName = collectionName;
        }

        private ILiteCollection<T> Collection
        {
            get { return _database.GetCollection<T>(_collectionName); }
        }

        public List<T> GetAll()
        {
            return Collection.FindAll().ToList();
        }

        public List<T> Find(Expression<Func<T, bool>> predicate)
        {
            return Collection.Find(predicate).ToList();
        }

        public T FindById(string id)
        {
            return Collection.FindById(new BsonValue(id));
        }

        public bool Upsert(T entity)
        {
            return Collection.Upsert(entity);
        }

        public int UpsertMany(IEnumerable<T> entities)
        {
            return Collection.Upsert(entities);
        }

        public bool Delete(string id)
        {
            return Collection.Delete(new BsonValue(id));
        }

        public int DeleteAll()
        {
            return Collection.DeleteAll();
        }

        public int Count()
        {
            return Collection.Count();
        }
    }
}
