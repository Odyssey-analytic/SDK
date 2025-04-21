using odysseyAnalytics.Connections;
using odysseyAnalytics.Core;

namespace odysseyAnalytics.Test
{
    public class Test
    {
        private Test()
        {
            var odysseyAnalyticsInitializer = new OdysseyAnalyticsInitializerBuilder()
                .SetDatabasePort(null)
                .SetDatabasePort()
                .Build();
        }
    }
}