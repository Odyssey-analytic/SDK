using System;
using System.Collections.Generic;

namespace odysseyAnalytics.Core.Application.Events
{
    public enum SeverityLevel
    {
        Info,
        Debug,
        Warning,
        Error,
        Critical
    }

    public class ErrorEvent : AnalyticsEvent
    {
        private SeverityLevel severity;
        private string message;

        public ErrorEvent(string queueName, DateTime eventTime, string sessionId, string clientId,
            int priority,
            Dictionary<string, string> data, int id, SeverityLevel severity, string message) : base(queueName,
            eventTime, sessionId, clientId, priority, data, id)
        {
            EventType = ERROR_EVENT_TYPE;
            this.severity = severity;
            this.message = message;
        }
    }
}