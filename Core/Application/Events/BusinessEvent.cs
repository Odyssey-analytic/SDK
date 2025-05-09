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


        public BusinessEvent(string queueName, DateTime eventTime, string sessionId, string clientId,
            int priority, Dictionary<string, string> data, int id, string cartType, string itemType, string itemId,
            int amount, string currency) : base(queueName, eventTime,
            sessionId, clientId, priority, data, id)
        {
            this.cartType = cartType;
            this.itemType = itemType;
            this.itemId = itemId;
            this.amount = amount;
            this.currency = currency;
        }
        
        
        
    }
    
}