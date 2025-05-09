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

        public void SaveEvent(AnalyticsEvent analyticsEvent)
        {
            _databasePort.Write(analyticsEvent.Id.ToString(), analyticsEvent);
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

        public void Close()
        {
            _databasePort.Close();
        }
    }
}