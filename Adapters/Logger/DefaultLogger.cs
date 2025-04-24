using System;
using odysseyAnalytics.Core.Ports;

#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE
using UnityEngine;
#else
using System.Diagnostics;
#endif

namespace odysseyAnalytics.Adapters.Logger
{
    public class DefaultLogger : ILogger
    {
        public void Log(string message)
        {
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE
            Debug.Log($"[INFO] {message}");
#else
            Console.WriteLine($"[INFO] {message}");
#endif
        }

        public void Warn(string message)
        {
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE
            Debug.LogWarning($"[WARN] {message}");
#else
            Console.WriteLine($"[WARN] {message}");
#endif
        }

        public void Error(string message, Exception ex = null)
        {
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE
            Debug.LogError($"[ERROR] {message}\n{(ex != null ? ex.ToString() : "")}");
#else
            if (message != null)
            {
                Console.WriteLine($"[ERROR] {message}");
            }
            if (ex != null)
                Console.WriteLine($"[ERROR] {ex.Message}");
#endif
        }
    }
}