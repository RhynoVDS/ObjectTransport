using OTransport;
using OTransport.Factory;
using System;
using System.Collections.Generic;
using System.Text;

namespace OTransport.NetworkChannel.UDP
{
    public static class ObjectTransportFactoryUDPExtension
    { 
        /// <summary>
        /// Create a UDP server. This supports reliable and unreliable communications. Defaults to unreliable.
        /// </summary>
        /// <param name="ipAddress">the IP address to start the server on</param>
        /// <param name="port">the port to listen on</param>
        /// <returns></returns>
        public static ObjectTransportAssemblyLine CreateUDPServer(this ObjectTransportFactory o)
        {
            UDPServerChannel server = new UDPServerChannel();
            var assemblyLine = new ObjectTransportAssemblyLine();
            assemblyLine.SetNetworkChannel(server);
            assemblyLine.SetUnreliableTransport();

            return assemblyLine;
        }

        /// <summary>
        /// Create a UDP client. This supports reliable and unreliable communications. Defaults to unreliable.
        /// </summary>
        /// <param name="ipAddress">the IP address to start the server on</param>
        /// <param name="port">the port to listen on</param>
        /// <returns></returns>
        public static ObjectTransportAssemblyLine CreateUDPClient(this ObjectTransportFactory o)
        {
            var client = new UDPClientChannel();
            var assemblyLine = new ObjectTransportAssemblyLine();
            assemblyLine.SetNetworkChannel(client);
            assemblyLine.SetUnreliableTransport();

            return assemblyLine;
        }
    }
}
