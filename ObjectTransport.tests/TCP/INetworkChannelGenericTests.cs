using Microsoft.VisualStudio.TestTools.UnitTesting;
using OTransport.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Test;

namespace OTransport
{
    [TestClass]
    public abstract class INetworkChannelGenericTests 
    {
        private INetworkChannel Client { get; set; }
        private INetworkChannel Server { get; set; }
        protected void SetUpNetworkChannels(INetworkChannel client,INetworkChannel server)
        {
            Client = client;
            Server = server;
        }
        [TestMethod]
        public void ClientDisconnects_CallbackCalled()
        {
            //Arrange
            Client client = null;
            Client clientDisconnect = null;

            ObjectTransport serverObjectTransport = TestObjectTransportFactory.CreateNewObjectTransport(Server);
            serverObjectTransport.Start("127.0.0.1",0);

            ObjectTransport clientObjectTransport = TestObjectTransportFactory.CreateNewObjectTransport(Client);
            clientObjectTransport.Start("127.0.0.1",Server.LocalPort);

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
        public void Client_StartIsCalled_ServerIsAddedAsClient()
        {
            //Arrange
            Client FirstClient = null;

            ObjectTransport serverObjectTransport = TestObjectTransportFactory.CreateNewObjectTransport(Server);
            serverObjectTransport.Start("127.0.0.1", 0);
            ObjectTransport clientObjectTransport = TestObjectTransportFactory.CreateNewObjectTransportTCPclient();

            //Act

            clientObjectTransport.Start("127.0.0.1", Server.LocalPort);
            //When the start method is called, there should be clients
            FirstClient = clientObjectTransport.GetConnectedClients().First();

            Utilities.WaitFor(ref FirstClient);
            Utilities.WaitFor(()=> serverObjectTransport.GetConnectedClients().Count() == 1);

            //Assert
            Assert.AreEqual(FirstClient.IPAddress, "127.0.0.1");
            Assert.AreEqual(FirstClient.Port, Server.LocalPort);
        }

    }
}
