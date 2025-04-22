using System;
using System.Collections.Generic;

namespace odysseyAnalytics.Adapters.RabbitMQ
{
    public class RabbitMQDTO
    {
        public string EventName { get; set; }
        public string QueueName { get; set; }
        public DateTime EventTime { get; set; }
        public string SessionId { get; set; }
        public string ClientId { get; set; }
        public int Priority { get; set; }
        public Dictionary<string, object> Data { get; set; }
    }
}