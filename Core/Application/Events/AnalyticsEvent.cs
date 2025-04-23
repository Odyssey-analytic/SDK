using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace odysseyAnalytics.Core.Application.Events
{
    public class AnalyticsEvent
    {
        public AnalyticsEvent(string eventName, string queueName, DateTime eventTime, string sessionId,
            string clientId, int priority, Dictionary<string, string> data)
        {
            EventName = eventName;
            QueueName = queueName;
            EventTime = eventTime;
            SessionId = sessionId;
            ClientId = clientId;
            Priority = priority;
            Data = data;
            Data.Add("time", EventTime.ToString("yyyy-MM-dd HH:mm:ss.ffffff"));
            Data.Add("session", SessionId);
            Data.Add("client", ClientId);
        }

        public int Id { get; set; }
        public string EventName { get; set; }
        public string QueueName { get; set; }

        private DateTime _eventTime
        {
            get;
            set;
        }
        public DateTime EventTime
        {
            get => _eventTime;
            set
            {
                _eventTime = value;
                Data["time"] = _eventTime.ToString("yyyy-MM-dd HH:mm:ss.ffffff");
            }
        }
        

        private string _sessionId
        {
            get;
            set;
        }
        public string SessionId
        {
            get => _sessionId;
            set
            {
                _sessionId = value;
                Data["session"] = _sessionId;
            }
        }
        

        private string _clientId
        {
            get;
            set;
        }

        public string ClientId
        {
            get => _clientId;
            set
            {
                _clientId = value;
                Data["client"] = _clientId;
            }
        }

        public int Priority { get; set; }

        private string dataJson;

        public Dictionary<string, string> Data
        {
            get => JsonConvert.DeserializeObject<Dictionary<string, string>>(dataJson);
            set => dataJson = JsonConvert.SerializeObject(value);
        }

        public string GetRawDataJson() => dataJson;

        public void SetRawDataJson(string json)
        {
            var t = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
            foreach (var VARIABLE in t)
            {
                Data.Add(VARIABLE.Key, VARIABLE.Value);                       
            }
        }
    }
}