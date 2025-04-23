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

        public IEnumerable<T> ReadAll<T>() where T : AnalyticsEvent
        {
                var dtos = db.Table<SQLiteDTO>().ToList();
                return dtos.Select(dto => (T)(object)dto.ToDomain());
        }

        public T Read<T>(string key) where T : AnalyticsEvent
        {
                var dto = db.Find<SQLiteDTO>(int.Parse(key));
                return (T)(object)dto?.ToDomain();
        }

        public void Write<T>(string key, T value) where T : AnalyticsEvent
        {
                var dto = SQLiteDTO.FromDomain(value);
                db.Insert(dto);
        }

        public void Delete<T>(string key) where T : AnalyticsEvent
        {
                db.Delete<SQLiteDTO>(int.Parse(key));
        }

        public void Update<T>(string key, T value) where T : AnalyticsEvent
        {
            
                var dto = SQLiteDTO.FromDomain(value);
                dto.Id = int.Parse(key);
                db.Update(dto);
        }

        public IEnumerable<T> ReadWhere<T>(Func<T, bool> predicate) where T : AnalyticsEvent
        {
            var all = ReadAll<T>();
            return all.Where(predicate);
        }

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