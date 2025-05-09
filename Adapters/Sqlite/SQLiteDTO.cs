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
            EventType = analyticsEvent.EventType;
            QueueName = analyticsEvent.QueueName;
            EventTime = analyticsEvent.EventTime;
            SessionId = analyticsEvent.SessionId;
            ClientId = analyticsEvent.ClientId;
            Priority = analyticsEvent.Priority;
            DataJson = analyticsEvent.GetRawDataJson();
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

                if (eventType == typeof(BusinessEvent))
                {
                    Dictionary<string, string> data =
                        JsonConvert.DeserializeObject<Dictionary<string, string>>(DataJson);
                    result = new BusinessEvent(
                        QueueName,
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
                    Dictionary<string, string> data =
                        JsonConvert.DeserializeObject<Dictionary<string, string>>(DataJson);
                    result = new ErrorEvent(
                        QueueName,
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
                    Dictionary<string, string> data =
                        JsonConvert.DeserializeObject<Dictionary<string, string>>(DataJson);
                    result = new ProgressionEvent(
                        QueueName,
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
                    Dictionary<string, string> data =
                        JsonConvert.DeserializeObject<Dictionary<string, string>>(DataJson);
                    result = new QualityEvent(
                        data.ContainsKey("FPS") ? float.Parse(data["FPS"]) : 0f,
                        data.ContainsKey("memoryUsage") ? float.Parse(data["memoryUsage"]) : 0f,
                        QueueName,
                        EventTime,
                        SessionId,
                        ClientId,
                        Priority,
                        Id
                    );
                }
                else if (eventType == typeof(ResourceEvent))
                {
                    Dictionary<string, string> data =
                        JsonConvert.DeserializeObject<Dictionary<string, string>>(DataJson);
                    result = new ResourceEvent(
                        data.ContainsKey("flowType") ? data["flowType"] : "",
                        data.ContainsKey("itemType") ? data["itemType"] : "",
                        data.ContainsKey("itemId") ? data["itemId"] : "",
                        data.ContainsKey("amount") ? int.Parse(data["amount"]) : 0,
                        data.ContainsKey("resourceCurrency") ? data["resourceCurrency"] : "",
                        "", // eventName is not stored in _data
                        QueueName,
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
                    Dictionary<string, string> data =
                        JsonConvert.DeserializeObject<Dictionary<string, string>>(DataJson);
                    result = new SessionStartEvent(
                        QueueName,
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

                                Dictionary<string, string> data =
                                    JsonConvert.DeserializeObject<Dictionary<string, string>>(DataJson);

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