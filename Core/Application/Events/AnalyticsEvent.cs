using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace odysseyAnalytics.Core.Application.Events
{
    public class AnalyticsEvent
    {
        public int Id { get; set; }
        public string EventName { get; set; }
        public string QueueName { get; set; }
        public DateTime EventTime { get; set; }
        public string SessionId { get; set; }
        public string ClientId { get; set; }
        public int Priority { get; set; }

        private string dataJson;

        public Dictionary<string, string> Data
        {
            get => JsonConvert.DeserializeObject<Dictionary<string, string>>(dataJson);
            set => dataJson = JsonConvert.SerializeObject(value);
        }

        public string GetRawDataJson() => dataJson;
        public void SetRawDataJson(string json) => dataJson = json;
    }
}