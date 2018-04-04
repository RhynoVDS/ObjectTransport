using Microsoft.VisualStudio.TestTools.UnitTesting;
using OTransport.Test.Utilities;
using OTransport.tests;
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
        private INetworkChannel Client1 { get; set; }
        private INetworkChannel Client2 { get; set; }
        private INetworkChannel Server { get; set; }
        protected void SetUpNetworkChannels(INetworkChannel client,INetworkChannel server)
        {
            Client1 = client;
            Server = server;
        }
        protected void SetUpNetworkChannels(INetworkChannel client,INetworkChannel client2,INetworkChannel server)
        {
            Client1 = client;
            Client2 = client2;
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

            ObjectTransport clientObjectTransport = TestObjectTransportFactory.CreateNewObjectTransport(Client1);
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
        public void Client_StartExecuted_ServerIsAddedAsClient()
        {
            //Arrange
            Client FirstClient = null;

            ObjectTransport serverObjectTransport = TestObjectTransportFactory.CreateNewObjectTransport(Server);
            serverObjectTransport.Start("127.0.0.1", 0);
            ObjectTransport clientObjectTransport = TestObjectTransportFactory.CreateNewObjectTransport(Client1);

            //Act

            clientObjectTransport.Start("127.0.0.1", Server.LocalPort);
            //When the start method is called, there should be clients

            Utilities.WaitFor(()=> clientObjectTransport.GetConnectedClients().Count() == 1);
            FirstClient = clientObjectTransport.GetConnectedClients().First();

            Utilities.WaitFor(()=> serverObjectTransport.GetConnectedClients().Count() == 1);

            //Assert
            Assert.AreEqual(FirstClient.IPAddress, "127.0.0.1");
            Assert.AreEqual(FirstClient.Port, Server.LocalPort);
        }

        [TestMethod]
        public void SendAndReplyMessage_ResponseIsCalled()
        {
            //Arrange
            Client client = null;
            Client clientDisconnect = null;

            
            ObjectTransport serverObjectTransport = TestObjectTransportFactory.CreateNewObjectTransport(Server);
            serverObjectTransport.Start("127.0.0.1", 0);

            ObjectTransport clientObjectTransport = TestObjectTransportFactory.CreateNewObjectTransport(Client1);
            clientObjectTransport.Start("127.0.0.1", Server.LocalPort);

            clientObjectTransport.OnClientDisconnect(c => clientDisconnect = c);
            client = clientObjectTransport.GetConnectedClients().First();

            Utilities.WaitFor(ref client);
            Utilities.WaitFor(()=> serverObjectTransport.GetConnectedClients().Count() == 1);

            //Act
            serverObjectTransport.Receive<MockObjectMessage>()
                                 .Reply((o) => { return o; })
                                 .Execute();


            var mockObject = new MockObjectMessage() { Property1_string = "Mock Object" };
            MockObjectMessage responseObject = null;

            clientObjectTransport.Send(mockObject)
                .Response<MockObjectMessage>((r) => {responseObject = r;})
                .Execute();

            Utilities.WaitFor(ref responseObject);

            //Assert
            Assert.AreEqual(responseObject.Property1_string, "Mock Object");
        }

        [TestMethod]
        public void ClientDisconnectsServer_ServerOnClientDisconnectCalled()
        {
            //Arrange
            Client disconnectedClient = null;
            Client connectedServer = null;

            ObjectTransport serverObjectTransport = TestObjectTransportFactory.CreateNewObjectTransport(Server);
            serverObjectTransport.Start("127.0.0.1", 0);
            serverObjectTransport.OnClientDisconnect(c => disconnectedClient = c);

            ObjectTransport clientObjectTransport = TestObjectTransportFactory.CreateNewObjectTransport(Client1);
            clientObjectTransport.Start("127.0.0.1", Server.LocalPort);
            clientObjectTransport.OnClientConnect(c => connectedServer = c);

            ObjectTransport clientObjectTransport2 = TestObjectTransportFactory.CreateNewObjectTransport(Client2);
            clientObjectTransport2.Start("127.0.0.1", Server.LocalPort);

            Utilities.WaitFor(() => serverObjectTransport.GetConnectedClients().Count() == 2);

            //Act

            //disconnect the server from the client
            clientObjectTransport.DisconnectClient();

            Utilities.WaitFor(ref disconnectedClient);

            //Assert
            //Ensure that the client record was disconnect from the server
            Assert.AreEqual(1,serverObjectTransport.GetConnectedClients().Count());

            //Esnure that the client who disconnected from the server was the one that we called disconect
            Assert.AreEqual(disconnectedClient.Port, Client1.LocalPort);
        }

        [TestMethod]
        public void Server_ClientDisconnects_CallbackCalled()
        {
            //Arrange
            Client clientConnect = null;
            Client clientDisconnect = null;

            ObjectTransport serverObjectTransport = TestObjectTransportFactory.CreateNewObjectTransport(Server);
            serverObjectTransport.Start("127.0.0.1", 0);
            serverObjectTransport.OnClientConnect(c => clientConnect = c);
            serverObjectTransport.OnClientDisconnect(c => clientDisconnect = c);

            ObjectTransport clientObjectTransport = TestObjectTransportFactory.CreateNewObjectTransport(Client1);
            clientObjectTransport.Start("127.0.0.1", Server.LocalPort);

            Utilities.WaitFor(ref clientConnect);
            Utilities.WaitFor(() => clientObjectTransport.GetConnectedClients().Count() ==1);

            //Act

            clientObjectTransport.Stop();

            Utilities.WaitFor(ref clientDisconnect);
            Utilities.Wait();
            //Assert
            Assert.AreEqual(clientConnect.IPAddress, "127.0.0.1");
            Assert.AreEqual(clientDisconnect.IPAddress, "127.0.0.1");
            Assert.AreEqual(clientDisconnect,clientConnect);
            Assert.AreEqual(0,clientObjectTransport.GetConnectedClients().Count());
            Assert.AreEqual(0,serverObjectTransport.GetConnectedClients().Count());
        }

        [TestMethod]
        public void ServerWith2Clients_ServerDisconnects1Client_1ClientDisconnected()
        {
            //Arrange
            Client disconnectedClient = null;

            ObjectTransport serverObjectTransport = TestObjectTransportFactory.CreateNewObjectTransport(Server);
            serverObjectTransport.Start("127.0.0.1", 0);
            serverObjectTransport.OnClientDisconnect(c => disconnectedClient = c);

            ObjectTransport clientObjectTransport = TestObjectTransportFactory.CreateNewObjectTransport(Client1);
            clientObjectTransport.Start("127.0.0.1", Server.LocalPort);

            ObjectTransport clientObjectTransport2 = TestObjectTransportFactory.CreateNewObjectTransport(Client2);
            clientObjectTransport2.Start("127.0.0.1", Server.LocalPort);

            Utilities.WaitFor(() => serverObjectTransport.GetConnectedClients().Count() == 2);

            //Act

            var FirstClient = serverObjectTransport.GetConnectedClients().First();
            serverObjectTransport.DisconnectClient(FirstClient);

            Utilities.WaitFor(ref disconnectedClient);

            //Assert
            Client LastClient = serverObjectTransport.GetConnectedClients().First();

            Assert.AreEqual(1, serverObjectTransport.GetConnectedClients().Count());
            Assert.AreNotEqual(FirstClient.Port, LastClient.Port);
        }
        [TestMethod]
        public void ServerWith2Clients_ServerDisconnects2Client_AllClientsDisconnected()
        {
            //Arrange
            List<Client> disconnectedClients = new List<Client>();

            ObjectTransport serverObjectTransport = TestObjectTransportFactory.CreateNewObjectTransport(Server);
            serverObjectTransport.Start("127.0.0.1", 0);
            serverObjectTransport.OnClientDisconnect(c => disconnectedClients.Add(c));

            ObjectTransport clientObjectTransport = TestObjectTransportFactory.CreateNewObjectTransport(Client1);
            clientObjectTransport.Start("127.0.0.1", Server.LocalPort);

            ObjectTransport clientObjectTransport2 = TestObjectTransportFactory.CreateNewObjectTransport(Client2);
            Client2.Start("127.0.0.1", Server.LocalPort);

            Utilities.WaitFor(() => serverObjectTransport.GetConnectedClients().Count() == 2);

            //Act

            var allClients = serverObjectTransport.GetConnectedClients().ToArray();
            serverObjectTransport.DisconnectClient(allClients);

            Utilities.WaitFor(() => disconnectedClients.Count == 2);

            //Assert
            Assert.AreEqual(0, serverObjectTransport.GetConnectedClients().Count());
            Assert.AreEqual(2, disconnectedClients.Count());
        }

        [TestMethod]
        public void Server_ReceivesObjects_CorrectObjectReceived()
        {
            //Arrange
            MockObjectMessage receivedObject = null;
            ObjectTransport serverObjectTransport = TestObjectTransportFactory.CreateNewObjectTransport(Server);
            serverObjectTransport.Start("127.0.0.1", 0);

            ObjectTransport clientObjectTransport = TestObjectTransportFactory.CreateNewObjectTransport(Client1);
            clientObjectTransport.Start("127.0.0.1", Server.LocalPort);

            //Act
            serverObjectTransport.Receive<MockObjectMessage>(o =>
            {
                receivedObject = o;

            }).Execute();

            clientObjectTransport.Send(new MockObjectMessage()
            {
                Property1_string = "hello world!",
                Property2_int = 123,
                Property3_decimal = 12.3M
            }).Execute();

            Utilities.WaitFor(ref receivedObject);
            //Assert
            Assert.AreEqual("hello world!", receivedObject.Property1_string);
            Assert.AreEqual(123, receivedObject.Property2_int);
            Assert.AreEqual(12.3M, receivedObject.Property3_decimal);
        }
        
        [TestMethod]
        public void Server_SendObject_CorrectObjectSent()
        {
            //Arrange
            MockObjectMessage receivedObject = null;

            Client client = null;

            ObjectTransport serverObjectTransport = TestObjectTransportFactory.CreateNewObjectTransport(Server);
            serverObjectTransport.Start("127.0.0.1", 0);
            serverObjectTransport.OnClientConnect(c => client = c);

            Utilities.WaitFor(()=> Server.LocalPort != 0);

            ObjectTransport clientObjectTransport = TestObjectTransportFactory.CreateNewObjectTransport(Client1);
            clientObjectTransport.Start("127.0.0.1", Server.LocalPort);

            Utilities.WaitFor(ref client);

            //Act

            clientObjectTransport.Receive<MockObjectMessage>(o =>
            receivedObject = o
            ).Execute();

            serverObjectTransport.Send(new MockObjectMessage()
            {
                Property1_string = "hello world!",
                Property2_int = 123,
                Property3_decimal = 12.3M

            })
            .To(client)
            .Execute();

            Utilities.WaitFor(ref receivedObject);
            //Assert
            Assert.AreEqual("hello world!", receivedObject.Property1_string);
            Assert.AreEqual(123, receivedObject.Property2_int);
            Assert.AreEqual(12.3M, receivedObject.Property3_decimal);
        }


        [TestMethod]
        public void Server_WhenClientConnects_CallbackFunctionCalled()
        {
            bool connected = false;
            ObjectTransport serverObjectTransport = TestObjectTransportFactory.CreateNewObjectTransport(Server);
            serverObjectTransport.Start("127.0.0.1", 0);
            serverObjectTransport.OnClientConnect(c => connected = true);

            ObjectTransport clientObjectTransport = TestObjectTransportFactory.CreateNewObjectTransport(Client1);
            clientObjectTransport.Start("127.0.0.1", Server.LocalPort);

            Utilities.WaitFor(ref connected);
            Assert.IsTrue(connected);
        }

        [TestCleanup]
        public void ShutDownNetworkChannels()
        {
            Client1.Stop();
            Client2.Stop();
            Server.Stop();
        }

    }
}
