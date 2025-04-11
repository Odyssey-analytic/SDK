using System;


namespace odysseyAnalytics.Connections
{
    public interface ISession<T>
    {
        
        DateTime StartTime { get; set; }
        DateTime EndTime { get; set; }
        string Device { get; set; }
        string Platform { get; set; }

        /// <summary>
        /// Duration in seconds (or computed as needed).
        /// </summary>
        double Duration { get; }

        /// <summary>
        /// Optional: attach additional data (e.g., metadata, user info) of type T.
        /// </summary>
        T ExtraData { get; set; }
    }
}