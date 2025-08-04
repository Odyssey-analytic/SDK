using System;
using odysseyAnalytics.Core.Ports;

namespace odysseyAnalytics.Adapters.Bootstrap
{
    public class Bootstrap
    {
        IDatabasePort databasePort;
        IMessagePublisherPort messagePublisherPort;
        IGatewayPort gatewayPort;
        ILogger logger;
        
        
    }
}