using System.Collections.Generic;
using System.Threading.Tasks;
using odysseyAnalytics.Core.Application.Gateway;

namespace odysseyAnalytics.Core.Ports
{
    public interface IGatewayPort
    {
        Task<bool> SendAsync(GatewayPayload payload); // POST
        Task<IGatewayPortResponse> FetchAsync(GatewayPayload payload); // GET
    }

    public class IGatewayPortResponse
    {
        public IGatewayPortResponse(string statusCode, string data)
        {
            this.StatusCode = statusCode;
            this.Data = data;
        }

        public IGatewayPortResponse()
        {
            
        }
        public string StatusCode { get; set; }
        public string Data { get; set; } 
        
    }
    
    
    
    
    
    
    
}