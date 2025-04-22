using System.Collections.Generic;
using System.Threading.Tasks;
using odysseyAnalytics.Core.Application.Gateway;

namespace odysseyAnalytics.Core.Ports
{
    public interface IGatewayPort
    {
        Task<bool> SendAsync(GatewayPayload payload); // POST
        Task<Dictionary<string, object>> FetchAsync(GatewayPayload payload); // GET
    }
}