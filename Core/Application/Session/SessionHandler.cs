using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using odysseyAnalytics.Core.Application.Events;
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

        private readonly IGatewayPort gatewayPort;
        private readonly IMessagePublisherPort messagePublisher;
        private readonly IConnectablePublisher connection;

        private readonly Dictionary<string, string> queues = new Dictionary<string, string>();

        public SessionHandler(
            IGatewayPort gatewayPort,
            IMessagePublisherPort messagePublisher,
            IConnectablePublisher connection,
            ILogger logger,
            string token
            )
        {
            this.gatewayPort = gatewayPort;
            this.messagePublisher = messagePublisher;
            this.connection = connection;
            this.token = token;
            SessionId = new Random().Next(1, 10000);
        }

        public async Task InitializeSessionAsync()
        {
            GatewayPayload payload = new GatewayPayload();
            payload.Data=null;
            payload.Endpoint = "api/token/" ;
            payload.AccessToken = token;
            
            var response = await gatewayPort.FetchAsync(payload);

            if (response.StatusCode == "OK")
            {
                JObject data = JObject.Parse(response.Data);

                CID = data["cid"]?.Value<int>() ?? 0;
                username = data["rb_username"]?.ToString();
                password = data["rb_password"]?.ToString();

                foreach (var queue in data["queues"] as JArray)
                {
                    var name = queue["name"]?.ToString();
                    var fullname = queue["fullname"]?.ToString();
                    if (!string.IsNullOrWhiteSpace(name) && !string.IsNullOrWhiteSpace(fullname))
                        queues[name] = fullname;
                }

                await connection.ConnectAsync("185.8.172.219", username, password, "analytic");
            }
            else
            {
                throw new Exception("Failed to get credentials from gateway.");
            }
        }

        public async Task StartSessionAsync(string platform)
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("platform", platform);
            var evt = new AnalyticsEvent
            {
                EventName = "start_session",
                QueueName = queues["start_session"],
                EventTime = DateTime.UtcNow,
                SessionId = SessionId.ToString(),
                ClientId = CID.ToString(),
                Data = data
            };

            await messagePublisher.PublishMessage(evt);
        }

        public async Task EndSessionAsync()
        {
            var evt = new AnalyticsEvent
            {
                EventName = "end_session",
                QueueName = queues["end_session"],
                EventTime = DateTime.UtcNow,
                SessionId = SessionId.ToString(),
                ClientId = CID.ToString(),
                Data = new Dictionary<string, string>()
            };

            await messagePublisher.PublishMessage(evt);
            await connection.CloseAsync();
        }
    }
}