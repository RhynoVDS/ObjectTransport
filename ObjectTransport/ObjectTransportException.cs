using System;

namespace OTransport
{
    [Serializable]
    public class ObjectTransportException : Exception
    {
        public ObjectTransportException()
        {
        }

        public ObjectTransportException(string message) : base(message)
        {
        }

        public ObjectTransportException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}