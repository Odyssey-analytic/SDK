using System;
using System.Threading.Tasks;
using Newtonsoft.Json;
using odysseyAnalytics.Core.Application.Events;
using odysseyAnalytics.Core.Ports;
using RabbitMQ.Client;
using System.Collections.Generic;
using System.Text;

namespace odysseyAnalytics.Adapters.RabbitMQ
{
    public class RabbitMQAdapter : IMessagePublisherPort, IConnectablePublisher
    {
        private IConnection connection;
        private IChannel channel;

        public async Task ConnectAsync(string host, string username, string password, int port = 5672,
            string vhost = "/")
        {
            var factory = new ConnectionFactory()
            {
                HostName = host,
                UserName = username,
                Password = password,
                Port = port,
                VirtualHost = vhost,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(5)
            };

            connection = await factory.CreateConnectionAsync();
            channel = await connection.CreateChannelAsync();

            await Task.CompletedTask;
        }

        public async Task<T> PublishMessage<T>(T analyticsEvent) where T : AnalyticsEvent
        {
            if (channel == null)
                throw new System.InvalidOperationException("Not connected to message broker.");

            string queueName = analyticsEvent.QueueName;
            string message = analyticsEvent.GetRawDataJson();

            var body = Encoding.UTF8.GetBytes(message);

            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: queueName,
                body: body);

            return await Task.FromResult(analyticsEvent);
        }

        public async Task CloseAsync()
        {
            await channel?.CloseAsync();
            connection?.CloseAsync();
            channel?.DisposeAsync();
            connection?.DisposeAsync();
            await Task.CompletedTask;
        }
    }
}