using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using odysseyAnalytics.Core.Application.Events;
using odysseyAnalytics.Core.Application.Exceptions;
using odysseyAnalytics.Core.Application.Gateway;
using odysseyAnalytics.Core.Ports;
using odysseyAnalytics.Core.Application.CacheManager;

namespace odysseyAnalytics.Core.Application.Session
{
    public class SessionHandler
    {
        private int SessionId { get; set; }
        private int CID { get; set; }

        private string username;
        private string password;
        private readonly string token;
        private bool isSessionInitialized = false;

        private readonly IGatewayPort gatewayPort;
        private readonly IMessagePublisherPort messagePublisher;
        private readonly IConnectablePublisher connection;
        private readonly ILogger logger;
        private readonly IDatabasePort databasePort;
        private CacheHandler cacheHandler;

        private Dictionary<string, string> queues = new Dictionary<string, string>();

        public SessionHandler(
            IGatewayPort gatewayPort,
            IMessagePublisherPort messagePublisher,
            IConnectablePublisher connection,
            ILogger logger,
            IDatabasePort databasePort,
            string token
        )
        {
            this.gatewayPort = gatewayPort ?? throw new ArgumentNullException(nameof(gatewayPort));
            this.messagePublisher = messagePublisher ?? throw new ArgumentNullException(nameof(messagePublisher));
            this.connection = connection ?? throw new ArgumentNullException(nameof(connection));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.databasePort = databasePort ?? throw new ArgumentNullException(nameof(databasePort));
            this.cacheHandler = new CacheHandler(this.databasePort);
            this.token = token;
            SessionId = new Random().Next(1, 10000);
        }

        public async Task InitializeSessionAsync()
        {
            try
            {
                logger.Log("Initializing session...");
                GatewayPayload payload = new GatewayPayload("api/token/", null, token);
                logger.Log("Payload Generated");

                var response = await gatewayPort.FetchAsync(payload);

                if (response == null)
                {
                    throw new NotConnectedToServerException("Failed to get response from server");
                }

                if (response.StatusCode == "OK")
                {
                    try
                    {
                        JObject data = JObject.Parse(response.Data);
                        SetUserPassCidFromData(data);
                        AddQueueFullNameToQueueList(data);
                        logger.Log($"Found {queues.Count} queues");

                        // Check for missing credentials
                        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                        {
                            throw new AuthenticationException("Missing credentials in server response");
                        }

                        // Connect to message broker
                        await connection.ConnectAsync("185.8.172.219", username, password, "analytic");
                        isSessionInitialized = true;

                        #region CacheHandlingRegion

                        List<AnalyticsEvent> cacheEvents = cacheHandler.LoadAllEvents();

                        if (cacheEvents.Count > 0)
                        {
                            logger.Log($"Found {cacheEvents.Count} analytic cache events");
                            foreach (var evt in cacheEvents)
                            {
                                logger.Log($"Found analytic cache event {evt.Id}");
                                try
                                {
                                    switch (evt.EventType.Split('.').Last())
                                    {
                                        case "SessionStartEvent":
                                            evt.QueueName=GetQueueName("start_session");
                                            break;

                                        case "SessionEndEvent":
                                            evt.QueueName=GetQueueName("end_session");
                                            break;

                                        case "ResourceEvent":
                                            evt.QueueName=GetQueueName("resource_event");
                                            break;

                                        case "QualityEvent":
                                            evt.QueueName=GetQueueName("quality_event");
                                            break;

                                        case "ProgressionEvent":
                                            evt.QueueName=GetQueueName("progression_event");
                                            break;
                                        case "ErrorEvent":
                                            evt.QueueName=GetQueueName("error_event");
                                            break;
                                        case "BusinessEvent":
                                            evt.QueueName=GetQueueName("business_event");
                                            break;
                                        default:
                                            throw new Exception("Unknown event type");
                                    }
                                    evt.ClientId = CID.ToString();
                                    evt.Data["client"] = CID.ToString();
                                    if (evt.QueueName == null)
                                    {
                                        throw new Exception("Unknown event type");
                                    }
                                    await messagePublisher.PublishMessage(evt);
                                    cacheHandler.DeleteEvent(evt.Id);
                                }
                                catch
                                {
                                    logger.Log($"Error publishing analytic cache event {evt.Id}");
                                }
                            }
                        }

                        #endregion
                    }
                    catch (Exception ex)
                    {
                        if (ex is OdysseyAnalyticsException)
                        {
                            logger.Error(null, ex);
                        }

                        logger.Error(null, new NotConnectedToServerException("Failed to get response from server"));
                    }
                }
                else
                {
                    logger.Error(null,
                        new AuthenticationException($"Authentication failed with status: {response.StatusCode}"));
                }
            }
            catch (Exception ex)
            {
                if (ex is OdysseyAnalyticsException)
                {
                    logger.Error(null, ex);
                }

                if (ex.Message.Contains("No such host") || ex.Message.Contains("network") ||
                    ex.Message.Contains("connection"))
                {
                    logger.Error(null, new NoInternetConnectionException("Cannot connect to analytics server"));
                }

                logger.Error(null, new NotConnectedToServerException("Failed to initialize session"));
            }
        }

        private void AddQueueFullNameToQueueList(JObject data)
        {
            try
            {
                if (data["queues"] == null || !(data["queues"] is JArray))
                {
                    throw new NotConnectedToServerException("Invalid server response: queues data missing or invalid");
                }

                foreach (var queue in data["queues"] as JArray)
                {
                    var name = queue["name"]?.ToString();
                    var fullname = queue["fullname"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(fullname))
                        queues[name] = fullname;
                }

                foreach (var essentialQueue in new[] { "start_session", "end_session" })
                {
                    if (!queues.ContainsKey(essentialQueue))
                    {
                        throw new QueueNotFoundException(essentialQueue, "Required queue not found in server response");
                    }
                }
            }
            catch (Exception e)
            {
                if (e is OdysseyAnalyticsException)
                {
                    logger.Error(null, e);
                }
                else
                {
                    logger.Error(null, new Exception("an error occurred for given queue ", e));
                }
            }
        }

        private void SetUserPassCidFromData(JObject data)
        {
            CID = data["cid"]?.Value<int>() ?? 0;
            username = data["rb_username"]?.ToString();
            password = data["rb_password"]?.ToString();
        }

        public async Task SendBusinessEventAsync(string cartType,
            string itemType,
            string itemId,
            int amount,
            string currency)
        {
            #region SavingIntoCache

            // mehid joon data ro nullable kardam
            if (!isSessionInitialized)
            {
                try
                {
                    var evt = new BusinessEvent(GetQueueName("business_event"), DateTime.Now, SessionId.ToString(),
                        CID.ToString(), 0,-1,cartType, itemType, itemId, amount, currency);
                    cacheHandler.SaveEvent(evt);
                    logger.Log("Saved event in DB");
                }
                catch (Exception e)
                {
                    logger.Error(null, e);
                }
            }

            #endregion

            else
            {
                try
                {

                    #region MUSTBEDEBUGGED

                    // var evt = new SessionStartEvent(GetQueueName("start_session"), DateTime.UtcNow,
                    //     SessionId.ToString(), CID.ToString(), 0, platform);
                    var evt = new BusinessEvent(GetQueueName("business_event"), DateTime.Now, SessionId.ToString(),
                        CID.ToString(), 0,-1,cartType, itemType, itemId, amount, currency);
                    await messagePublisher.PublishMessage(evt);
                    #endregion
                }
                catch (Exception ex)
                {
                    if (ex is QueueNotFoundException)
                    {
                        logger.Error(null, ex);
                        return;
                    }

                    #region CacheHandling

                    var evt = new BusinessEvent(GetQueueName("business_event"), DateTime.Now, SessionId.ToString(),
                        CID.ToString(), 0,-1,cartType, itemType, itemId, amount, currency);
                    cacheHandler.SaveEvent(evt);
                    logger.Log("Saved event in DB");

                    #endregion

                    logger.Error(null, new NotConnectedToServerException("Failed to Connect to server"));
                }
            }
        }
        public async Task SendErrorEventAsync(SeverityLevel severityLevel, string message)
        {
            #region SavingIntoCache

            // mehid joon data ro nullable kardam
            if (!isSessionInitialized)
            {
                try
                {
                    var evt = new ErrorEvent(GetQueueName("error_event"), DateTime.Now, SessionId.ToString(),
                        CID.ToString(), 0,-1,severityLevel, message);
                    cacheHandler.SaveEvent(evt);
                    logger.Log("Saved event in DB");
                }
                catch (Exception e)
                {
                    logger.Error(null, e);
                }
            }

            #endregion

            else
            {
                try
                {

                    #region MUSTBEDEBUGGED
                    var evt = new ErrorEvent(GetQueueName("error_event"), DateTime.Now, SessionId.ToString(),
                        CID.ToString(), 0,-1,severityLevel, message);
                    await messagePublisher.PublishMessage(evt);
                    #endregion
                }
                catch (Exception ex)
                {
                    if (ex is QueueNotFoundException)
                    {
                        logger.Error(null, ex);
                        return;
                    }

                    #region CacheHandling

                    var evt = new ErrorEvent(GetQueueName("error_event"), DateTime.Now, SessionId.ToString(),
                        CID.ToString(), 0,-1,severityLevel, message);
                    cacheHandler.SaveEvent(evt);
                    logger.Log("Saved event in DB");

                    #endregion

                    logger.Error(null, new NotConnectedToServerException("Failed to Connect to server"));
                }
            }
        }
        public async Task SendProgressionEventAsync(string progressionStatus,string progression01, string progression02, string progression03, float value)
        {
            #region SavingIntoCache

            // mehid joon data ro nullable kardam
            if (!isSessionInitialized)
            {
                try
                {
                    var evt = new ProgressionEvent(GetQueueName("progression_event"), DateTime.Now, SessionId.ToString(),
                        CID.ToString(), 0,-1,progressionStatus, progression01, progression02, progression03, value);
                    cacheHandler.SaveEvent(evt);
                    logger.Log("Saved event in DB");
                }
                catch (Exception e)
                {
                    logger.Error(null, e);
                }
            }

            #endregion

            else
            {
                try
                {

                    #region MUSTBEDEBUGGED

                    // var evt = new SessionStartEvent(GetQueueName("start_session"), DateTime.UtcNow,
                    //     SessionId.ToString(), CID.ToString(), 0, platform);
                    var evt = new ProgressionEvent(GetQueueName("progression_event"), DateTime.Now, SessionId.ToString(),
                        CID.ToString(), 0,-1,progressionStatus, progression01, progression02, progression03, value);
                    await messagePublisher.PublishMessage(evt);
                    #endregion
                }
                catch (Exception ex)
                {
                    if (ex is QueueNotFoundException)
                    {
                        logger.Error(null, ex);
                        return;
                    }

                    #region CacheHandling

                    var evt = new ProgressionEvent(GetQueueName("progression_event"), DateTime.Now, SessionId.ToString(),
                        CID.ToString(), 0,-1,progressionStatus, progression01, progression02, progression03, value);
                    cacheHandler.SaveEvent(evt);
                    logger.Log("Saved event in DB");

                    #endregion

                    logger.Error(null, new NotConnectedToServerException("Failed to Connect to server"));
                }
            }
        }
        public async Task SendQualityEventAsync(float fps, float memoryUsage)
        {
            #region SavingIntoCache

            // mehid joon data ro nullable kardam
            if (!isSessionInitialized)
            {
                try
                {
                    var evt = new QualityEvent(fps,memoryUsage,GetQueueName("quality_event"), DateTime.Now, SessionId.ToString(),
                        CID.ToString(), 0,-1);
                    cacheHandler.SaveEvent(evt);
                    logger.Log("Saved event in DB");
                }
                catch (Exception e)
                {
                    logger.Error(null, e);
                }
            }

            #endregion

            else
            {
                try
                {

                    #region MUSTBEDEBUGGED

                    // var evt = new SessionStartEvent(GetQueueName("start_session"), DateTime.UtcNow,
                    //     SessionId.ToString(), CID.ToString(), 0, platform);
                    var evt = new QualityEvent(fps,memoryUsage,GetQueueName("quality_event"), DateTime.Now, SessionId.ToString(),
                        CID.ToString(), 0,-1);
                    await messagePublisher.PublishMessage(evt);
                    #endregion
                }
                catch (Exception ex)
                {
                    if (ex is QueueNotFoundException)
                    {
                        logger.Error(null, ex);
                        return;
                    }

                    #region CacheHandling

                    var evt = new QualityEvent(fps,memoryUsage,GetQueueName("quality_event"), DateTime.Now, SessionId.ToString(),
                        CID.ToString(), 0,-1);
                    cacheHandler.SaveEvent(evt);
                    logger.Log("Saved event in DB");

                    #endregion

                    logger.Error(null, new NotConnectedToServerException("Failed to Connect to server"));
                }
            }
        }
        public async Task SendResourceEventAsync(string flowType, string itemType, string itemId, int amount, string resourceCurrency)
        {
            #region SavingIntoCache

            // mehid joon data ro nullable kardam
            if (!isSessionInitialized)
            {
                try
                {
                    var evt = new ResourceEvent(flowType,itemType,itemId,amount,resourceCurrency,GetQueueName("resource_event"), DateTime.Now, SessionId.ToString(),
                        CID.ToString(), 0,-1);
                    cacheHandler.SaveEvent(evt);
                    logger.Log("Saved event in DB");
                }
                catch (Exception e)
                {
                    logger.Error(null, e);
                }
            }

            #endregion

            else
            {
                try
                {

                    #region MUSTBEDEBUGGED

                    // var evt = new SessionStartEvent(GetQueueName("start_session"), DateTime.UtcNow,
                    //     SessionId.ToString(), CID.ToString(), 0, platform);
                    var evt = new ResourceEvent(flowType,itemType,itemId,amount,resourceCurrency,GetQueueName("resource_event"), DateTime.Now, SessionId.ToString(),
                        CID.ToString(), 0,-1);
                    await messagePublisher.PublishMessage(evt);
                    #endregion
                }
                catch (Exception ex)
                {
                    if (ex is QueueNotFoundException)
                    {
                        logger.Error(null, ex);
                        return;
                    }

                    #region CacheHandling

                    var evt = new ResourceEvent(flowType,itemType,itemId,amount,resourceCurrency,GetQueueName("resource_event"), DateTime.Now, SessionId.ToString(),
                        CID.ToString(), 0,-1);
                    cacheHandler.SaveEvent(evt);
                    logger.Log("Saved event in DB");

                    #endregion

                    logger.Error(null, new NotConnectedToServerException("Failed to Connect to server"));
                }
            }
        }
        public async Task StartSessionAsync(string platform)
        {
            #region SavingIntoCache

            // mehid joon data ro nullable kardam
            if (!isSessionInitialized)
            {
                try
                {
                    var evt = new SessionStartEvent(GetQueueName("start_session"), DateTime.Now, SessionId.ToString(),
                        CID.ToString(), 0, platform);
                    cacheHandler.SaveEvent(evt);
                    logger.Log("Saved event in DB");
                }
                catch (Exception e)
                {
                    logger.Error(null, e);
                }
            }

            #endregion

            else
            {
                try
                {
                    var evt = new SessionStartEvent(GetQueueName("start_session"), DateTime.UtcNow,
                        SessionId.ToString(), CID.ToString(), 0, platform);
                    await messagePublisher.PublishMessage(evt);
                }
                catch (Exception ex)
                {
                    if (ex is QueueNotFoundException)
                    {
                        logger.Error(null, ex);
                        return;
                    }

                    #region CacheHandling

                    var evt = new SessionStartEvent(GetQueueName("start_session"), DateTime.Now, SessionId.ToString(),
                        CID.ToString(), 0, platform);
                    cacheHandler.SaveEvent(evt);
                    logger.Log("Saved event in DB");

                    #endregion

                    logger.Error(null, new NotConnectedToServerException("Failed to Connect to server"));
                }
            }
        }

        public async Task EndSessionAsync()
        {
            #region CacheHandling

            // Dictionary<string, string> data = new Dictionary<string, string>();
            // data ro nullable kardam mehdi joon
            if (!isSessionInitialized)
            {
                try
                {
                    var evt = new SessionEndEvent(GetQueueName("end_session"), DateTime.Now, SessionId.ToString(),
                        CID.ToString(), 5);
                    cacheHandler.SaveEvent(evt);
                    logger.Log("Saved event in DB");
                }
                catch (Exception e)
                {
                    logger.Error("An Error Happened in Cache: ", e);
                }
            }

            #endregion

            else
            {
                try
                {
                    var evt = new SessionEndEvent(GetQueueName("end_session"), DateTime.UtcNow,
                        SessionId.ToString(), CID.ToString(), 5);

                    await messagePublisher.PublishMessage(evt);
                }
                catch (Exception ex)
                {
                    if (ex is QueueNotFoundException)
                    {
                        logger.Error(null, ex);
                        return;
                    }

                    #region CacheHandling

                    var evt = new SessionEndEvent(GetQueueName("end_session"), DateTime.Now, SessionId.ToString(),
                        CID.ToString(), 5);
                    cacheHandler.SaveEvent(evt);
                    logger.Log("Saved event in DB");

                    #endregion

                    logger.Error(null, new NotConnectedToServerException("Failed to Connect to server"));
                }
            }

            cacheHandler.Close();
        }

        private string GetQueueName(string queueKey)
        {
            if (!queues.TryGetValue(queueKey, out string queueName))
            {
                logger.Error(null, new QueueNotFoundException(queueKey));
            }

            return queueName;
        }
    }
}