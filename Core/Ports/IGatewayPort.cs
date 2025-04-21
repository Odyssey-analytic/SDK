using System.Threading.Tasks;

namespace odysseyAnalytics.Core.Ports
{
    public interface IGatewayPort
    {
        Task Update();
        Task<T> Get<T>(string id);
    }
}