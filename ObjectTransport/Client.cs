using System;
using System.Collections.Generic;
using System.Text;

namespace OTransport
{
    public class Client
    {
        internal Client()
        {

        }
        public Client(string ipaddress,int port)
        {
            IPAddress = ipaddress;
            Port = port;
        }

        public string IPAddress { get; internal set; }
        public int Port { get; internal set; }
    }
}
