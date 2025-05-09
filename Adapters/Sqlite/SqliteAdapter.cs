using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Data.Sqlite;
using odysseyAnalytics.Core.Application.Events;

namespace odysseyAnalytics.Infrastructure.Persistence
{
    public class SqliteAnalyticsRepository
    {
        private readonly string _connectionString;

        public SqliteAnalyticsRepository(string dbPath)
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
                            EventType TEXT NOT NULL,
                            QueueName TEXT NOT NULL,
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

        public void SaveEvent(AnalyticsEvent analyticsEvent)
        {
            var dto = new SqliteDTO(analyticsEvent);

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
                            (EventType, QueueName, EventTime, SessionId, ClientId, Priority, DataJson)
                            VALUES 
                            (@EventType, @QueueName, @EventTime, @SessionId, @ClientId, @Priority, @DataJson);
                            SELECT last_insert_rowid();";

                        command.Parameters.AddWithValue("@EventType", dto.EventType);
                        command.Parameters.AddWithValue("@QueueName", dto.QueueName);
                        command.Parameters.AddWithValue("@EventTime", dto.EventTime.ToString("yyyy-MM-dd HH:mm:ss"));
                        command.Parameters.AddWithValue("@SessionId", dto.SessionId);
                        command.Parameters.AddWithValue("@ClientId", dto.ClientId);
                        command.Parameters.AddWithValue("@Priority", dto.Priority);
                        command.Parameters.AddWithValue("@DataJson", dto.DataJson);

                        // Get the auto-generated ID
                        long newId = Convert.ToInt64(command.ExecuteScalar());
                        analyticsEvent.Id = (int)newId;
                    }
                    else
                    {
                        // Update existing record
                        command.CommandText = @"
                            UPDATE AnalyticsEvents 
                            SET EventType = @EventType, 
                                QueueName = @QueueName, 
                                EventTime = @EventTime, 
                                SessionId = @SessionId, 
                                ClientId = @ClientId, 
                                Priority = @Priority, 
                                DataJson = @DataJson
                            WHERE Id = @Id";

                        command.Parameters.AddWithValue("@Id", dto.Id);
                        command.Parameters.AddWithValue("@EventType", dto.EventType);
                        command.Parameters.AddWithValue("@QueueName", dto.QueueName);
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

        public void SaveEvents(IEnumerable<AnalyticsEvent> events)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        foreach (var analyticsEvent in events)
                        {
                            var dto = new SqliteDTO(analyticsEvent);

                            using (var command = connection.CreateCommand())
                            {
                                command.Transaction = transaction;

                                if (dto.Id <= 0)
                                {
                                    // Insert new record
                                    command.CommandText = @"
                                        INSERT INTO AnalyticsEvents 
                                        (EventType, QueueName, EventTime, SessionId, ClientId, Priority, DataJson)
                                        VALUES 
                                        (@EventType, @QueueName, @EventTime, @SessionId, @ClientId, @Priority, @DataJson);
                                        SELECT last_insert_rowid();";

                                    command.Parameters.AddWithValue("@EventType", dto.EventType);
                                    command.Parameters.AddWithValue("@QueueName", dto.QueueName);
                                    command.Parameters.AddWithValue("@EventTime",
                                        dto.EventTime.ToString("yyyy-MM-dd HH:mm:ss"));
                                    command.Parameters.AddWithValue("@SessionId", dto.SessionId);
                                    command.Parameters.AddWithValue("@ClientId", dto.ClientId);
                                    command.Parameters.AddWithValue("@Priority", dto.Priority);
                                    command.Parameters.AddWithValue("@DataJson", dto.DataJson);

                                    // Get the auto-generated ID
                                    long newId = Convert.ToInt64(command.ExecuteScalar());
                                    analyticsEvent.Id = (int)newId;
                                }
                                else
                                {
                                    // Update existing record
                                    command.CommandText = @"
                                        UPDATE AnalyticsEvents 
                                        SET EventType = @EventType, 
                                            QueueName = @QueueName, 
                                            EventTime = @EventTime, 
                                            SessionId = @SessionId, 
                                            ClientId = @ClientId, 
                                            Priority = @Priority, 
                                            DataJson = @DataJson
                                        WHERE Id = @Id";

                                    command.Parameters.AddWithValue("@Id", dto.Id);
                                    command.Parameters.AddWithValue("@EventType", dto.EventType);
                                    command.Parameters.AddWithValue("@QueueName", dto.QueueName);
                                    command.Parameters.AddWithValue("@EventTime",
                                        dto.EventTime.ToString("yyyy-MM-dd HH:mm:ss"));
                                    command.Parameters.AddWithValue("@SessionId", dto.SessionId);
                                    command.Parameters.AddWithValue("@ClientId", dto.ClientId);
                                    command.Parameters.AddWithValue("@Priority", dto.Priority);
                                    command.Parameters.AddWithValue("@DataJson", dto.DataJson);

                                    command.ExecuteNonQuery();
                                }
                            }
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        public AnalyticsEvent GetEventById(int id)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT * FROM AnalyticsEvents WHERE Id = @Id";
                    command.Parameters.AddWithValue("@Id", id);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var dto = new SqliteDTO
                            {
                                Id = reader.GetInt32(0),
                                EventType = reader.GetString(1),
                                QueueName = reader.GetString(2),
                                EventTime = DateTime.Parse(reader.GetString(3)),
                                SessionId = reader.GetString(4),
                                ClientId = reader.GetString(5),
                                Priority = reader.GetInt32(6),
                                DataJson = reader.GetString(7)
                            };

                            return dto.ToAnalyticsEvent();
                        }
                    }
                }
            }

            return null;
        }

        public List<AnalyticsEvent> GetAllEvents()
        {
            var events = new List<AnalyticsEvent>();

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
                                    EventType = reader.GetString(1),
                                    QueueName = reader.GetString(2),
                                    EventTime = DateTime.Parse(reader.GetString(3)),
                                    SessionId = reader.GetString(4),
                                    ClientId = reader.GetString(5),
                                    Priority = reader.GetInt32(6),
                                    DataJson = reader.GetString(7)
                                };

                                events.Add(dto.ToAnalyticsEvent());
                            }
                            catch (Exception ex)
                            {
                                // Log error and continue
                                Console.WriteLine($"Error converting event: {ex.Message}");
                            }
                        }
                    }
                }
            }

            return events;
        }

        public List<AnalyticsEvent> GetEventsBySession(string sessionId)
        {
            var events = new List<AnalyticsEvent>();

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "SELECT * FROM AnalyticsEvents WHERE SessionId = @SessionId ORDER BY EventTime";
                    command.Parameters.AddWithValue("@SessionId", sessionId);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                var dto = new SqliteDTO
                                {
                                    Id = reader.GetInt32(0),
                                    EventType = reader.GetString(1),
                                    QueueName = reader.GetString(2),
                                    EventTime = DateTime.Parse(reader.GetString(3)),
                                    SessionId = reader.GetString(4),
                                    ClientId = reader.GetString(5),
                                    Priority = reader.GetInt32(6),
                                    DataJson = reader.GetString(7)
                                };

                                events.Add(dto.ToAnalyticsEvent());
                            }
                            catch (Exception ex)
                            {
                                // Log error and continue
                                Console.WriteLine($"Error converting event: {ex.Message}");
                            }
                        }
                    }
                }
            }

            return events;
        }

        public List<AnalyticsEvent> GetEventsByType(string eventType)
        {
            var events = new List<AnalyticsEvent>();

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();

                using (var command = connection.CreateCommand())
                {
                    command.CommandText =
                        "SELECT * FROM AnalyticsEvents WHERE EventType = @EventType ORDER BY EventTime";
                    command.Parameters.AddWithValue("@EventType", eventType);

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                var dto = new SqliteDTO
                                {
                                    Id = reader.GetInt32(0),
                                    EventType = reader.GetString(1),
                                    QueueName = reader.GetString(2),
                                    EventTime = DateTime.Parse(reader.GetString(3)),
                                    SessionId = reader.GetString(4),
                                    ClientId = reader.GetString(5),
                                    Priority = reader.GetInt32(6),
                                    DataJson = reader.GetString(7)
                                };

                                events.Add(dto.ToAnalyticsEvent());
                            }
                            catch (Exception ex)
                            {
                                // Log error and continue
                                Console.WriteLine($"Error converting event: {ex.Message}");
                            }
                        }
                    }
                }
            }

            return events;
        }
    }
}