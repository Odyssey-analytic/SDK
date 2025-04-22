using System;
using System.Collections.Generic;
using System.Linq;
using odysseyAnalytics.Core.Application.Events;
using odysseyAnalytics.Core.Ports;
using SQLite;
namespace odysseyAnalytics.Adapters.Sqlite
{
    public class SqliteAdapter : IDatabasePort, IDisposable
    {
        private readonly SQLiteConnection db;
        private bool disposed = false;

        public SqliteAdapter(string dbPath)
        {
            db = new SQLiteConnection(dbPath);
            db.CreateTable<SQLiteDTO>();
        }

        public IEnumerable<T> ReadAll<T>()
        {
            if (typeof(T) == typeof(AnalyticsEvent))
            {
                var dtos = db.Table<SQLiteDTO>().ToList();
                return dtos.Select(dto => (T)(object)dto.ToDomain());
            }
            throw new NotSupportedException($"Unsupported type {typeof(T).Name}");
        }

        public T Read<T>(string key)
        {
            if (typeof(T) == typeof(AnalyticsEvent) && int.TryParse(key, out int id))
            {
                var dto = db.Find<SQLiteDTO>(id);
                return (T)(object)dto?.ToDomain();
            }
            throw new NotSupportedException($"Unsupported type or key: {typeof(T).Name}");
        }

        public void Write<T>(string key, T value) where T : new()
        {
            if (value is AnalyticsEvent evt)
            {
                var dto = SQLiteDTO.FromDomain(evt);
                db.Insert(dto);
            }
            else
            {
                throw new NotSupportedException($"Unsupported type {typeof(T).Name}");
            }
        }

        public void Delete<T>(string key)
        {
            if (typeof(T) == typeof(AnalyticsEvent) && int.TryParse(key, out int id))
            {
                db.Delete<SQLiteDTO>(id);
            }
            else
            {
                throw new NotSupportedException($"Unsupported type or key: {typeof(T).Name}");
            }
        }

        public void Update<T>(string key, T value)
        {
            if (value is AnalyticsEvent evt && int.TryParse(key, out int id))
            {
                var dto = SQLiteDTO.FromDomain(evt);
                dto.Id = id;
                db.Update(dto);
            }
            else
            {
                throw new NotSupportedException($"Unsupported type or key: {typeof(T).Name}");
            }
        }

        public IEnumerable<T> ReadWhere<T>(Func<T, bool> predicate)
        {
            var all = ReadAll<T>();
            return all.Where(predicate);
        }

        // IDisposable implementation
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    db?.Close();
                    db?.Dispose();
                }
                disposed = true;
            }
        }

        ~SqliteAdapter()
        {
            Dispose(false);
        }
    }
}