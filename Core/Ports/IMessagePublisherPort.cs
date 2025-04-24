using System.Threading.Tasks;
using odysseyAnalytics.Core.Application.Events;

namespace odysseyAnalytics.Core.Ports
{
    public interface IMessagePublisherPort
    {
        Task<T> PublishMessage<T>(T analyticsEvent) where T : AnalyticsEvent;
    }
}