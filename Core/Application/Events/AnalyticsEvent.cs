using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using SQLite;
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE
using UnityEngine;
#endif

namespace odysseyAnalytics.Events
{
    public class AnalyticsEvent
    {
        [PrimaryKey, AutoIncrement] public int Id { get; set; }
        public string EventName { get; set; }
        
        public string QueueName { get; set; }
        public DateTime EventTime { get; set; }
        public string SessionId { get; set; }
        public string ClientId { get; set; }
        private string DataJson { get; set; }
        
        public int Priority { get; set; } 

        [Ignore] // Prevents this from being mapped to SQLite
        public Dictionary<string, object> Data
        {
            get
            {
                return JsonConvert.DeserializeObject<Dictionary<string, object>>(DataJson);
            }
            set
            {
                DataJson = JsonConvert.SerializeObject(value);
            }
        }
    }
}