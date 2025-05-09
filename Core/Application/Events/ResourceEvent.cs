using System;
using System.Collections.Generic;

namespace odysseyAnalytics.Core.Application.Events
{
    public class ResourceEvent : AnalyticsEvent
    {
        private string flowType;
        private string itemType;
        private string itemId;
        private int amount;
        private string resourceCurrency;

        public ResourceEvent(string flowType, string itemType, string itemId, int amount, string resourceCurrency,
            string eventName, string queueName, DateTime eventTime, string sessionId, string clientId, int priority,
            Dictionary<string, string> data, int id = -1) : base(eventName, queueName, eventTime, sessionId, clientId,
            priority, data, id)
        {
            this.flowType = flowType;
            this.itemType = itemType;
            this.itemId = itemId;
            this.amount = amount;
            this.resourceCurrency = resourceCurrency;
        }
    }
}