using odysseyAnalytics.Adapters.Logger;
using odysseyAnalytics.Adapters.Sqllite;
using odysseyAnalytics.Connections;
using odysseyAnalytics.Core.Application.Session;
using odysseyAnalytics.Core.Ports;
using odysseyAnalytics.Storage;
using odysseyAnalytics.Types;

namespace odysseyAnalytics.Core
{
    public class OdysseyAnalyticsInitializerBuilder
    {
        private OdysseyAnalyticsInitializer _analyticsInitializer;
        private IDatabasePort _databasePort;
        private IMessagePublisherPort _messagePublisherPort;
        private IGatewayPort _gatewayPort;
        private ILogger _logger;
        
        public OdysseyAnalyticsInitializerBuilder()
        {
            _databasePort = new SqliteAdapter();
            _messagePublisherPort = new RabbitMqHandler(null);
            _gatewayPort = null;
            _logger = new DefaultLogger();
        }

        public OdysseyAnalyticsInitializerBuilder SetDatabasePort(IDatabasePort databasePort)
        {
            _databasePort = databasePort;
            return this;
        }

        public OdysseyAnalyticsInitializer Build()
        {
            _analyticsInitializer =
                new OdysseyAnalyticsInitializer(_databasePort, _messagePublisherPort, _gatewayPort, _logger);
            return _analyticsInitializer;
        }
    }
    public class OdysseyAnalyticsInitializer
    {
        public OdysseyAnalyticsInitializer(IDatabasePort databasePort, IMessagePublisherPort messagePublisherPort, IGatewayPort gatewayPort, ILogger logger)
        {
            var sessionHandler = new SessionHandler(gatewayPort, messagePublisherPort);
            var eventCacheManager = new EventCacheManager(databasePort);
        }
    }
}