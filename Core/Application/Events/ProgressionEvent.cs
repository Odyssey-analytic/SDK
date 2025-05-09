using System;
using System.Collections.Generic;

namespace odysseyAnalytics.Core.Application.Events
{
    public class ProgressionEvent : AnalyticsEvent
    {
        private string progressionStatus;
        private string progression01;
        private string progression02;
        private string progression03;
        private float value;

        public ProgressionEvent(string eventName, string queueName, DateTime eventTime, string sessionId,
            string clientId, int priority, Dictionary<string, string> data, int id, string progressionStatus,
            string progression01, string progression02, string progression03, float value) : base(eventName, queueName,
            eventTime, sessionId, clientId, priority, data, id)
        {
            this.progressionStatus = progressionStatus;
            this.progression01 = progression01;
            this.progression02 = progression02;
            this.progression03 = progression03;
            this.value = value;
        }
    }
}