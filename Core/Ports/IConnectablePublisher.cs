using System.Threading.Tasks;

namespace odysseyAnalytics.Core.Ports
{
    public interface IConnectablePublisher
    {
        Task ConnectAsync(string host, string username, string password, string vhost = null,int port=5672);
        Task CloseAsync();
    }
}