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

    public interface IGatewayPortResponse
    {
        string StatusCode { get; set; }
        string Data { get; set; } 
        
    }
    
    
    
    
    
    
    
}