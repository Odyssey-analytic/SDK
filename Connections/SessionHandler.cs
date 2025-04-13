using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using odysseyAnalytics.Connections.ConnectionHandler;


namespace odysseyAnalytics.Connections
{
    public class SessionHandler
    {
        public string? SessionId { get; set; }
        public int? CID { get; set; }
        public ConnectionHandler? Connection { get; set; }
        
        public RabbitMqHandler? RabbitMQ { get; set; }  
        
        public Dictionary<string,string>Queues = new Dictionary<string, string>();

        public void SessionStart(string token)
        {
            this.Connection = new ConnectionHandler(token,"http://127.0.0.1:8000/");
            HttpResponseMessage credentials = await Connection.SendRequestAsync("api/token/", HttpMethod.Get);
            string? host = "localhost";
            string? username = "";
            string? password = "";
            string? payload = "";
            if (credentials.StatusCode == System.Net.HttpStatusCode.OK)
            {
                JObject data =JObject.Parse(credentials.Content.ReadAsStringAsync().Result);
                this.CID = data["cid"];
                username = data["rb_username"].ToString();
                password = data["rb_password"].ToString();
                JArray queues = (JArray)data["queues"];
                for (int i = 0; i < queues.Count; i++)
                {
                    this.Queues.Add(queues[i].Value<string>("name"), queues[i].Value<string>("fullname"));
                }
                string timestamp = DateTime.UtcNow.ToString("o");
                string nonce = Guid.NewGuid().ToString();
                string raw = $"{CID}-{timestamp}-{nonce}";
                using (var sha = SHA256.Create())
                {
                    byte[] bytes = Encoding.UTF8.GetBytes(raw);
                    byte[] hash = sha.ComputeHash(bytes);
                    this.SessionId = BitConverter.ToString(hash).Replace("-", "").ToLower();
                }
                System.Console.WriteLine("Your session has been created successfully.Here is the Session ID:");
                System.Console.WriteLine(this.SessionId);
                this.RabbitMQ = new RabbitMqHandler();
                await rabbitMqHandler.Connect('localhost', username, password , "analytic");
                System.Console.WriteLine("Rabbit MQ connection established.");
                
            }
            else
            {
                System.Console.WriteLine("An Error Has Occurred.");
            }
        }

        public void SendSessionStart()
        {
            var session_payload =
            new {
                time: DateTime.UtcNow.ToString("o"),
                client: this.CID,
                session: this.SessionId,
                platform: "android"
            };
            var message = JsonConvert.SerializeObject(session_payload);
            await RabbitMQ.PublishMessage(this.Queues["start_session"], message);

        }

        public void SessionEnd()
        {
            this.Connection.CloseConnection();
            this.RabbitMQ.Close()
        }
    }
}
