using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace odysseyAnalytics.Core.Application.Events
{
    public class SessionStartEvent : AnalyticsEvent
    {
        private string platform;

        public SessionStartEvent(string queueName, DateTime eventTime, string sessionId,
            string clientId, int priority, string platform, int id = -1) : base(
            queueName,
            eventTime, sessionId, clientId, priority, id)
        {
            this.platform = platform;
            _data.Add("platform", platform);
            _dataJson = JsonConvert.SerializeObject(_data);
        }
    }
}