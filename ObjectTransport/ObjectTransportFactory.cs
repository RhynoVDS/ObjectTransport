using OTransport.Implementation;
using System;
using System.Collections.Generic;
using System.Text;

namespace OTransport
{
    public class ObjectTransportFactory
    { 

        public ObjectTransport CreateTCPServer(string ipAddress,int port)
        {
            TCPServerChannel server = new TCPServerChannel(ipAddress, port);
            return new ObjectTransport(server);
        }
        public ObjectTransport CreateTCPClient(string ipAddress,int port)
        {
            TCPClientChannel client = new TCPClientChannel(ipAddress, port);
            return new ObjectTransport(client);
        }
    }
}
