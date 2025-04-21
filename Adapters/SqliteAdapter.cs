using System;
using System.IO;
using odysseyAnalytics.Core.Ports;
using SQLite;

namespace odysseyAnalytics.Connections
{
    public class SqliteAdapter : IDatabasePort, IDisposable
    {
        private readonly SQLiteConnection _db;
        public SqliteAdapter()
        {
            var databasePath = Path.Combine(AppContext.BaseDirectory, "analytics.db");
            _db = new SQLiteConnection(databasePath);
        }
        

        public void Dispose()
        {
            _db.Dispose();
        }

        public T ReadAll<T>()
        {
            return _db.Table<T>().ToList();
        }

        public T Read<T>(string key)
        {
            return _db.Get<T>(key);
        }

        void IDatabasePort.Write<T>(string key, T value)
        {
            Write(key, value);
        }

        public void Write<T>(string key, T value)
        {
            _db.Insert(value);
        }

        public void Delete<T>(string key)
        {
            _db.Delete<T>(key);
        }

        public void Update<T>(string key, T value)
        {
            _db.Update(value, typeof(T));
        }
    }
}