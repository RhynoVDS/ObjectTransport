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
    public class UDPNetworkChannel_Server
    {
        UDPServerChannel udpServer = new UDPServerChannel();
        UDPClientChannel udpClient = new UDPClientChannel();
        UDPClientChannel udpClient2 = new UDPClientChannel();

        [TestCleanup]
        public void CleanUpServer()
        {
            if (udpServer != null)
                udpServer.Stop();
            if (udpClient != null)
                udpClient.Stop();
            if (udpClient2 != null)
                udpClient2.Stop();

            UDPObjectTransportChannel.TearDown();
        }
        [TestMethod]
        public void UDPServer_WhenClientConnects_CallbackFunctionCalled()
        {
            bool connected = false;
            udpServer.Start("127.0.0.1", 0);
            udpServer.OnClientConnect(c => connected = true);

            udpClient.Start("127.0.0.1", udpServer.LocalPort);

            Utilities.WaitFor(ref connected);
            Assert.IsTrue(connected);
        }
        [TestMethod]
        public void UDPServer_ReceivesObjects_CorrectObjectReceived()
        {
            //Arrange
            MockObjectMessage receivedObject = null;
            var connectTransports = UDPObjectTransportChannel.GetConnectObjectTransports();
            var server = connectTransports.Item1;
            var client = connectTransports.Item2;

            //Act
            server.Receive<MockObjectMessage>(o =>
            {
                receivedObject = o;

            }).Execute();

            client.Send(new MockObjectMessage()
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
        public void UDPServer_SendObject_CorrectObjectSent()
        {
            //Arrange
            MockObjectMessage receivedObject = null;
            udpServer.Start("127.0.0.1", 0);

            Client client = null;

            ObjectTransport serverObjectTransport = TestObjectTransportFactory.CreateNewObjectTransport(udpServer);
            serverObjectTransport.OnClientConnect(c => client = c);

            udpClient.Start("127.0.0.1", udpServer.LocalPort);
            ObjectTransport clientObjectTransport = TestObjectTransportFactory.CreateNewObjectTransport(udpClient);

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
        public void UDPServerWith2Clients_Disconnect1Client_1ClientDisconnected()
        {
            //Arrange
            Client disconnectedClient = null;

            udpServer.Start("127.0.0.1", 0);
            ObjectTransport serverObjectTransport = TestObjectTransportFactory.CreateNewObjectTransport(udpServer);
            serverObjectTransport.OnClientDisconnect(c => disconnectedClient = c);

            udpClient.Start("127.0.0.1", udpServer.LocalPort);
            ObjectTransport clientObjectTransport = TestObjectTransportFactory.CreateNewObjectTransport(udpClient);

            udpClient2 = new UDPClientChannel();
            udpClient2.Start("127.0.0.1", udpServer.LocalPort);
            ObjectTransport clientObjectTransport2 = TestObjectTransportFactory.CreateNewObjectTransport(udpClient2);

            Utilities.WaitFor(() => serverObjectTransport.GetConnectedClients().Count() == 2);

            //Act

            var FirstClient = serverObjectTransport.GetConnectedClients().First();
            serverObjectTransport.DisconnectClient(FirstClient);

            Utilities.WaitFor(ref disconnectedClient);

            //Assert
            Client LastClient = serverObjectTransport.GetConnectedClients().First();

            Assert.AreEqual(1,serverObjectTransport.GetConnectedClients().Count());
            Assert.AreNotEqual(FirstClient.Port,LastClient.Port);
        }

        [TestMethod]
        public void UDPServerWith2Clients_Disconnect2Client_AllClientsDisconnected()
        {
            //Arrange
            List<Client> disconnectedClients = new List<Client>();

            udpServer.Start("127.0.0.1", 0);
            ObjectTransport serverObjectTransport = TestObjectTransportFactory.CreateNewObjectTransport(udpServer);
            serverObjectTransport.OnClientDisconnect(c => disconnectedClients.Add(c));

            udpClient.Start("127.0.0.1", udpServer.LocalPort);
            ObjectTransport clientObjectTransport = TestObjectTransportFactory.CreateNewObjectTransport(udpClient);

            udpClient2.Start("127.0.0.1", udpServer.LocalPort);
            ObjectTransport clientObjectTransport2 = TestObjectTransportFactory.CreateNewObjectTransport(udpClient2);

            Utilities.WaitFor(() => serverObjectTransport.GetConnectedClients().Count() == 2);

            //Act

            var allClients = serverObjectTransport.GetConnectedClients().ToArray();
            serverObjectTransport.DisconnectClient(allClients);

            Utilities.WaitFor(()=> disconnectedClients.Count == 2);

            //Assert
            Assert.AreEqual(0,serverObjectTransport.GetConnectedClients().Count());
            Assert.AreEqual(2, disconnectedClients.Count());
        }


        [TestMethod]
        public void UDPServer_ClientDisconnects_CallbackCalled()
        {
            //Arrange
            Client clientConnect = null;
            Client clientDisconnect = null;

            udpServer.Start("127.0.0.1", 0);
            ObjectTransport serverObjectTransport = TestObjectTransportFactory.CreateNewObjectTransport(udpServer);
            serverObjectTransport.OnClientConnect(c => clientConnect = c);
            serverObjectTransport.OnClientDisconnect(c => clientDisconnect = c);

            udpClient.Start("127.0.0.1", udpServer.LocalPort);
            ObjectTransport clientObjectTransport = TestObjectTransportFactory.CreateNewObjectTransport(udpClient);

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
    }
}
