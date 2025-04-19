#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE
using UnityEngine;
#endif
using System;
using odysseyAnalytics.Types;

namespace odysseyAnalytics.Logging
{
    public class DefaultLogger : ILogger
    {
        public void Log(string message)
        {
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE
            Debug.Log("[OdysseyAnalytics] " + message);
#else
            Console.WriteLine("[OdysseyAnalytics] " + message);
#endif
        }

        public void Warn(string message)
        {
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE
            Debug.LogWarning("[OdysseyAnalytics] " + message);
#else
            Console.WriteLine("[OdysseyAnalytics][WARN] " + message);
#endif
        }

        public void Error(string message, Exception ex = null)
        {
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE
            Debug.LogError($"[OdysseyAnalytics] {message}\n{ex?.ToString()}");
#else
            Console.Error.WriteLine($"[OdysseyAnalytics][ERROR] {message}");
            if (ex != null)
                Console.Error.WriteLine(ex);
#endif
        }
    }
}