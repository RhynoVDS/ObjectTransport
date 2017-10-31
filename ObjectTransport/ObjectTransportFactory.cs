using OTransport.Implementation;
using System;
using System.Collections.Generic;
using System.Text;

namespace OTransport
{
    public class ObjectTransportFactory
    { 

        /// <summary>
        /// Create a TCP server. This network channel only supports reliable communication.
        /// </summary>
        /// <param name="ipAddress">the IP address to start the server on</param>
        /// <param name="port">the port to listen on</param>
        /// <returns></returns>
        public ObjectTransport CreateTCPServer(string ipAddress,int port)
        {
            TCPServerChannel server = new TCPServerChannel(ipAddress, port);
            return new ObjectTransport(server);
        }

        /// <summary>
        /// Create a TCP client. This network channel only supports reliable communication.
        /// </summary>
        /// <param name="ipAddress">the IP address to start the server on</param>
        /// <param name="port">the port to listen on</param>
        /// <returns></returns>
        public ObjectTransport CreateTCPClient(string ipAddress,int port)
        {
            TCPClientChannel client = new TCPClientChannel(ipAddress, port);
            return new ObjectTransport(client);
        }

        /// <summary>
        /// Create a UDP server. This supports reliable and unreliable communications. Defaults to unreliable.
        /// </summary>
        /// <param name="ipAddress">the IP address to start the server on</param>
        /// <param name="port">the port to listen on</param>
        /// <returns></returns>
        public ObjectTransport CreateUDPServer(string ipAddress,int port)
        {
            UDPServerChannel server = new UDPServerChannel(ipAddress, port,32);
            var serverOT = new ObjectTransport(server);
            serverOT.SetUnreliable();

            return serverOT;
        }

        /// <summary>
        /// Create a UDP client. This supports reliable and unreliable communications. Defaults to unreliable.
        /// </summary>
        /// <param name="ipAddress">the IP address to start the server on</param>
        /// <param name="port">the port to listen on</param>
        /// <returns></returns>
        public ObjectTransport CreateUDPClient(string ipAddress,int port)
        {
            var client = new UDPClientChannel(ipAddress, port);
            var clientObjectTransort = new ObjectTransport(client);
            clientObjectTransort.SetUnreliable();

            return clientObjectTransort;
        }
    }
}
