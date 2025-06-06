using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

namespace odysseyAnalytics.Core.Application.Events
{
    public class CustomEvent : AnalyticsEvent
    {
        private string depth1;
        private string depth2;
        private string depth3;
        private string depth4;
        private string depth5;
        private float? value;
        private int depth = 0;


        public CustomEvent(string queueName, DateTime eventTime, string sessionId, string clientId,
            int priority, int id, string depth1 = null, string depth2 = null, string depth3 = null,
            string depth4= null, string depth5 = null, float? value = null) : base(queueName, eventTime,
            sessionId, clientId, priority, id)
        {
            this.depth1 = depth1;
            this.depth2 = depth2;
            this.depth3 = depth3;
            this.depth4 = depth4;
            this.depth5 = depth5;
            this.value = value;
            
            
            if (!string.IsNullOrEmpty(depth1)) depth++;
            if (!string.IsNullOrEmpty(depth2)) depth++;
            if (!string.IsNullOrEmpty(depth3)) depth++;
            if (!string.IsNullOrEmpty(depth4)) depth++;
            if (!string.IsNullOrEmpty(depth5)) depth++;
            
            
            _data.Add("custom_field1", depth1);
            _data.Add("custom_field2", depth2);
            _data.Add("custom_field3", depth3);
            _data.Add("custom_field4", depth4);
            _data.Add("custom_field5", depth5);
            _data.Add("value", value.ToString());
            _dataJson = JsonConvert.SerializeObject(_data);
        }
    }
}