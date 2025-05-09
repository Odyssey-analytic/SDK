using System;
using System.Collections.Generic;
using Newtonsoft.Json;

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
            int id = -1) : base(queueName, eventTime, sessionId, clientId,
            priority, id)
        {
            this.flowType = flowType;
            this.itemType = itemType;
            this.itemId = itemId;
            this.amount = amount;
            this.resourceCurrency = resourceCurrency;
            _data.Add("flowType",flowType);
            _data.Add("itemType", itemType);
            _data.Add("itemId", itemId);
            _data.Add("amount", amount.ToString());
            _data.Add("resourceCurrency", resourceCurrency);
            _dataJson = JsonConvert.SerializeObject(_data);

        }
    }
}