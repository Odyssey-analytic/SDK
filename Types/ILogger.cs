using System;

namespace odysseyAnalytics.Types
{
    public interface ILogger
    {
        void Log(string message);
        void Warn(string message);
        void Error(string message, Exception ex = null);
    }
}