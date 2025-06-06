using System;

namespace odysseyAnalytics.Core.Application.Exceptions
{
    /// <summary>
    /// Base exception for all OdysseyAnalytics specific exceptions
    /// </summary>
    public class OdysseyAnalyticsException : Exception
    {
        public OdysseyAnalyticsException() : base()
        {
        }

        public OdysseyAnalyticsException(string message) : base(message)
        {
        }

        public OdysseyAnalyticsException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Thrown when the client cannot connect to the analytics server
    /// </summary>
    public class NotConnectedToServerException : OdysseyAnalyticsException
    {
        public NotConnectedToServerException() : base("Could not connect to the analytics server")
        {
        }

        public NotConnectedToServerException(string message) : base(message)
        {
        }

        public NotConnectedToServerException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Thrown when there's no internet connection available
    /// </summary>
    public class NoInternetConnectionException : OdysseyAnalyticsException
    {
        public NoInternetConnectionException() : base("No internet connection available")
        {
        }

        public NoInternetConnectionException(string message) : base(message)
        {
        }

        public NoInternetConnectionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Thrown when authentication or token validation fails
    /// </summary>
    public class AuthenticationException : OdysseyAnalyticsException
    {
        public AuthenticationException() : base("Authentication failed")
        {
        }

        public AuthenticationException(string message) : base(message)
        {
        }

        public AuthenticationException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Thrown when session details are invalid or not properly initialized
    /// </summary>
    public class InvalidSessionException : OdysseyAnalyticsException
    {
        public InvalidSessionException() : base("Invalid or uninitialized session")
        {
        }

        public InvalidSessionException(string message) : base(message)
        {
        }

        public InvalidSessionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    /// <summary>
    /// Thrown when a required queue is missing 
    /// </summary>
    public class QueueNotFoundException : OdysseyAnalyticsException
    {
        public string QueueName { get; }

        public QueueNotFoundException(string queueName)
            : base($"Queue '{queueName}' not found")
        {
            QueueName = queueName;
        }

        public QueueNotFoundException(string queueName, string message)
            : base(message)
        {
            QueueName = queueName;
        }

        public QueueNotFoundException(string queueName, string message, Exception innerException)
            : base(message, innerException)
        {
            QueueName = queueName;
        }
    }

    /// <summary>
    /// Thrown when progression event depth exceeds MAX_DEPTH
    /// </summary>
    public class DepthLimitExceeded : OdysseyAnalyticsException
    {
        public int Depth { get; }
        public int MaxDepth { get; }

        private const int DefaultMaxDepth = 5;

        public DepthLimitExceeded()
            : this(DefaultMaxDepth + 1, DefaultMaxDepth)
        {
        }

        public DepthLimitExceeded(string message)
            : base(message)
        {
        }

        public DepthLimitExceeded(int depth, int maxDepth)
            : base($"Depth Limit Exceeded: {depth}. MAX_DEPTH = {maxDepth}")
        {
            Depth = depth;
            MaxDepth = maxDepth;
        }

        public DepthLimitExceeded(int depth, string message, Exception innerException)
            : base(message, innerException)
        {
            Depth = depth;
        }
    }

}