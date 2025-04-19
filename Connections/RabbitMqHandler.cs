using System;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using System.Net.Http;
using MessagePack;
using OdysseyAnalytics.Exceptions;


namespace odysseyAnalytics.Connections
{
    public class RabbitMqHandler
    {
        private IConnection _connection;
        private IChannel _channel;
        private bool _isConnected = false;

        public async Task Connect(string host, string username, string password, string vHost = "/", int port = 5672,
            bool checkInternetConnectionFirst = false)
        {
            try
            {
                if (checkInternetConnectionFirst)
                {
                    await ConnectionHandler.IsConnectedToInternet();
                }

                var factory = new ConnectionFactory()
                {
                    HostName = host,
                    UserName = username,
                    Password = password,
                    Port = port,
                    VirtualHost = vHost,
                    AutomaticRecoveryEnabled = true,
                    NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
                };
                _connection = await factory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();
                _isConnected = true;
                Console.WriteLine("Connected to RabbitMQ");
            }
            catch (NoInternetConnectionException ex)
            {
                Console.WriteLine("❌ No internet: " + ex.Message);
            }
            catch (TaskCanceledException ex)
            {
                Console.WriteLine("❌ Connection timed out: " + ex.Message);
            }
            catch (HttpRequestException ex)
            {
                Console.WriteLine("❌ HTTP error during internet check: " + ex.Message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Unexpected error: " + ex.Message);
            }
        }

        public async Task PublishMessage(string queueName, string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            await _channel.BasicPublishAsync(exchange: "", routingKey: queueName, body: body);
            Console.WriteLine($"Sent: {message}");
        }

        public async Task Close()
        {
            await _channel?.CloseAsync();
            await _connection?.CloseAsync();
            _channel?.DisposeAsync();
            _connection?.DisposeAsync();
        }
    }
}