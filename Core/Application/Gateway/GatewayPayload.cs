using System.Collections.Generic;

namespace odysseyAnalytics.Core.Application.Gateway
{
    public class GatewayPayload
    {
        public string Endpoint { get; set; }
        public Dictionary<string, object> Data { get; set; } = new Dictionary<string, object>();
        public string AccessToken { get; set; } = string.Empty;
    }
}