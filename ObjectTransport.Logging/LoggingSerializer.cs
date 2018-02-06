using OTransport.Serializer;
using System;
using System.Collections.Generic;
using System.Text;

namespace ObjectTransport.Logging
{
    public class LoggingSerializer : ISerializer
    {
        private readonly ISerializer Serializer;

        public LoggingSerializer(ISerializer serializer)
        {
            Serializer = serializer;
        }
        public object Deserialize(string objectPayload, Type objectType)
        {
            throw new NotImplementedException();
        }

        public string Serialize(object obj)
        {
            throw new NotImplementedException();
        }
    }
}
