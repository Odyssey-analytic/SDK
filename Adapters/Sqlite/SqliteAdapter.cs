using System;
using System.Collections.Generic;
using System.Linq;
using odysseyAnalytics.Core.Application.Events;
using odysseyAnalytics.Core.Ports;
using System.Data.SQLite;
using System.IO;
using Microsoft.Data.Sqlite;

namespace odysseyAnalytics.Adapters.Sqlite
{
    public class SqliteAdapter : IDatabasePort
    {
        private readonly string _connectionString;

        public SqliteAdapter(string dbPath)
        {
            // Create directory if it doesn't exist
            string directory = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            _connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = dbPath,
                Mode = SqliteOpenMode.ReadWriteCreate
            }.ConnectionString;

            // Initialize database
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = @"
                        CREATE TABLE IF NOT EXISTS AnalyticsEvents (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            EventKey TEXT NOT NULL,
                            EventType TEXT NOT NULL,
                            EventTime TEXT NOT NULL,
                            SessionId TEXT NOT NULL,
                            ClientId TEXT NOT NULL,
                            Priority INTEGER NOT NULL,
                            DataJson TEXT NOT NULL
                        )";

                    command.ExecuteNonQuery();
                }
            }
        }

        public IEnumerable<T> ReadAll<T>() where T : AnalyticsEvent
        {
            var events = new List<T>();

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM AnalyticsEvents ORDER BY EventTime";

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                var dto = new SqliteDTO
                                {
                                    Id = reader.GetInt32(0),
                                    EventKey = reader.GetString(1),
                                    EventType = reader.GetString(2),
                                    EventTime = DateTime.Parse(reader.GetString(3)),
                                    SessionId = reader.GetString(4),
                                    ClientId = reader.GetString(5),
                                    Priority = reader.GetInt32(6),
                                    DataJson = reader.GetString(7)
                                };

                                var analyticsEvent = dto.ToAnalyticsEvent();
                                if (analyticsEvent is T typedEvent)
                                {
                                    events.Add(typedEvent);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error converting event: {ex.Message}");
                            }
                        }
                    }
                }
            }

            return events;
        }

        public T Read<T>(string key) where T : AnalyticsEvent
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM AnalyticsEvents WHERE EventKey = @EventKey";
                    command.Parameters.AddWithValue("@EventKey", key);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            try
                            {
                                var dto = new SqliteDTO
                                {
                                    Id = reader.GetInt32(0),
                                    EventKey = reader.GetString(1),
                                    EventType = reader.GetString(2),
                                    EventTime = DateTime.Parse(reader.GetString(3)),
                                    SessionId = reader.GetString(4),
                                    ClientId = reader.GetString(5),
                                    Priority = reader.GetInt32(6),
                                    DataJson = reader.GetString(7)
                                };

                                var analyticsEvent = dto.ToAnalyticsEvent();
                                if (analyticsEvent is T typedEvent)
                                {
                                    return typedEvent;
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error converting event: {ex.Message}");
                            }
                        }
                    }
                }
            }

            return null;
        }

        public void Write<T>(string key, T value) where T : AnalyticsEvent
        {
            var dto = new SqliteDTO(value);
            dto.EventKey = key; // Use the provided key

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    if (dto.Id <= 0)
                    {
                        // Insert new record
                        command.CommandText = @"
                            INSERT INTO AnalyticsEvents 
                            (EventKey, EventType, EventTime, SessionId, ClientId, Priority, DataJson)
                            VALUES 
                            (@EventKey, @EventType, @EventTime, @SessionId, @ClientId, @Priority, @DataJson);
                            SELECT last_insert_rowid();";

                        command.Parameters.AddWithValue("@EventKey", dto.EventKey);
                        command.Parameters.AddWithValue("@EventType", dto.EventType);
                        command.Parameters.AddWithValue("@EventTime", dto.EventTime.ToString("yyyy-MM-dd HH:mm:ss"));
                        command.Parameters.AddWithValue("@SessionId", dto.SessionId);
                        command.Parameters.AddWithValue("@ClientId", dto.ClientId);
                        command.Parameters.AddWithValue("@Priority", dto.Priority);
                        command.Parameters.AddWithValue("@DataJson", dto.DataJson);

                        // Get the auto-generated ID
                        long newId = Convert.ToInt64(command.ExecuteScalar());
                        value.Id = (int)newId;
                    }
                    else
                    {
                        // Update existing record
                        command.CommandText = @"
                            UPDATE AnalyticsEvents 
                            SET EventKey = @EventKey,
                                EventType = @EventType,
                                EventTime = @EventTime, 
                                SessionId = @SessionId, 
                                ClientId = @ClientId, 
                                Priority = @Priority, 
                                DataJson = @DataJson
                            WHERE Id = @Id";

                        command.Parameters.AddWithValue("@Id", dto.Id);
                        command.Parameters.AddWithValue("@EventKey", dto.EventKey);
                        command.Parameters.AddWithValue("@EventType", dto.EventType);
                        command.Parameters.AddWithValue("@EventTime", dto.EventTime.ToString("yyyy-MM-dd HH:mm:ss"));
                        command.Parameters.AddWithValue("@SessionId", dto.SessionId);
                        command.Parameters.AddWithValue("@ClientId", dto.ClientId);
                        command.Parameters.AddWithValue("@Priority", dto.Priority);
                        command.Parameters.AddWithValue("@DataJson", dto.DataJson);

                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        public void Delete<T>(string key) where T : AnalyticsEvent
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "DELETE FROM AnalyticsEvents WHERE EventKey = @EventKey";
                    command.Parameters.AddWithValue("@EventKey", key);
                    command.ExecuteNonQuery();
                }
            }
        }

        public void Update<T>(string key, T value) where T : AnalyticsEvent
        {
            Write(key, value);
        }

        public IEnumerable<T> ReadWhere<T>(Func<T, bool> predicate) where T : AnalyticsEvent
        {
            return ReadAll<T>().Where(predicate);
        }

        public void Close()
        {
            // No specific cleanup needed for SQLite connections in this implementation
        }
    }
}