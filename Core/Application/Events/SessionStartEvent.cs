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
            string clientId, int priority, string platform, Dictionary<string, string> data = null, int id = -1) : base(
            queueName,
            eventTime, sessionId, clientId, priority, data, id)
        {
            this.platform = platform;
            _data["time"] = EventTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
            _data["session"] = SessionId;
            _data["client"] = ClientId;
            _data["platform"] = platform;
            _dataJson = JsonConvert.SerializeObject(_data);
        }
    }
}