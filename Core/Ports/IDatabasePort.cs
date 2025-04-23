using System;
using System.Collections.Generic;
using odysseyAnalytics.Core.Application.Events;

namespace odysseyAnalytics.Core.Ports
{
    public interface IDatabasePort
    {
        IEnumerable<T> ReadAll<T>() where T : AnalyticsEvent;
        T Read<T>(string key) where T : AnalyticsEvent;
        void Write<T>(string key, T value) where T : AnalyticsEvent;
        void Delete<T>(string key) where T : AnalyticsEvent;
        void Update<T>(string key, T value) where T : AnalyticsEvent;

        IEnumerable<T> ReadWhere<T>(Func<T, bool> predicate) where T : AnalyticsEvent;
    }
}