using System;
using System.Collections.Generic;

namespace odysseyAnalytics.Core.Ports
{
    public interface IDatabasePort
    {
        IEnumerable<T> ReadAll<T>();
        T Read<T>(string key);
        void Write<T>(string key, T value) where T : new();
        void Delete<T>(string key);
        void Update<T>(string key, T value);

        IEnumerable<T> ReadWhere<T>(Func<T, bool> predicate);
    }
}