using System;

namespace odysseyAnalytics.Core.Application.Session
{
    public interface ISession<T>
    {
            DateTime StartTime { get; set; }
            DateTime EndTime { get; set; }
            string Device { get; set; }
            string Platform { get; set; }
            double Duration { get; }
            T ExtraData { get; set; }
    }
}