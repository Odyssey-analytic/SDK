using System.Threading.Tasks;

namespace odysseyAnalytics.Core.Ports
{
    public interface IMessagePublisherPort
    {
        Task<T> PublishMessage<T>(string message);
    }
}