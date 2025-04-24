using System;
using System.Collections.Generic;
using odysseyAnalytics.Core.Application.Events;
using odysseyAnalytics.Core.Ports;

namespace odysseyAnalytics.Core.Application.CacheManager
{
    public class CacheHandler
    {
        private readonly IDatabasePort _databasePort;

        public CacheHandler(IDatabasePort databasePort)
        {
            _databasePort = databasePort;
        }

        public void SaveEvent(string name, string queueName, string clientId, string sessionId,
            Dictionary<string, string> eventDatas)
        {
            int priority = name == "start_session" ? 0 : 5;

            var evt = new AnalyticsEvent(name, queueName, DateTime.UtcNow, sessionId, clientId, priority, eventDatas);
            
            _databasePort.Write<AnalyticsEvent>(evt.Id.ToString(), evt);
        }

        public List<AnalyticsEvent> LoadAllEvents()
        {
            return new List<AnalyticsEvent>(_databasePort.ReadAll<AnalyticsEvent>());
        }

        public void DeleteEvent(int id)
        {
            _databasePort.Delete<AnalyticsEvent>(id.ToString());
        }

        public void Clear()
        {
            var allEvents = _databasePort.ReadAll<AnalyticsEvent>();
            foreach (var evt in allEvents)
            {
                _databasePort.Delete<AnalyticsEvent>(evt.Id.ToString());
            }
        }
    }
}