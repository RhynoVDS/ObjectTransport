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
        public void TCPNetwork_SendAndReplyMessage_ResponseIsCalled()
        {
            //Arrange
            Client client = null;
            Client clientDisconnect = null;

            server = new TCPServerChannel("127.0.0.1", 0);
            ObjectTransport serverObjectTransport = TestObjectTransportFactory.CreateNewObjectTransport(server);

            tcpclient = new TCPClientChannel("127.0.0.1", server.Port);
            ObjectTransport clientObjectTransport = TestObjectTransportFactory.CreateNewObjectTransport(tcpclient);
            clientObjectTransport.OnClientDisconnect(c => clientDisconnect = c);
            client = clientObjectTransport.GetConnecectedClients().First();

            Utilities.WaitFor(ref client);
            Utilities.WaitFor(()=> serverObjectTransport.GetConnecectedClients().Count() == 1);

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
    }
}
