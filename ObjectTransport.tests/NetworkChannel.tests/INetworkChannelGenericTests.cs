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
        protected INetworkChannel Client1 { get; set; }
        protected INetworkChannel Client2 { get; set; }
        protected INetworkChannel Server { get; set; }
        protected List<Client> ServerOnConnectClients { get; set; }
        protected List<Client> ServerOnDisconnectClients { get; set; }
        protected List<Client> Client1OnConnectClients { get; set; }
        protected List<Client> Client1OnDisconnectClients { get; set; }
        protected IObjectTransport Client1ObjectTransport { get; set; }
        protected IObjectTransport serverObjectTransport { get; set; }

        protected void SetUpNetworkChannels(INetworkChannel client,INetworkChannel client2,INetworkChannel server)
        {
            Client1 = client;
            Client2 = client2;
            Server = server;

            ServerOnConnectClients = new List<Client>();
            ServerOnDisconnectClients = new List<Client>();
            Client1OnConnectClients = new List<Client>();
            Client1OnDisconnectClients = new List<Client>();

            serverObjectTransport = TestObjectTransportFactory.CreateNewObjectTransport(Server);
            serverObjectTransport.OnClientConnect(c => ServerOnConnectClients.Add(c));
            serverObjectTransport.OnClientDisconnect(c => ServerOnDisconnectClients.Add(c));
            serverObjectTransport.Start("127.0.0.1", 0);

            Utilities.WaitFor(()=> server.LocalPort !=0);

            Client1ObjectTransport = TestObjectTransportFactory.CreateNewObjectTransport(Client1);
            Client1ObjectTransport.OnClientConnect(c => Client1OnConnectClients.Add(c));
            Client1ObjectTransport.OnClientDisconnect(c => Client1OnDisconnectClients.Add(c));
            Client1ObjectTransport.Start("127.0.0.1", Server.LocalPort);

            //Wait for the connection to have been made
            Utilities.WaitFor(()=> serverObjectTransport.GetConnectedClients().Count() == 1);
            Utilities.WaitFor(()=> Client1ObjectTransport.GetConnectedClients().Count() == 1);
        }
        [TestMethod]
        public void ClientDisconnects_CallbackCalled()
        {
            //Arrange
            Client client = null;
            Client clientDisconnect = null;

            client = Client1ObjectTransport.GetConnectedClients().First();

            //Act
            serverObjectTransport.Stop();

            Utilities.WaitFor(()=> Client1OnDisconnectClients.Count() > 0);
            clientDisconnect = Client1OnDisconnectClients.First();

            //Assert
            Assert.AreEqual(client.IPAddress, "127.0.0.1");
            Assert.AreEqual(clientDisconnect.IPAddress, "127.0.0.1");
            Assert.AreEqual(clientDisconnect,client);
            Utilities.WaitFor(()=>Client1ObjectTransport.GetConnectedClients().Count() == 0);
            Utilities.WaitFor(()=>serverObjectTransport.GetConnectedClients().Count() == 0);
        }
        [TestMethod]
        public void Client_StartExecuted_ServerIsAddedAsClient()
        {
            //Arrange
            Client FirstClient = null;

            //Act
            FirstClient = Client1ObjectTransport.GetConnectedClients().First();


            //Assert
            Assert.AreEqual(FirstClient.IPAddress, "127.0.0.1");
            Assert.AreEqual(FirstClient.Port, Server.LocalPort);
        }

        [TestMethod]
        public void SendAndReplyMessage_ResponseIsCalled()
        {
            //Arrange
            Client client = null;
            client = Client1ObjectTransport.GetConnectedClients().First();

            //Act
            serverObjectTransport.Receive<MockObjectMessage>()
                                 .Reply((o) => { return o; })
                                 .Execute();

            var mockObject = new MockObjectMessage() { Property1_string = "Mock Object" };
            MockObjectMessage responseObject = null;

            Client1ObjectTransport.Send(mockObject)
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
            //Create a second client
            ObjectTransport clientObjectTransport2 = TestObjectTransportFactory.CreateNewObjectTransport(Client2);
            clientObjectTransport2.Start("127.0.0.1", Server.LocalPort);

            //Wait for the connection to have been made
            Utilities.WaitFor(() => serverObjectTransport.GetConnectedClients().Count() == 2);

            //Act

            //disconnect the server from the client
            Client1ObjectTransport.DisconnectClient();

            Utilities.WaitFor(()=> ServerOnDisconnectClients.Count ==1);

            //Assert
            //Ensure that the client record was disconnect from the server
            Assert.AreEqual(1,serverObjectTransport.GetConnectedClients().Count());

            //Esnure that the client who disconnected from the server was the one that we called disconect
            Assert.AreEqual(ServerOnDisconnectClients.First().Port, Client1.LocalPort);
        }

        [TestMethod]
        public void Server_ClientDisconnects_CallbackCalled()
        {
            //Arrange

            //Act

            Client1ObjectTransport.Stop();
            Utilities.WaitFor(()=> ServerOnDisconnectClients.Count() == 1);
            
            //Assert
            var clientConnect = ServerOnConnectClients.First();
            var clientDisconnect = ServerOnDisconnectClients.First();
            Assert.AreEqual(clientConnect.IPAddress, "127.0.0.1");
            Assert.AreEqual(clientDisconnect.IPAddress, "127.0.0.1");
            Assert.AreEqual(clientDisconnect,clientConnect);
            Assert.AreEqual(0,Client1ObjectTransport.GetConnectedClients().Count());
            Assert.AreEqual(0,serverObjectTransport.GetConnectedClients().Count());
        }

        [TestMethod]
        public void ServerWith2Clients_ServerDisconnects1Client_1ClientDisconnected()
        {
            //Arrange
            ObjectTransport clientObjectTransport2 = TestObjectTransportFactory.CreateNewObjectTransport(Client2);
            clientObjectTransport2.Start("127.0.0.1", Server.LocalPort);

            //Wait for the connection to have been made
            Utilities.WaitFor(() => serverObjectTransport.GetConnectedClients().Count() == 2);
            var FirstClient = serverObjectTransport.GetConnectedClients().First();

            //Act

            serverObjectTransport.DisconnectClient(FirstClient);
            Utilities.WaitFor(()=> ServerOnDisconnectClients.Count() ==1);

            //Assert

            Client LastClient = serverObjectTransport.GetConnectedClients().First();
            Assert.AreEqual(1, serverObjectTransport.GetConnectedClients().Count());
            Assert.AreNotEqual(FirstClient.Port, LastClient.Port);
        }
        [TestMethod]
        public void ServerWith2Clients_ServerDisconnects2Client_AllClientsDisconnected()
        {
            //Arrange

            //Create a second client
            ObjectTransport clientObjectTransport2 = TestObjectTransportFactory.CreateNewObjectTransport(Client2);
            clientObjectTransport2.Start("127.0.0.1", Server.LocalPort);

            //Wait for the connection to have been made
            Utilities.WaitFor(() => serverObjectTransport.GetConnectedClients().Count() == 2);

            //Act

            var allClients = serverObjectTransport.GetConnectedClients().ToArray();
            serverObjectTransport.DisconnectClient(allClients);

            Utilities.WaitFor(() => ServerOnDisconnectClients.Count == 2);

            //Assert
            Assert.AreEqual(0, serverObjectTransport.GetConnectedClients().Count());
            Assert.AreEqual(2, ServerOnDisconnectClients.Count());
        }

        [TestMethod]
        public void Server_ReceivesObjects_CorrectObjectReceived()
        {
            //Arrange
            MockObjectMessage receivedObject = null;

            //Act
            serverObjectTransport.Receive<MockObjectMessage>(o =>
            {
                receivedObject = o;

            }).Execute();

            Client1ObjectTransport.Send(new MockObjectMessage()
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

            Client client = serverObjectTransport.GetConnectedClients().First();

            //Act
            Client1ObjectTransport.Receive<MockObjectMessage>(o =>
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
            Assert.AreEqual(serverObjectTransport.GetConnectedClients().Count(), 1);
        }

        [TestMethod]
        public void Server_ServerDisconnects2Client_AllClientsDisconnected()
        {
            //Arrange

            //Create a second client
            ObjectTransport clientObjectTransport2 = TestObjectTransportFactory.CreateNewObjectTransport(Client2);
            clientObjectTransport2.Start("127.0.0.1", Server.LocalPort);

            //Wait for the connection to have been made
            Utilities.WaitFor(() => serverObjectTransport.GetConnectedClients().Count() == 2);

            //Act

            var allClients = serverObjectTransport.GetConnectedClients().ToArray();
            serverObjectTransport.DisconnectClient(allClients);

            Utilities.WaitFor(() => ServerOnDisconnectClients.Count == 2);

            //Assert
            Assert.AreEqual(0, serverObjectTransport.GetConnectedClients().Count());
            Assert.AreEqual(2, ServerOnDisconnectClients.Count());
        }

        [TestCleanup]
        public void ShutDownNetworkChannels()
        {
            Server.Stop();
            Client1.Stop();
            Client2.Stop();
        }

    }
}
