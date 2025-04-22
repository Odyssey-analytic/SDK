using System.Threading.Tasks;

namespace odysseyAnalytics.Core.Ports
{
    public interface IConnectablePublisher
    {
        Task ConnectAsync(string host, string username, string password, int port, string vhost = null);
        Task CloseAsync();
    }
}