using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace odysseyAnalytics.Core.Application.Events
{
    public class QualityEvent : AnalyticsEvent
    {
        private float FPS;
        private float memoryUsage;

        public QualityEvent(float fps, float memoryUsage, string queueName, DateTime eventTime,
            string sessionId, string clientId, int priority, int id = -1) : base(
            queueName, eventTime, sessionId, clientId, priority, id)
        {
            FPS = fps;
            this.memoryUsage = memoryUsage;
            _data.Add("FPS",FPS.ToString("0.000"));
            _data.Add("memoryUsage", memoryUsage.ToString("0.000"));
            _dataJson = JsonConvert.SerializeObject(_data);

        }
    }
}