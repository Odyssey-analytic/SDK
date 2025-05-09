using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace odysseyAnalytics.Core.Application.Events
{
    public class SessionEndEvent : AnalyticsEvent
    {
        public SessionEndEvent(string queueName, DateTime eventTime,
            string sessionId, string clientId, int priority, Dictionary<string, string> data = null,
            int id = -1) : base(
            queueName, eventTime, sessionId, clientId, priority, data, id)
        {
            EventType = SESSION_END_EVENT_TYPE;
            _data["time"] = EventTime.ToString("yyyy-MM-ddTHH:mm:ssZ");
            _data["session"] = SessionId;
            _data["client"] = ClientId;
            _dataJson = JsonConvert.SerializeObject(_data);
        }
    }
}