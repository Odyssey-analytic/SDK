using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Newtonsoft.Json;

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
            int priority, int id, string cartType, string itemType, string itemId,
            int amount, string currency) : base(queueName, eventTime,
            sessionId, clientId, priority,  id)
        {
            this.cartType = cartType;
            this.itemType = itemType;
            this.itemId = itemId;
            this.amount = amount;
            this.currency = currency;
            _data.Add("cartType", cartType);
            _data.Add("itemType", itemType);
            _data.Add("itemId", itemId);
            _data.Add("amount", amount.ToString());
            _data.Add("currency", currency);
            _dataJson = JsonConvert.SerializeObject(_data);

        }
        
        
        
    }
    
}