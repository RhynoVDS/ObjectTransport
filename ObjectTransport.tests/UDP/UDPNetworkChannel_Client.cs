using Microsoft.VisualStudio.TestTools.UnitTesting;
using OTransport;
using OTransport.Implementation;
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
        TCPServerChannel server = null;
        TCPClientChannel tcpclient = null;

        [TestCleanup]
        public void CleanUpServer()
        {
            if (server != null)
                server.Stop();
            if (tcpclient != null)
                tcpclient.Stop();
        }
        [TestMethod]
        public void TCPClient_ClientDisconnects_CallbackCalled()
        {
            //Arrange
            Client client = null;
            Client clientDisconnect = null;

            server = new TCPServerChannel("127.0.0.1", 0);
            ObjectTransport serverObjectTransport = new ObjectTransport(server);

            tcpclient = new TCPClientChannel("127.0.0.1", server.Port);
            ObjectTransport clientObjectTransport = new ObjectTransport(tcpclient);
            clientObjectTransport.OnClientDisconnect(c => clientDisconnect = c);
            client = clientObjectTransport.GetConnecectedClients().First();

            Utilities.WaitFor(ref client);
            Utilities.WaitFor(()=> serverObjectTransport.GetConnecectedClients().Count() == 1);
            //Act

            serverObjectTransport.Stop();

            Utilities.WaitFor(ref clientDisconnect);
            //Assert
            Assert.AreEqual(client.IPAddress, "127.0.0.1");
            Assert.AreEqual(clientDisconnect.IPAddress, "127.0.0.1");
            Assert.AreEqual(clientDisconnect,client);
            Utilities.WaitFor(()=>clientObjectTransport.GetConnecectedClients().Count() == 0);
            Utilities.WaitFor(()=>serverObjectTransport.GetConnecectedClients().Count() == 0);
        }
    }
}
