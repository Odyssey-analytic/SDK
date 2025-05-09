using System;
using System.Collections.Generic;

namespace odysseyAnalytics.Core.Application.Events
{
    public class QualityEvent : AnalyticsEvent
    {
        private float FPS;
        private float memoryUsage;

        public QualityEvent(float fps, float memoryUsage, string eventName, string queueName, DateTime eventTime,
            string sessionId, string clientId, int priority, Dictionary<string, string> data, int id = -1) : base(
            eventName, queueName, eventTime, sessionId, clientId, priority, data, id)
        {
            FPS = fps;
            this.memoryUsage = memoryUsage;
        }
    }
}