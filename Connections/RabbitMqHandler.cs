﻿using System;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using System.Net.Http;


namespace odysseyAnalytics.Connections
{
    public class RabbitMqHandler
    {
        private IConnection _connection;
        private IChannel _channel; // ✅ Ensure this is correctly declared

        public async Task Connect(string host, string username, string password,string vHost="/", int port = 5672)
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
            _channel = await _connection.CreateChannelAsync(); // ✅ Use _channel instead of undeclared 'channel'

            Console.WriteLine("Connected to RabbitMQ");
        }

        public async Task PublishMessage(string queueName, string message)
        {
            var body = Encoding.UTF8.GetBytes(message);
            await _channel.BasicPublishAsync(exchange:"",routingKey:queueName,body: body);
            Console.WriteLine($"Sent: {message}");
        }
        // public async Task PublishMessagee(string queueName, string message)
        // {
        //     HttpClient client = new HttpClient();
        //     HttpResponseMessage credentials = HttpClient.SendRequestAsync("api/token/", HttpMethod.Get);
        //
        // }

        public async Task Close()
        {
            await _channel?.CloseAsync();
            await _connection?.CloseAsync();
            _channel?.DisposeAsync(); 
            _connection?.DisposeAsync();
            
        }
    }
}