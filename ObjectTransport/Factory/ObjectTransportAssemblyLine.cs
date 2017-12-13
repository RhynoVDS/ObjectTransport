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
            if (Serializer == null)
                throw new ObjectTransportException("Please specify a Serializer to use. If you haven't done so, please install a serializer from nuget or implement your own.");

            var objectTransport = new ObjectTransport(NetworkChannel, Serializer);
            return objectTransport;
        }

    }
}
