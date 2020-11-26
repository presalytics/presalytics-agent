using System;

namespace shared.Exceptions
{
    public class PresalyticsHubConnectionException : Exception

    {
        public PresalyticsHubConnectionException()
        {
            
        }
        public PresalyticsHubConnectionException(string message) : base(message)
        {
            
        }

        public PresalyticsHubConnectionException(string message, Exception inner) : base(message, inner)
        {
            
        }
    }
}