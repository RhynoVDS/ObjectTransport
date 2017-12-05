using OT.TCP.Implementation;
using OTransport;
using OTransport.Factory;
using System;
using System.Collections.Generic;
using System.Text;

namespace OT.TCP
{
    public static class ObjectTransportFactoryTCPExtension
    {
        /// <summary>
        /// Create a TCP server. This network channel only supports reliable communication.
        /// </summary>
        /// <param name="ipAddress">the IP address to start the server on</param>
        /// <param name="port">the port to listen on</param>
        /// <returns></returns>
        public static ObjectTransportAssemblyLine CreateTCPServer(this ObjectTransportFactory o,string ipAddress,int port)
        {
            TCPServerChannel server = new TCPServerChannel(ipAddress, port);
            var assemblyLine = new ObjectTransportAssemblyLine(server);

            return assemblyLine;
        }

        /// <summary>
        /// Create a TCP client. This network channel only supports reliable communication.
        /// </summary>
        /// <param name="ipAddress">the IP address to start the server on</param>
        /// <param name="port">the port to listen on</param>
        /// <returns></returns>
        public static ObjectTransportAssemblyLine CreateTCPClient(this ObjectTransportFactory o,string ipAddress,int port)
        {
            TCPClientChannel client = new TCPClientChannel(ipAddress, port);

            var assemblyLine = new ObjectTransportAssemblyLine(client);

            return assemblyLine;
        }

    }
}
