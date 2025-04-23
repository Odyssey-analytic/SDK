using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using odysseyAnalytics.Core.Application.Events;
using odysseyAnalytics.Core.Application.Exceptions;
using odysseyAnalytics.Core.Application.Gateway;
using odysseyAnalytics.Core.Ports;

namespace odysseyAnalytics.Core.Application.Session
{
    public class SessionHandler
    {
        public int SessionId { get; private set; }
        public int CID { get; private set; }

        private string username;
        private string password;
        private string token;
        private bool isSessionInitialized = false;

        private readonly IGatewayPort gatewayPort;
        private readonly IMessagePublisherPort messagePublisher;
        private readonly IConnectablePublisher connection;
        private readonly ILogger logger;
        private Dictionary<string, string> queues = new Dictionary<string, string>();

        public SessionHandler(
            IGatewayPort gatewayPort,
            IMessagePublisherPort messagePublisher,
            IConnectablePublisher connection,
            ILogger logger,
            string token
        )
        {
            this.gatewayPort = gatewayPort ?? throw new ArgumentNullException(nameof(gatewayPort));
            this.messagePublisher = messagePublisher ?? throw new ArgumentNullException(nameof(messagePublisher));
            this.connection = connection ?? throw new ArgumentNullException(nameof(connection));
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
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
                    }
                    catch (Exception ex)
                    {
                        if (ex is OdysseyAnalyticsException)
                        {
                            throw;
                        }

                        throw new NotConnectedToServerException("Failed to process server response", ex);
                    }
                }
                else
                {
                    throw new AuthenticationException($"Authentication failed with status: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                if (ex is OdysseyAnalyticsException)
                {
                    throw;
                }
                if (ex.Message.Contains("No such host") || ex.Message.Contains("network") || 
                    ex.Message.Contains("connection"))
                {
                    throw new NoInternetConnectionException("Cannot connect to analytics server", ex);
                }
                throw new NotConnectedToServerException("Failed to initialize session", ex);
            }
        }

        private void AddQueueFullNameToQueueList(JObject data)
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

        private void SetUserPassCidFromData(JObject data)
        {
            CID = data["cid"]?.Value<int>() ?? 0;
            username = data["rb_username"]?.ToString();
            password = data["rb_password"]?.ToString();
            
            if (CID <= 0)
            {
                throw new AuthenticationException("Invalid CID received from server");
            }
        }

        public async Task StartSessionAsync(string platform)
        {
            CheckSessionInitialized();
            
            try
            {
                Dictionary<string, string> data = new Dictionary<string, string>
                {
                    { "platform", platform }
                };
                
                var evt = new AnalyticsEvent(
                    "start_session", 
                    GetQueueName("start_session"), 
                    DateTime.UtcNow, 
                    SessionId.ToString(),
                    CID.ToString(), 
                    0, 
                    data
                );

                await messagePublisher.PublishMessage(evt);
            }
            catch (Exception ex)
            {
                throw new NotConnectedToServerException("Failed to start session", ex);
            }
        }

        public async Task EndSessionAsync()
        {
            CheckSessionInitialized();
            try
            {
                var evt = new AnalyticsEvent(
                    "end_session", 
                    GetQueueName("end_session"), 
                    DateTime.UtcNow, 
                    SessionId.ToString(),
                    CID.ToString(), 
                    5, 
                    new Dictionary<string, string>()
                );

                await messagePublisher.PublishMessage(evt);
                await connection.CloseAsync();
            }
            catch (Exception ex)
            {
                throw new NotConnectedToServerException("Failed to end session", ex);
            }
        }
        
        private string GetQueueName(string queueKey)
        {
            if (!queues.TryGetValue(queueKey, out string queueName))
            {
                throw new QueueNotFoundException(queueKey);
            }
            
            return queueName;
        }
        
        private void CheckSessionInitialized()
        {
            if (!isSessionInitialized)
            {
                throw new InvalidSessionException("Session not initialized. Call InitializeSessionAsync first.");
            }
        }
    }
}