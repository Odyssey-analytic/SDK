using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace odysseyAnalytics.Core.Application.Events
{
    public class BusinessEvent : AnalyticsEvent
    {
        
        private string cartType;
        private string itemType;
        private string itemId;
        private int amount;
        private string currency;


        public BusinessEvent(string eventName, string queueName, DateTime eventTime, string sessionId, string clientId,
            int priority, Dictionary<string, string> data, int id, string cartType, string itemType, string itemId,
            int amount, string currency) : base(eventName, queueName, eventTime,
            sessionId, clientId, priority, data, id)
        {
            EventType = BUSINESS_EVENT_TYPE;
            this.cartType = cartType;
            this.itemType = itemType;
            this.itemId = itemId;
            this.amount = amount;
        }
        
        
        
    }
    
}