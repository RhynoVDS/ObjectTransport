using Microsoft.VisualStudio.TestTools.UnitTesting;
using OTransport;
using OTransport.NetworkChannel.UDP;
using OTransport.Test.Utilities;
using OTransport.tests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Test
{
    [TestClass]
    public class UDPNetworkChannel_Client
    {
        UDPServerChannel udpServer = null;
        UDPClientChannel udpClient = null;
        UDPClientChannel udpClient2 = null;

        [TestCleanup]
        public void CleanUpServer()
        {
            if (udpServer != null)
                udpServer.Stop();
            if (udpClient != null)
                udpClient.Stop();
            if (udpClient2 != null)
                udpClient2.Stop();
        }
        [TestMethod]
        public void UDPClient_ServerDisconnects_ClientDisconnectCallbackCalled()
        {
            //Arrange
            Client client = null;
            Client clientDisconnect = null;

            udpServer = new UDPServerChannel("127.0.0.1", 0,32);
            ObjectTransport serverObjectTransport = TestObjectTransportFactory.CreateNewObjectTransport(udpServer);

            udpClient = new UDPClientChannel("127.0.0.1", udpServer.LocalPort);
            ObjectTransport clientObjectTransport = TestObjectTransportFactory.CreateNewObjectTransport(udpClient);

            clientObjectTransport.OnClientDisconnect(c => clientDisconnect = c);
            client = clientObjectTransport.GetConnectedClients().First();

            Utilities.WaitFor(ref client);
            Utilities.WaitFor(()=> serverObjectTransport.GetConnectedClients().Count() == 1);
            //Act

            serverObjectTransport.Stop();

            Utilities.WaitFor(ref clientDisconnect);

            //Assert
            Assert.AreEqual(client.IPAddress, "127.0.0.1");
            Assert.AreEqual(clientDisconnect.IPAddress, "127.0.0.1");
            Assert.AreEqual(clientDisconnect,client);
            Utilities.WaitFor(()=>clientObjectTransport.GetConnectedClients().Count() == 0);
            Utilities.WaitFor(()=>serverObjectTransport.GetConnectedClients().Count() == 0);
        }
        [TestMethod]
        public void UDPClient_ClientDisconnectsServer_ServerOnClientDisconnectCalled()
        {
            //Arrange
            Client disconnectedClient = null;
            Client connectedServer = null;

            udpServer = new UDPServerChannel("127.0.0.1", 0,32);
            ObjectTransport serverObjectTransport = TestObjectTransportFactory.CreateNewObjectTransport(udpServer);
            serverObjectTransport.OnClientDisconnect(c => disconnectedClient = c);

            udpClient = new UDPClientChannel("127.0.0.1", udpServer.LocalPort);
            ObjectTransport clientObjectTransport = TestObjectTransportFactory.CreateNewObjectTransport(udpClient);
            clientObjectTransport.OnClientConnect(c => connectedServer = c);

            udpClient2 = new UDPClientChannel("127.0.0.1", udpServer.LocalPort);
            ObjectTransport clientObjectTransport2 = TestObjectTransportFactory.CreateNewObjectTransport(udpClient2);

            Utilities.WaitFor(() => serverObjectTransport.GetConnectedClients().Count() == 2);

            //Act

            //disconnect the server from the client
            clientObjectTransport.DisconnectClient();

            Utilities.WaitFor(ref disconnectedClient);

            //Assert
            //Ensure that the client record was disconnect from the server
            Assert.AreEqual(1,serverObjectTransport.GetConnectedClients().Count());

            //Esnure that the client who disconnected from the server was the one that we called disconect
            Assert.AreEqual(disconnectedClient.Port, udpClient.LocalPort);
        }

    }
}
