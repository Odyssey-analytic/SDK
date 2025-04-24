using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
            // Ensure directory exists
            var directory = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            db = new SQLiteConnection(dbPath);
            db.CreateTable<SQLiteDTO>();
        }

        public void Close()
        {
            db.Dispose();
        }

        public IEnumerable<T> ReadAll<T>() where T : AnalyticsEvent
        {
            var dtos = db.Table<SQLiteDTO>().ToList().OrderBy(o => o.Priority);
            return dtos.Select(dto => (T)(object)dto.ToDomain());
        }

        public T Read<T>(string key) where T : AnalyticsEvent
        {
            // Try to parse key as int for ID-based lookup
            if (int.TryParse(key, out int id))
            {
                var dto = db.Find<SQLiteDTO>(id);
                return dto != null ? (T)(object)dto.ToDomain() : null;
            }
            
            // If not an ID, look up by event name
            var result = db.Table<SQLiteDTO>()
                .Where(e => e.EventName == key)
                .FirstOrDefault();
                
            return result != null ? (T)(object)result.ToDomain() : null;
        }

        public void Write<T>(string key, T value) where T : AnalyticsEvent
        {
            var dto = SQLiteDTO.FromDomain(value);
            
            // If key is numeric and value.Id is 0, use key as ID
            if (int.TryParse(key, out int id) && value.Id == 0)
            {
                dto.Id = id;
            }
            
            db.Insert(dto);
        }

        public void Delete<T>(string key) where T : AnalyticsEvent
        {
            // Try to parse key as int for ID-based deletion
            if (int.TryParse(key, out int id))
            {
                Console.WriteLine($"id :{id}");
                db.Delete<SQLiteDTO>(id);
                return;
            }
            
            // If key is not numeric, try by event name
            var matchingEvents = db.Table<SQLiteDTO>()
                .Where(e => e.EventName == key || e.SessionId == key)
                .ToList();
                
            foreach (var evt in matchingEvents)
            {
                db.Delete<SQLiteDTO>(evt.Id);
            }
        }

        public void Update<T>(string key, T value) where T : AnalyticsEvent
        {
            var dto = SQLiteDTO.FromDomain(value);
            
            // Use key as ID if possible, otherwise use value's ID
            if (int.TryParse(key, out int id))
            {
                dto.Id = id;
            }
            
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
