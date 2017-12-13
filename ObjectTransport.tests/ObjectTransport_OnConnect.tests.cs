using Microsoft.VisualStudio.TestTools.UnitTesting;
using OTransport.Test.Utilities;
using System;
using System.Collections.Generic;
using System.Text;
using Test;

namespace OTransport.tests
{
    [TestClass]
    public class ObjectTransport_OnConnect
    {
        [TestMethod]
        public void OnClientConnect_ClientConnectsToObject_ClientInjectedIntoAction()
        {
            //Arrange
            var sentJson = string.Empty;
            var client = new Client("10.0.0.1",123);
            var networkChannel = MockNetworkChannelFactory.GetMockedNetworkChannel();

            Client connectedClient = null;

            //Act 
            ObjectTransport transport = TestObjectTransportFactory.CreateNewObjectTransport(networkChannel);
            transport.OnClientConnect(c =>
            {
                connectedClient = c;
            });

            networkChannel.SimulateClientConnect(client);
            //Assert
            Assert.AreEqual(client, connectedClient);
        }

    }
}
