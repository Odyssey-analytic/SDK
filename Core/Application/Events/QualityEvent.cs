using System;
using System.Collections.Generic;

namespace odysseyAnalytics.Core.Application.Events
{
    public class QualityEvent : AnalyticsEvent
    {
        private float FPS;
        private float memoryUsage;

        public QualityEvent(float fps, float memoryUsage, string queueName, DateTime eventTime,
            string sessionId, string clientId, int priority, Dictionary<string, string> data, int id = -1) : base(
            queueName, eventTime, sessionId, clientId, priority, data, id)
        {
            EventType = QUALITY_EVENT_TYPE;
            FPS = fps;
            this.memoryUsage = memoryUsage;
        }
    }
}