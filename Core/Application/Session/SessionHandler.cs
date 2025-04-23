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
            this.gatewayPort = gatewayPort;
            this.messagePublisher = messagePublisher;
            this.connection = connection;
            this.token = token;
            this.logger = logger;
            SessionId = new Random().Next(1, 10000);
        }

        public async Task InitializeSessionAsync()
        {
            logger.Log("Initializing session...");
            GatewayPayload payload = new GatewayPayload("api/token/", null, token);
            logger.Log("payload_generated");
            Console.WriteLine("INITIALIZE SESSION");
            Console.WriteLine(payload.AccessToken);
            var response = await gatewayPort.FetchAsync(payload);
            Console.WriteLine(response.StatusCode);


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

                Console.WriteLine(queues.Count);
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
            (
                "start_session", queues["start_session"], DateTime.UtcNow, SessionId.ToString(),
                CID.ToString(), 0, new Dictionary<string, string>()
            );

            await messagePublisher.PublishMessage(evt);
        }

        public async Task EndSessionAsync()
        {
            var evt = new AnalyticsEvent
            (
                "end_session", queues["end_session"], DateTime.UtcNow, SessionId.ToString(),
                CID.ToString(), 5, new Dictionary<string, string>()
            );

            await messagePublisher.PublishMessage(evt);
            await connection.CloseAsync();
        }
    }
}