﻿using OTransport;
using OTransport.Implementation;
using System;
using System.Threading;

namespace Test
{
    class UDPObjectTransportChannel
    {
        private static UDPServerChannel server;
        private static UDPClientChannel udpclient;
        public static Tuple<ObjectTransport,ObjectTransport> GetConnectObjectTransports()
        {
            server = new UDPServerChannel("127.0.0.1", 0,32);

            ObjectTransport serverObjectTransport = new ObjectTransport(server);

            udpclient = new UDPClientChannel("127.0.0.1", server.Port);
            ObjectTransport client = new ObjectTransport(udpclient);

            Tuple<ObjectTransport, ObjectTransport> result = new Tuple<ObjectTransport, ObjectTransport>(serverObjectTransport, client);
            return result;
        }
        public static void TearDown()
        {
            if (server != null)
                server.Stop();
            if (udpclient != null)
                udpclient.Stop();
        }
    }
}
