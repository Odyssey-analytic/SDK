using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace odysseyAnalytics.Core.Application.Events
{
    public class SessionEndEvent : AnalyticsEvent
    {
        public SessionEndEvent(string queueName, DateTime eventTime,
            string sessionId, string clientId, int priority,
            int id = -1) : base(
            queueName, eventTime, sessionId, clientId, priority, id)
        {
            _dataJson = JsonConvert.SerializeObject(_data);
        }
    }
}