using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using odysseyAnalytics.Core.Application.Events;

namespace odysseyAnalytics.Infrastructure.Persistence
{
    public class SqliteDTO
    {
        public int Id { get; set; }
        public string EventKey { get; set; }
        public string EventType { get; set; }
        public string QueueName { get; set; }
        public DateTime EventTime { get; set; }
        public string SessionId { get; set; }
        public string ClientId { get; set; }
        public int Priority { get; set; }
        public string DataJson { get; set; }

        public SqliteDTO()
        {
        }

        public SqliteDTO(AnalyticsEvent analyticsEvent)
        {
            Id = analyticsEvent.Id;
            EventKey = analyticsEvent.Id.ToString(); // Using Id as EventKey by default
            EventType = analyticsEvent.EventType;
            QueueName = analyticsEvent.QueueName;
            EventTime = analyticsEvent.EventTime;
            SessionId = analyticsEvent.SessionId;
            ClientId = analyticsEvent.ClientId;
            Priority = analyticsEvent.Priority;
            
            // Format the DataJson as required
            FormatDataJson(analyticsEvent);
        }

        private void FormatDataJson(AnalyticsEvent analyticsEvent)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            
            // Add the required base fields
            data["session"] = analyticsEvent.SessionId;
            data["client"] = analyticsEvent.ClientId;
            data["time"] = analyticsEvent.EventTime.ToString("yyyy-MM-dd HH:mm:ss");
            
            // Add event-specific fields
            if (analyticsEvent is SessionStartEvent)
            {
                if (analyticsEvent.Data.ContainsKey("platform"))
                    data["platform"] = analyticsEvent.Data["platform"];
            }
            else if (analyticsEvent is BusinessEvent)
            {
                if (analyticsEvent.Data.ContainsKey("cartType"))
                    data["cartType"] = analyticsEvent.Data["cartType"];
                if (analyticsEvent.Data.ContainsKey("itemType"))
                    data["itemType"] = analyticsEvent.Data["itemType"];
                if (analyticsEvent.Data.ContainsKey("itemId"))
                    data["itemId"] = analyticsEvent.Data["itemId"];
                if (analyticsEvent.Data.ContainsKey("amount"))
                    data["amount"] = analyticsEvent.Data["amount"];
                if (analyticsEvent.Data.ContainsKey("currency"))
                    data["currency"] = analyticsEvent.Data["currency"];
            }
            else if (analyticsEvent is ErrorEvent)
            {
                if (analyticsEvent.Data.ContainsKey("message"))
                    data["message"] = analyticsEvent.Data["message"];
                if (analyticsEvent.Data.ContainsKey("severity"))
                    data["severity"] = analyticsEvent.Data["severity"];
            }
            else if (analyticsEvent is ProgressionEvent)
            {
                if (analyticsEvent.Data.ContainsKey("progressionStatus"))
                    data["progressionStatus"] = analyticsEvent.Data["progressionStatus"];
                if (analyticsEvent.Data.ContainsKey("progression01"))
                    data["progression01"] = analyticsEvent.Data["progression01"];
                if (analyticsEvent.Data.ContainsKey("progression02"))
                    data["progression02"] = analyticsEvent.Data["progression02"];
                if (analyticsEvent.Data.ContainsKey("progression03"))
                    data["progression03"] = analyticsEvent.Data["progression03"];
                if (analyticsEvent.Data.ContainsKey("value"))
                    data["value"] = analyticsEvent.Data["value"];
            }
            else if (analyticsEvent is QualityEvent)
            {
                if (analyticsEvent.Data.ContainsKey("FPS"))
                    data["FPS"] = analyticsEvent.Data["FPS"];
                if (analyticsEvent.Data.ContainsKey("memoryUsage"))
                    data["memoryUsage"] = analyticsEvent.Data["memoryUsage"];
            }
            else if (analyticsEvent is ResourceEvent)
            {
                if (analyticsEvent.Data.ContainsKey("flowType"))
                    data["flowType"] = analyticsEvent.Data["flowType"];
                if (analyticsEvent.Data.ContainsKey("itemType"))
                    data["itemType"] = analyticsEvent.Data["itemType"];
                if (analyticsEvent.Data.ContainsKey("itemId"))
                    data["itemId"] = analyticsEvent.Data["itemId"];
                if (analyticsEvent.Data.ContainsKey("amount"))
                    data["amount"] = analyticsEvent.Data["amount"];
                if (analyticsEvent.Data.ContainsKey("resourceCurrency"))
                    data["resourceCurrency"] = analyticsEvent.Data["resourceCurrency"];
            }
            
            // Set the DataJson property
            DataJson = JsonConvert.SerializeObject(data);
        }

        public AnalyticsEvent ToAnalyticsEvent()
        {
            try
            {
                Type eventType = Type.GetType(EventType);
                if (eventType == null)
                {
                    throw new InvalidOperationException($"Could not find event type: {EventType}");
                }

                AnalyticsEvent result = null;
                Dictionary<string, string> data = JsonConvert.DeserializeObject<Dictionary<string, string>>(DataJson);

                if (eventType == typeof(BusinessEvent))
                {
                    result = new BusinessEvent(
                        "", // QueueName - will be set later if needed
                        EventTime,
                        SessionId,
                        ClientId,
                        Priority,
                        Id,
                        data.ContainsKey("cartType") ? data["cartType"] : "",
                        data.ContainsKey("itemType") ? data["itemType"] : "",
                        data.ContainsKey("itemId") ? data["itemId"] : "",
                        data.ContainsKey("amount") ? int.Parse(data["amount"]) : 0,
                        data.ContainsKey("currency") ? data["currency"] : ""
                    );
                }
                else if (eventType == typeof(ErrorEvent))
                {
                    result = new ErrorEvent(
                        "", // QueueName - will be set later if needed
                        EventTime,
                        SessionId,
                        ClientId,
                        Priority,
                        Id,
                        data.ContainsKey("severity")
                            ? (SeverityLevel)Enum.Parse(typeof(SeverityLevel), data["severity"])
                            : SeverityLevel.Info,
                        data.ContainsKey("message") ? data["message"] : ""
                    );
                }
                else if (eventType == typeof(ProgressionEvent))
                {
                    result = new ProgressionEvent(
                        "", // QueueName - will be set later if needed
                        EventTime,
                        SessionId,
                        ClientId,
                        Priority,
                        Id,
                        data.ContainsKey("progressionStatus") ? data["progressionStatus"] : "",
                        data.ContainsKey("progression01") ? data["progression01"] : "",
                        data.ContainsKey("progression02") ? data["progression02"] : "",
                        data.ContainsKey("progression03") ? data["progression03"] : "",
                        data.ContainsKey("value") ? float.Parse(data["value"]) : 0f
                    );
                }
                else if (eventType == typeof(QualityEvent))
                {
                    result = new QualityEvent(
                        data.ContainsKey("FPS") ? float.Parse(data["FPS"]) : 0f,
                        data.ContainsKey("memoryUsage") ? float.Parse(data["memoryUsage"]) : 0f,
                        "", // QueueName - will be set later if needed
                        EventTime,
                        SessionId,
                        ClientId,
                        Priority,
                        Id
                    );
                }
                else if (eventType == typeof(ResourceEvent))
                {
                    result = new ResourceEvent(
                        data.ContainsKey("flowType") ? data["flowType"] : "",
                        data.ContainsKey("itemType") ? data["itemType"] : "",
                        data.ContainsKey("itemId") ? data["itemId"] : "",
                        data.ContainsKey("amount") ? int.Parse(data["amount"]) : 0,
                        data.ContainsKey("resourceCurrency") ? data["resourceCurrency"] : "",
                        "", // eventName is not stored in _data
                        "", // QueueName - will be set later if needed
                        EventTime,
                        SessionId,
                        ClientId,
                        Priority,
                        Id
                    );
                }
                else if (eventType == typeof(SessionEndEvent))
                {
                    result = new SessionEndEvent(
                        QueueName,
                        EventTime,
                        SessionId,
                        ClientId,
                        Priority,
                        Id
                    );
                }
                else if (eventType == typeof(SessionStartEvent))
                {
                    result = new SessionStartEvent(
                        "", // QueueName - will be set later if needed
                        EventTime,
                        SessionId,
                        ClientId,
                        Priority,
                        data.ContainsKey("platform") ? data["platform"] : "",
                        Id
                    );
                }
                else
                {
                    try
                    {
                        ConstructorInfo[] constructors = eventType.GetConstructors();

                        foreach (var constructor in constructors)
                        {
                            var parameters = constructor.GetParameters();
                            if (parameters.Length > 0)
                            {
                                object[] paramValues = new object[parameters.Length];

                                // Attempt to populate parameters
                                bool canUseConstructor = true;
                                for (int i = 0; i < parameters.Length; i++)
                                {
                                    var param = parameters[i];

                                    // Check for specific standard parameters
                                    if (param.Name == "queueName")
                                        paramValues[i] = QueueName;
                                    else if (param.Name == "eventTime")
                                        paramValues[i] = EventTime;
                                    else if (param.Name == "sessionId")
                                        paramValues[i] = SessionId;
                                    else if (param.Name == "clientId")
                                        paramValues[i] = ClientId;
                                    else if (param.Name == "priority")
                                        paramValues[i] = Priority;
                                    else if (param.Name == "id")
                                        paramValues[i] = Id;
                                    else
                                    {
                                        // Look for the parameter in the data dictionary
                                        if (data.ContainsKey(param.Name))
                                        {
                                            // Try to convert the string value to the parameter type
                                            try
                                            {
                                                if (param.ParameterType == typeof(int))
                                                    paramValues[i] = int.Parse(data[param.Name]);
                                                else if (param.ParameterType == typeof(float))
                                                    paramValues[i] = float.Parse(data[param.Name]);
                                                else if (param.ParameterType == typeof(string))
                                                    paramValues[i] = data[param.Name];
                                                else if (param.ParameterType.IsEnum)
                                                    paramValues[i] = Enum.Parse(param.ParameterType, data[param.Name]);
                                                else
                                                {
                                                    // Can't handle this parameter type
                                                    canUseConstructor = false;
                                                    break;
                                                }
                                            }
                                            catch
                                            {
                                                // Conversion failed
                                                canUseConstructor = false;
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            // Parameter not found in data - use default
                                            if (param.HasDefaultValue)
                                                paramValues[i] = param.DefaultValue;
                                            else
                                            {
                                                // Required parameter not found
                                                canUseConstructor = false;
                                                break;
                                            }
                                        }
                                    }
                                }

                                if (canUseConstructor)
                                {
                                    // Try to invoke the constructor
                                    result = (AnalyticsEvent)constructor.Invoke(paramValues);
                                    break;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException(
                            $"Failed to create event instance using reflection: {ex.Message}", ex);
                    }
                }

                // If we couldn't create the event with any approach
                if (result == null)
                {
                    throw new InvalidOperationException($"Could not create event of type {EventType}");
                }

                // Ensure the data is correctly set
                if (!string.IsNullOrEmpty(DataJson))
                    result.SetRawDataJson(DataJson);

                return result;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to convert DTO to AnalyticsEvent: {ex.Message}", ex);
            }
        }
    }
}