using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace odysseyAnalytics.Connections
{
    public class SessionHandler
    {
        public int SessionId { get; set; }
        public int CID { get; set; }
        public ConnectionHandler Connection { get; set; }
        
        private string username { get; set; }
        private string password { get; set; }
        public RabbitMqHandler RabbitMQ { get; set; }  
        
        private Dictionary<string,string>Queues = new Dictionary<string, string>();

        public SessionHandler(ConnectionHandler connection, RabbitMqHandler rabbitMQ)
        {
            Connection = connection;
            RabbitMQ = rabbitMQ;
            SessionId = new Random().Next(1,10000);
        }

        public async Task GetCreds()
        {
            HttpResponseMessage credentials = await Connection.SendRequestAsync("api/token/", HttpMethod.Get);
            if (credentials.StatusCode == System.Net.HttpStatusCode.OK)
            {
                JObject data = JObject.Parse(credentials.Content.ReadAsStringAsync().Result);
                Console.WriteLine(data);
                CID = int.Parse(data["cid"].ToString());
                username = data["rb_username"].ToString();
                password = data["rb_password"].ToString();
                JArray queues = (JArray)data["queues"];
                for (int i = 0; i < queues.Count; i++)
                {
                    this.Queues.Add(queues[i].Value<string>("name"), queues[i].Value<string>("fullname"));
                }

                foreach (var v in Queues)
                {
                    Console.WriteLine(v.Value);
                    Console.WriteLine(v.Key);
                }
            }
            else
            {
                System.Console.WriteLine("An Error Has Occurred.");
            }

            try
            {
                
                await RabbitMQ.Connect("odysseyanalytics.ir", username, password, "analytic");
                System.Console.WriteLine("Rabbit MQ connection established.");
            }
            catch(Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }

        public async void SessionStart()
        {
            var session_payload = new 
            {
                    time= DateTime.UtcNow.ToString("o"),
                    client= this.CID,
                    session= this.SessionId,
                    platform= "android"
                };
            var message = JsonConvert.SerializeObject(session_payload);
            await RabbitMQ.PublishMessage(this.Queues["start_session"], message);
        }
        
        public async void SessionEnd()
        {
            var session_payload = new 
            {
                time= DateTime.UtcNow.ToString("o"),
                client= this.CID,
                session= this.SessionId,
            };
            var message = JsonConvert.SerializeObject(session_payload);
            await RabbitMQ.PublishMessage(this.Queues["end_session"], message);
            this.Connection.Dispose();
            await this.RabbitMQ.Close();
            
        }
    }
}
