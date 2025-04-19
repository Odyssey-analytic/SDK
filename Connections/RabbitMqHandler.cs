using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using odysseyAnalytics.Logging;
using odysseyAnalytics.Storage;


namespace odysseyAnalytics.Connections
{
    public class RabbitMqHandler
    {
        private IConnection _connection;
        private IChannel _channel;
        private readonly DefaultLogger _logger = new DefaultLogger();
        private readonly EventCacheManager _cacheManager;

        private Thread _flushThread;
        private bool _running = false;

        private bool HasStartSession { get; set; } = false;
        private bool IsConnected =>
            _connection != null && _connection.IsOpen &&
            _channel != null && _channel.IsOpen;

        public RabbitMqHandler(EventCacheManager cacheManager)
        {
            _cacheManager = cacheManager;
        }

        public async Task Connect(string host, string username, string password, string vHost = "/", int port = 5672, bool checkInternetConnectionFirst = false)
        {
            try
            {
                if (checkInternetConnectionFirst)
                    await ConnectionHandler.IsConnectedToInternet();

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

                StartFlusher();
            }
            catch (Exception ex)
            {
                _logger.Error("❌ Connection error: ", ex);
            }
        }

        public async Task PublishMessage(string queueName, string message)
        {
            if (!IsConnected)
            {
                _logger.Warn("Not connected, message cached.");
                return;
            }

            var body = Encoding.UTF8.GetBytes(message);
            await _channel.BasicPublishAsync(exchange: "", routingKey: queueName, body: body);
            _logger.Log($"[Sent Immediately] {message}");
        }

        private void StartFlusher()
        {
            if (_running) return;

            _running = true;
            _flushThread = new Thread(async () =>
            {
                while (_running)
                {
                    if (HasStartSession && IsConnectedToRabbitMq())
                    {
                        try
                        {
                            var events = _cacheManager.LoadAllEvents();
                            events.Sort((a, b) =>
                            {
                                int priorityComparison = a.Priority.CompareTo(b.Priority);
                                return priorityComparison == 0
                                    ? a.EventTime.CompareTo(b.EventTime)
                                    : priorityComparison;
                            });

                            int highestPendingPriority = events.Count > 0 ? events[0].Priority : int.MaxValue;

                            foreach (var evt in events)
                            {
                                if (evt.Priority > highestPendingPriority)
                                {
                                    _logger.Warn($"Blocked {evt.EventName} until higher priority events are sent.");
                                    break; // Exit the loop, retry later
                                }

                                try
                                {
                                    byte[] body = MessagePack.MessagePackSerializer.Serialize(evt.Data);
                                    await _channel.BasicPublishAsync(exchange: "", routingKey: evt.QueueName, body: body);
                                    _cacheManager.DeleteEvent(evt.Id);
                                    _logger.Log($"[Flushed] {evt.EventName} (Priority {evt.Priority}) at {evt.EventTime}");
                                }
                                catch (Exception e)
                                {
                                    _logger.Warn($"Failed to send {evt.EventName} (Priority {evt.Priority}) Because of {e.Message}, we will retry.");
                                    break;
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Error("Flushing failed", ex);
                        }
                    }

                    Thread.Sleep(8000); // Every 5s
                }
            });

            _flushThread.IsBackground = true;
            _flushThread.Start();
        }

        public void StopFlusher()
        {
            _running = false;
            _flushThread?.Join();
        }

        public async Task Close()
        {
            StopFlusher();
            await _channel?.CloseAsync();
            await _connection?.CloseAsync();
            _channel?.DisposeAsync();
            _connection?.DisposeAsync();
        }

        public bool IsConnectedToRabbitMq() => IsConnected;
    }
}
