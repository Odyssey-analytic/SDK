using System;
using System.Collections.Generic;

namespace odysseyAnalytics.Core.Application.Events
{
    public class SessionEndEvent : AnalyticsEvent
    {
        private string platform;

        public SessionEndEvent(string platform, string eventName, string queueName, DateTime eventTime,
            string sessionId, string clientId, int priority, Dictionary<string, string> data, int id = -1) : base(
            eventName, queueName, eventTime, sessionId, clientId, priority, data, id)
        {
            this.platform = platform;
        }
    }
}