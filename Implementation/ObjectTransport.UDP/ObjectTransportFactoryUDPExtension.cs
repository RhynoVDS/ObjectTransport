using OTransport;
using OTransport.Factory;
using OTransport.Implementation;
using System;
using System.Collections.Generic;
using System.Text;

namespace OT.UDP
{
    public static class ObjectTransportFactoryUDPExtension
    { 
        /// <summary>
        /// Create a UDP server. This supports reliable and unreliable communications. Defaults to unreliable.
        /// </summary>
        /// <param name="ipAddress">the IP address to start the server on</param>
        /// <param name="port">the port to listen on</param>
        /// <returns></returns>
        public static ObjectTransportAssemblyLine CreateUDPServer(this ObjectTransportFactory o, string ipAddress,int port)
        {
            UDPServerChannel server = new UDPServerChannel(ipAddress, port,32);
            var assemblyLine = new ObjectTransportAssemblyLine(server);

            return assemblyLine;
        }

        /// <summary>
        /// Create a UDP client. This supports reliable and unreliable communications. Defaults to unreliable.
        /// </summary>
        /// <param name="ipAddress">the IP address to start the server on</param>
        /// <param name="port">the port to listen on</param>
        /// <returns></returns>
        public static ObjectTransportAssemblyLine CreateUDPClient(this ObjectTransportFactory o,string ipAddress,int port)
        {
            var client = new UDPClientChannel(ipAddress, port);
            var assemblyLine = new ObjectTransportAssemblyLine(client);

            return assemblyLine;
        }
    }
}
