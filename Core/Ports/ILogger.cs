using System;

namespace odysseyAnalytics.Core.Ports
{
    public interface ILogger
    {
        void Log(string message);
        void Warn(string message);
        void Error(string message, Exception ex = null);
    }
}