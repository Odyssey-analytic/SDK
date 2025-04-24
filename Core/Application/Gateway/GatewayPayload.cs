using System.Collections.Generic;

namespace odysseyAnalytics.Core.Application.Gateway
{
    public class GatewayPayload
    {
        public GatewayPayload(string endpoint,Dictionary<string,string> data,string accessToken)
        {
            this.Endpoint = endpoint;
            this.Data = data;
            this.AccessToken = accessToken;
        }
        public string Endpoint { get; set; }
        public Dictionary<string, string> Data { get; set; }
        public string AccessToken { get; set; } = string.Empty;
    }
}