using System;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using System.Net.Http;
using MessagePack;
using odysseyAnalytics.Exceptions;
using odysseyAnalytics.Logging;
using odysseyAnalytics.Types;

namespace odysseyAnalytics.Connections
{
    public class RabbitMqHandler
    {
        private IConnection _connection;
        private IChannel _channel;

        private bool _isConnected =>
            _connection != null && _connection.IsOpen &&
            _channel != null && _channel.IsOpen;

        private DefaultLogger _logger = new DefaultLogger();

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
                _logger.Log("Connected to RabbitMQ");
            }
            catch (NoInternetConnectionException ex)
            {
                _logger.Error("❌ No internet: ", ex);
            }
            catch (TaskCanceledException ex)
            {
                _logger.Error("❌ Connection timed out: ", ex);
            }
            catch (HttpRequestException ex)
            {
                _logger.Error("❌ HTTP error during internet check: ", ex);
            }
            catch (Exception ex)
            {
                _logger.Error("❌ Unexpected error: ", ex);
            }
        }

        public async Task PublishMessage(string queueName, string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            await _channel.BasicPublishAsync(exchange: "", routingKey: queueName, body: body);
            _logger.Log($"Sent: {message}");
        }

        public async Task Close()
        {
            await _channel?.CloseAsync();
            await _connection?.CloseAsync();
            _channel?.DisposeAsync();
            _connection?.DisposeAsync();
        }

        public bool IsConnectedToRabbitMq()
        {
            return _isConnected;
        }
    }
}