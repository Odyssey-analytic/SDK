using System;
using System.Collections.Generic;
using Newtonsoft.Json;

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
            int priority, int id, SeverityLevel severity, string message) : base(queueName,
            eventTime, sessionId, clientId, priority, id)
        {
            this.severity = severity;
            this.message = message;
            _data.Add("message", message);
            _data.Add("severity", severity.ToString());
            _dataJson = JsonConvert.SerializeObject(_data);

        }
    }
}