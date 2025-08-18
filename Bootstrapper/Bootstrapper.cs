using System;
using System.IO;
using System.Threading.Tasks;
using odysseyAnalytics.Core.Ports;
using odysseyAnalytics.Core.Application.Session;
using odysseyAnalytics.Adapters.Logger;
using odysseyAnalytics.Adapters.RabbitMQ;
using odysseyAnalytics.Adapters.REST;
using odysseyAnalytics.Adapters.Sqlite;

#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE
                        using UnityEngine;
#endif

namespace Bootstrapper
{
    public class Bootstrapper
    {
        private IGatewayPort gatewayPort;
        private IMessagePublisherPort messagePublisher;
        private IConnectablePublisher connection;
        private ILogger logger;
        private IDatabasePort databasePort;
        private SessionHandler session;


        public Bootstrapper()
        {
            logger = new DefaultLogger();
            messagePublisher = new RabbitMQAdapter();
            gatewayPort = new RESTAdapter("https://odysseyanalytics.ir/api/");
            connection = messagePublisher as IConnectablePublisher;


#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE
                        databasePort = new SqliteAdapter(Path.Combine(Application.persistentDataPath, "events.db"));
#else
            databasePort = new SqliteAdapter("./events.db");
#endif
        }

        public Bootstrapper SetGatewayPort(IGatewayPort port)
        {
            gatewayPort = port;
            return this;
        }

        public Bootstrapper SetMessagePublisher(IMessagePublisherPort port)
        {
            messagePublisher = port;
            return this;
        }

        public Bootstrapper SetIConnectablePublisher(IConnectablePublisher connection)
        {
            this.connection = connection;
            return this;
        }

        public Bootstrapper SetDatabasePort(IDatabasePort port)
        {
            this.databasePort = port;
            return this;
        }

        public Bootstrapper SetLogger(ILogger logger)
        {
            this.logger = logger;
            return this;
        }

        public async Task<SessionHandler> InitializeSession(string token)
        {
            session = new SessionHandler(gatewayPort, messagePublisher, connection, logger, databasePort, token);
            await session.InitializeSessionAsync();
            return session;
        }
    }
}