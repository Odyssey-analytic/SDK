using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace odysseyAnalytics.Core.Application.Events
{
    public class ProgressionEvent : AnalyticsEvent
    {
        private string progressionStatus;
        private string progression01;
        private string progression02;
        private string progression03;
        private float value;

        public ProgressionEvent(string queueName, DateTime eventTime, string sessionId,
            string clientId, int priority, int id, string progressionStatus,
            string progression01, string progression02, string progression03, float value) : base(queueName,
            eventTime, sessionId, clientId, priority, id)
        {
            this.progressionStatus = progressionStatus;
            this.progression01 = progression01;
            this.progression02 = progression02;
            this.progression03 = progression03;
            this.value = value;
            _data.Add("progressionStatus", progressionStatus);
            _data.Add("progression01", progression01);
            _data.Add("progression02", progression02);
            _data.Add("progression03", progression03);
            _data.Add("value", value.ToString("0.000"));
            _dataJson = JsonConvert.SerializeObject(_data);

        }
    }
}