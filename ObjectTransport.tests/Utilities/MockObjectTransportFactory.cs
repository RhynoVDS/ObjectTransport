using OT.Serializer.JSON;
using OTransport;
using System;
using System.Collections.Generic;
using System.Text;

namespace OTransport.Test.Utilities
{
    public static class TestObjectTransportFactory
    {
        public static ObjectTransport CreateNewObjectTransport(INetworkChannel channel)
        {
            var jsonSerializer = new JSONserializer();

            return new ObjectTransport(channel, jsonSerializer);

        }
    }
}
