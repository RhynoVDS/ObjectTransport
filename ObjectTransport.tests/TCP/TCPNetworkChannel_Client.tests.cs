using Microsoft.VisualStudio.TestTools.UnitTesting;
using OTransport.NetworkChannel.TCP;
using OTransport;
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
    public class TCPNetworkChannel_Reply
    {
        TCPServerChannel tcpserver = null;
        TCPClientChannel tcpclient = null;
        TCPClientChannel tcpclient2 = null;

        [TestCleanup]
        public void CleanUpServer()
        {
            if (tcpserver != null)
                tcpserver.Stop();
            if (tcpclient != null)
                tcpclient.Stop();
            if (tcpclient2 != null)
                tcpclient2.Stop();
        }
        [TestMethod]
        public void TCPNetwork_SendAndReplyMessage_ResponseIsCalled()
        {
            //Arrange
            Client client = null;
            Client clientDisconnect = null;

            tcpserver = new TCPServerChannel("127.0.0.1", 0);
            ObjectTransport serverObjectTransport = TestObjectTransportFactory.CreateNewObjectTransport(tcpserver);

            tcpclient = new TCPClientChannel("127.0.0.1", tcpserver.LocalPort);
            ObjectTransport clientObjectTransport = TestObjectTransportFactory.CreateNewObjectTransport(tcpclient);
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
        public void TCPClient_ClientDisconnectsServer_ServerOnClientDisconnectCalled()
        {
            //Arrange
            Client disconnectedClient = null;
            Client connectedServer = null;

            tcpserver = new TCPServerChannel("127.0.0.1", 0);
            ObjectTransport serverObjectTransport = TestObjectTransportFactory.CreateNewObjectTransport(tcpserver);
            serverObjectTransport.OnClientDisconnect(c => disconnectedClient = c);

            tcpclient = new TCPClientChannel("127.0.0.1", tcpserver.LocalPort);
            ObjectTransport clientObjectTransport = TestObjectTransportFactory.CreateNewObjectTransport(tcpclient);
            clientObjectTransport.OnClientConnect(c => connectedServer = c);

            tcpclient2 = new TCPClientChannel("127.0.0.1", tcpserver.LocalPort);
            ObjectTransport clientObjectTransport2 = TestObjectTransportFactory.CreateNewObjectTransport(tcpclient2);

            Utilities.WaitFor(() => serverObjectTransport.GetConnectedClients().Count() == 2);

            //Act

            //disconnect the server from the client
            clientObjectTransport.DisconnectClient();

            Utilities.WaitFor(ref disconnectedClient);

            //Assert
            //Ensure that the client record was disconnect from the server
            Assert.AreEqual(1,serverObjectTransport.GetConnectedClients().Count());

            //Esnure that the client who disconnected from the server was the one that we called disconect
            Assert.AreEqual(disconnectedClient.Port, tcpclient.LocalPort);
        }

    }
}
