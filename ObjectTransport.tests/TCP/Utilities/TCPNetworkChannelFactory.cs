using OTransport;
using OTransport.Implementation;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Test
{
    class TCPObjectTransportChannel
    {
        private static TCPServerChannel server;
        private static TCPClientChannel tcpclient;
        public static Tuple<ObjectTransport,ObjectTransport> GetConnectObjectTransports()
        {
            server = new TCPServerChannel("127.0.0.1", 0);

            ObjectTransport serverObjectTransport = new ObjectTransport(server);

            tcpclient = new TCPClientChannel("127.0.0.1", server.Port);
            ObjectTransport client = new ObjectTransport(tcpclient);

            Tuple<ObjectTransport, ObjectTransport> result = new Tuple<ObjectTransport, ObjectTransport>(serverObjectTransport, client);
            return result;
        }
        public static void TearDown()
        {
            if (server != null)
                server.Stop();
            if (tcpclient != null)
                tcpclient.Stop();
        }
    }
}
