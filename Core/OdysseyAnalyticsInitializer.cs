using odysseyAnalytics.Adapters.Logger;
using odysseyAnalytics.Adapters.RabbitMQ;
using odysseyAnalytics.Adapters.REST;
using odysseyAnalytics.Adapters.Sqlite;
using odysseyAnalytics.Core.Application.CacheManager;
using odysseyAnalytics.Core.Application.Session;
using odysseyAnalytics.Core.Ports;

namespace odysseyAnalytics.Core
{
    public class OdysseyAnalyticsInitializerBuilder
    {
        private OdysseyAnalyticsInitializer _analyticsInitializer;
        private IConnectablePublisher _publisher;
        private IDatabasePort _databasePort;
        private IMessagePublisherPort _messagePublisherPort;
        private IGatewayPort _gatewayPort;
        private ILogger _logger;
        private string token;
        
        public OdysseyAnalyticsInitializerBuilder(string db_path , string token)
        {
            _databasePort = new SqliteAdapter(db_path);
            RabbitMQAdapter temp= new RabbitMQAdapter();
            _publisher = temp;
            _messagePublisherPort = temp;
            _gatewayPort = new RESTAdapter("https://odysseyanalytics.ir/api/");
            _logger = new DefaultLogger();
            
            this.token=token;
        }

        public OdysseyAnalyticsInitializerBuilder SetDatabasePort(IDatabasePort databasePort)
        {
            _databasePort = databasePort;
            return this;
        }

        public OdysseyAnalyticsInitializer Build()
        {
            _analyticsInitializer =
                new OdysseyAnalyticsInitializer(_databasePort, _messagePublisherPort, _publisher ,_gatewayPort, _logger, token);
            return _analyticsInitializer;
        }
    }
    public class OdysseyAnalyticsInitializer
    {
        public OdysseyAnalyticsInitializer(IDatabasePort databasePort, IMessagePublisherPort messagePublisherPort,IConnectablePublisher connectablePublisher, IGatewayPort gatewayPort, ILogger logger ,string token)
        {
            var sessionHandler = new SessionHandler(gatewayPort,messagePublisherPort,connectablePublisher,logger,token);
            var eventCacheManager = new CacheManager(databasePort);
        }
    }
}