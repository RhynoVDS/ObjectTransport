using Microsoft.VisualStudio.TestTools.UnitTesting;
using OTransport;
using OTransport.Implementation;
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
        UDPServerChannel udpServer = null;
        UDPClientChannel udpClient = null;

        [TestCleanup]
        public void CleanUpServer()
        {
            if (udpServer != null)
                udpServer.Stop();
            if (udpClient != null)
                udpClient.Stop();

            UDPObjectTransportChannel.TearDown();
        }
        [TestMethod]
        public void UDPServer_WhenClientConnects_CallbackFunctionCalled()
        {
            bool connected = false;
            udpServer = new UDPServerChannel("127.0.0.1", 0,32);
            udpServer.OnClientConnect(c => connected = true);

            udpClient = new UDPClientChannel("127.0.0.1", udpServer.Port);

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
            udpServer = new UDPServerChannel("127.0.0.1", 0,32);

            Client client = null;

            ObjectTransport serverObjectTransport = TestObjectTransportFactory.CreateNewObjectTransport(udpServer);
            serverObjectTransport.OnClientConnect(c => client = c);

            udpClient = new UDPClientChannel("127.0.0.1", udpServer.Port);
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
        public void UDPServer_ClientDisconnects_CallbackCalled()
        {
            //Arrange
            Client clientConnect = null;
            Client clientDisconnect = null;

            udpServer = new UDPServerChannel("127.0.0.1", 0,32);
            ObjectTransport serverObjectTransport = TestObjectTransportFactory.CreateNewObjectTransport(udpServer);
            serverObjectTransport.OnClientConnect(c => clientConnect = c);
            serverObjectTransport.OnClientDisconnect(c => clientDisconnect = c);

            udpClient = new UDPClientChannel("127.0.0.1", udpServer.Port);
            ObjectTransport clientObjectTransport = TestObjectTransportFactory.CreateNewObjectTransport(udpClient);

            Utilities.WaitFor(ref clientConnect);
            Utilities.WaitFor(() => clientObjectTransport.GetConnecectedClients().Count() ==1);
            //Act

            clientObjectTransport.Stop();

            Utilities.WaitFor(ref clientDisconnect);
            Utilities.Wait();
            //Assert
            Assert.AreEqual(clientConnect.IPAddress, "127.0.0.1");
            Assert.AreEqual(clientDisconnect.IPAddress, "127.0.0.1");
            Assert.AreEqual(clientDisconnect,clientConnect);
            Assert.AreEqual(0,clientObjectTransport.GetConnecectedClients().Count());
            Assert.AreEqual(0,serverObjectTransport.GetConnecectedClients().Count());
        }
    }
}
