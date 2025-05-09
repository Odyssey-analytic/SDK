using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace odysseyAnalytics.Core.Application.Events
{
    public abstract class AnalyticsEvent
    {
        protected string EventType;
        
        protected Dictionary<string, string> _data;
        protected string _dataJson;
        private DateTime _eventTime;
        private string _sessionId;
        private string _clientId;
        public int Id { get; set; }
        public string EventName { get; set; }
        public string QueueName { get; set; }

        public AnalyticsEvent(string queueName, DateTime eventTime, string sessionId,
            string clientId, int priority, Dictionary<string, string> data , int id = -1)
        {
            QueueName = queueName;
            _eventTime = eventTime;
            _sessionId = sessionId;
            _clientId = clientId;
            Priority = priority;
            Id = id;
            _data = data ?? new Dictionary<string, string>();
            EventType = GetType().FullName;
        }


        
        public DateTime EventTime
        {
            get => _eventTime;
            set { _eventTime = value; }
        }
        
        public string SessionId
        {
            get => _sessionId;
            set { _sessionId = value; }
        }
        
        public string ClientId
        {
            get => _clientId;
            set { _clientId = value; }
        }
        
        public int Priority { get; set; }
        
        public Dictionary<string, string> Data
        {
            get 
            {
                if (_data == null)
                {
                    try 
                    {
                        _data = JsonConvert.DeserializeObject<Dictionary<string, string>>(_dataJson) 
                             ?? new Dictionary<string, string>();
                    }
                    catch (Exception) 
                    {
                        // If deserialization fails, provide an empty dictionary
                        _data = new Dictionary<string, string>();
                    }
                }
                return _data;
            }
            set
            {
                _data = value ?? new Dictionary<string, string>();
                _dataJson = JsonConvert.SerializeObject(_data);
            }
        }

        public string GetRawDataJson()
        {
            _dataJson = JsonConvert.SerializeObject(Data);
            return _dataJson;
        }

        public void SetRawDataJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return;
            }
            
            try
            {
                var newData = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
                if (newData != null)
                {
                    foreach (var item in newData)
                    {
                        Data[item.Key] = item.Value;
                    }
                    _dataJson = json;
                }
            }
            catch (Exception ex)
            {
                throw new FormatException($"Invalid JSON format: {ex.Message}", ex);
            }
        }
    }
}
