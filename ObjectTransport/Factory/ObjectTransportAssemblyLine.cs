using OTransport.Serializer;
using System;
using System.Collections.Generic;
using System.Text;

namespace OTransport.Factory
{
    public class ObjectTransportAssemblyLine
    {
        private INetworkChannel NetworkChannel;
        private ISerializer Serializer;

        public ObjectTransportAssemblyLine(INetworkChannel networkChannel)
        {
            NetworkChannel = networkChannel;
        }

        /// <summary>
        /// Specify a custom serializer to be used when serilizing objects.
        /// </summary>
        /// <param name="serializer"></param>
        /// <returns></returns>
        public ObjectTransportAssemblyLine SetSerializer(ISerializer serializer)
        {
            Serializer = serializer;
            return this;
        }

        public ObjectTransport Build()
        {
            var objectTransport = new ObjectTransport(NetworkChannel, Serializer);
            return objectTransport;
        }

    }
}
