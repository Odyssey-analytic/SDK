using System;

namespace odysseyAnalytics.Exceptions
{
    public class NoInternetConnectionException : Exception
    {
        public NoInternetConnectionException()
            : base("No internet connection is available.") { }

        public NoInternetConnectionException(string message)
            : base(message) { }

        public NoInternetConnectionException(string message, Exception inner)
            : base(message, inner) { }
    }
}