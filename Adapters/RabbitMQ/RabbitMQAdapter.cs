using System.Threading.Tasks;
using Newtonsoft.Json;
using odysseyAnalytics.Core.Application.Events;
using odysseyAnalytics.Core.Ports;
using RabbitMQ.Client;

namespace odysseyAnalytics.Adapters.RabbitMQ
{
    public class RabbitMQAdapter : IMessagePublisherPort
    {
        private IConnection _connection;
        private IChannel _channel;
        
        public Task<T> PublishMessage<T>(T analyticsEvent) where T : AnalyticsEvent
        {
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

            _connection =await factory.CreateConnectionAsync();
            _channel = await _connection.CreateChannelAsync();
        }

        private RabbitMQDTO MapToDto(AnalyticsEvent evt)
        {
            return new RabbitMQDTO
            {
                EventName = evt.EventName,
                QueueName = evt.QueueName,
                EventTime = evt.EventTime,
                SessionId = evt.SessionId,
                ClientId = evt.ClientId,
                Priority = evt.Priority,
                Data = evt.Data
            };
        }
    }
    
}