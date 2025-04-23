using System;
using System.Collections.Generic;
using odysseyAnalytics.Core.Application.Events;
using SQLite;

namespace odysseyAnalytics.Adapters.Sqlite
{
    public class SQLiteDTO
    {
        [PrimaryKey, AutoIncrement] public int Id { get; set; }
        public string EventName { get; set; }
        public string QueueName { get; set; }
        public DateTime EventTime { get; set; }
        public string SessionId { get; set; }
        public string ClientId { get; set; }
        public string DataJson { get; set; }
        public int Priority { get; set; }

        public AnalyticsEvent ToDomain()
        {
            var evt = new AnalyticsEvent
                (EventName, QueueName, EventTime, SessionId, ClientId, Priority, new Dictionary<string, string>());
            evt.SetRawDataJson(DataJson);
            return evt;
        }

        public static SQLiteDTO FromDomain(AnalyticsEvent evt)
        {
            return new SQLiteDTO
            {
                Id = evt.Id,
                EventName = evt.EventName,
                QueueName = evt.QueueName,
                EventTime = evt.EventTime,
                SessionId = evt.SessionId,
                ClientId = evt.ClientId,
                Priority = evt.Priority,
                DataJson = evt.GetRawDataJson()
            };
        }
    }
}