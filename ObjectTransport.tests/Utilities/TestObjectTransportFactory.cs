using OTransport.Serializer.JSON;
using OTransport;
using System;
using System.Collections.Generic;
using System.Text;
using OTransport.Serializer;

namespace OTransport.Test.Utilities
{
    public static class TestObjectTransportFactory
    {
        public static ObjectTransport CreateNewObjectTransport(INetworkChannel channel)
        {
            var jsonSerializer = new JSONserializer();

            return new ObjectTransport(channel, jsonSerializer);
        }
        public static ObjectTransport CreateNewObjectTransport(INetworkChannel channel,ISerializer serializer)
        {
            return new ObjectTransport(channel, serializer);
        }
    }
}
