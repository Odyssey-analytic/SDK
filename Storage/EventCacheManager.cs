﻿using System;
using System.Collections.Generic;
using System.IO;
using SQLite;
using odysseyAnalytics.Events;

#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE
using UnityEngine;
#endif

namespace odysseyAnalytics.Storage
{
    public class EventCacheManager
    {
        private readonly SQLiteConnection _db;

        public EventCacheManager()
        {
            var path = GetDatabasePath();
            _db = new SQLiteConnection(path);
            _db.CreateTable<AnalyticsEvent>();
        }

        private string GetDatabasePath()
        {
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE
            return Path.Combine(Application.persistentDataPath, "analytics.db");
#else
            return Path.Combine(AppContext.BaseDirectory, "analytics.db");
#endif
        }

        public void SaveEvent(string name, string queueName, string clientId, string sessionId,
            Dictionary<string, object> eventDatas)
        {
            int priority = name == "start_session" ? 0 : 5; // Example: start_session is critical

            var evt = new AnalyticsEvent
            {
                EventName = name,
                SessionId = sessionId,
                QueueName = queueName,
                EventTime = DateTime.UtcNow,
                Data = eventDatas,
                ClientId = clientId,
                Priority = priority
            };

            _db.Insert(evt);
        }

        public List<AnalyticsEvent> LoadAllEvents()
        {
            return _db.Table<AnalyticsEvent>().ToList();
        }

        public void DeleteEvent(int id)
        {
            _db.Delete<AnalyticsEvent>(id);
        }

        public void Clear()
        {
            _db.DeleteAll<AnalyticsEvent>();
        }
    }
}