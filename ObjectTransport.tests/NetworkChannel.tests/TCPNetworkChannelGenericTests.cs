using Microsoft.VisualStudio.TestTools.UnitTesting;
using OTransport;
using OTransport.NetworkChannel.TCP;
using OTransport.Test.Utilities;
using OTransport.tests;
using System;
using System.Linq;
using Test;

namespace OTranport
{
    [TestClass]
    public class TCPNetworkChannelGenericTests : INetworkChannelGenericTests
    {
        [TestInitialize]
        public void SetUpNetworkChannels()
        {
            SetUpNetworkChannels(new TCPClientChannel(), new TCPClientChannel(), new TCPServerChannel());
        }
        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void TCPClient_SendUnreliably_ExceptionThrown()
        {
            //Arrange
            Client client = null;
            Client clientDisconnect = null;

            ObjectTransport serverObjectTransport = TestObjectTransportFactory.CreateNewObjectTransport(Server);
            serverObjectTransport.Start("127.0.0.1", 0);

            ObjectTransport clientObjectTransport = TestObjectTransportFactory.CreateNewObjectTransport(Client1);
            clientObjectTransport.OnClientDisconnect(c => clientDisconnect = c);
            clientObjectTransport.Start("127.0.0.1", Server.LocalPort);

            Utilities.WaitFor(() => serverObjectTransport.GetConnectedClients().Count() == 1);

            client = clientObjectTransport.GetConnectedClients().First();
            Utilities.WaitFor(ref client);

            var message = new MockObjectMessage();
            //Act

            clientObjectTransport.Send(message)
                .Unreliable()
                .Execute();
        }
    }
}
