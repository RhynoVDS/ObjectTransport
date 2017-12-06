using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;
using Test;
using System.Collections.Generic;
using System.Text;
using OTransport.Test.Utilities;

namespace OTransport.tests
{
    [TestClass]
    public class ObjectTransport_Send
    {
        [TestMethod]
        public void SendExecute_ObjectWithProperties_PayloadWithObjectAndTypeSent()
        {
            //Arrange
            var sentPayload = string.Empty;
            var client = new Client("10.0.0.1",123);
            var networkChannel = MockNetworkChannelFactory.GetMockedNetworkChannel()
                                               .OnSendHandle((Client, payload) => sentPayload = payload);

            MockObjectMessage sendObject = new MockObjectMessage();
            sendObject.Property1_string = "Test String";
            sendObject.Property2_int = 12;
            sendObject.Property3_decimal = 1.33M;

            //Act 
            ObjectTransport transport = TestObjectTransportFactory.CreateNewObjectTransport(networkChannel);
            networkChannel.SimulateClientConnect(client);
            transport.Send(sendObject)
                     .To(client)
                     .Execute();

            //Assert
            Assert.AreEqual(typeof(MockObjectMessage).AssemblyQualifiedName + "::{\"Property1_string\":\"Test String\",\"Property2_int\":12,\"Property3_decimal\":1.33}", sentPayload);
        }
        [TestMethod]
        public void SendWithResponse_ObjectWithPropertiesSet_PayloadWithObjectAndTokenSent()
        {
            //Arrange
            var sentPayload = string.Empty;
            var client = new Client("10.0.0.1",123);
            var networkChannel = MockNetworkChannelFactory.GetMockedNetworkChannel()
                                                     .OnSendHandle((Client, payload) => sentPayload = payload);

            MockObjectMessage sendObject = new MockObjectMessage();
            sendObject.Property1_string = "Test String";
            sendObject.Property2_int = 12;
            sendObject.Property3_decimal = 1.33M;

            ObjectTransport transport = TestObjectTransportFactory.CreateNewObjectTransport(networkChannel);
            networkChannel.SimulateClientConnect(client);

            //Act 

            transport.Send(sendObject)
                     .Response<MockObjectMessage>(o => o.GetType())
                     .Execute();


            Utilities.WaitFor(ref sentPayload);

            //Assert
            Regex rgx = new Regex(typeof(MockObjectMessage).AssemblyQualifiedName + "::(.*)::{\"Property1_string\":\"Test String\",\"Property2_int\":12,\"Property3_decimal\":1.33}");
            Assert.IsTrue(rgx.IsMatch(sentPayload));
        }

        [TestMethod]
        public void SendToAll_AllClients_AllClientsAreSendTo()
        {
            //Arrange
            var client1 = new Client("10.0.0.1", 123);
            var client2 = new Client("10.0.0.2", 123);
            var client3 = new Client("10.0.0.3", 123);

            var clientsSendTo = new List<Client>();

            var networkChannel = MockNetworkChannelFactory.GetMockedNetworkChannel()
                                                     .OnSendHandle((Client, payload) => clientsSendTo.Add(Client));

            MockObjectMessage sendObject = new MockObjectMessage();
            sendObject.Property1_string = "Test String";
            sendObject.Property2_int = 12;
            sendObject.Property3_decimal = 1.33M;

            ObjectTransport transport = TestObjectTransportFactory.CreateNewObjectTransport(networkChannel);

            networkChannel.SimulateClientConnect(client1);
            networkChannel.SimulateClientConnect(client2);
            networkChannel.SimulateClientConnect(client3);

            //Act 

            transport.Send(sendObject)
                     .Response<MockObjectMessage>(o => o.GetType())
                     .ToAll()
                     .Execute();

            //Assert
            Assert.AreEqual(3, clientsSendTo.Count);
            Assert.AreEqual(clientsSendTo[0].IPAddress, "10.0.0.1");
            Assert.AreEqual(clientsSendTo[1].IPAddress, "10.0.0.2");
            Assert.AreEqual(clientsSendTo[2].IPAddress, "10.0.0.3");

        }

        [TestMethod]
        public void SendToAll_Except1Client_AllClientsExcept1AreSendTo()
        {
            //Arrange
            var client1 = new Client("10.0.0.1", 123);
            var client2 = new Client("10.0.0.2", 123);
            var client3 = new Client("10.0.0.3", 123);

            var clientsSendTo = new List<Client>();

            var networkChannel = MockNetworkChannelFactory.GetMockedNetworkChannel()
                                                     .OnSendHandle((Client, paylaod) => clientsSendTo.Add(Client));

            MockObjectMessage sendObject = new MockObjectMessage();
            sendObject.Property1_string = "Test String";
            sendObject.Property2_int = 12;
            sendObject.Property3_decimal = 1.33M;

            ObjectTransport transport = TestObjectTransportFactory.CreateNewObjectTransport(networkChannel);

            networkChannel.SimulateClientConnect(client1);
            networkChannel.SimulateClientConnect(client2);
            networkChannel.SimulateClientConnect(client3);

            //Act 

            transport.Send(sendObject)
                     .Response<MockObjectMessage>(o => o.GetType())
                     .ToAll(client1)
                     .Execute();

            //Assert
            Assert.AreEqual(2, clientsSendTo.Count);
            Assert.AreEqual(clientsSendTo[0].IPAddress, "10.0.0.2");
            Assert.AreEqual(clientsSendTo[1].IPAddress, "10.0.0.3");

        }

        [TestMethod]
        public void SendToAll_Except2Client_AllClientsExcept2AreSendTo()
        {
            //Arrange
            var client1 = new Client("10.0.0.1", 123);
            var client2 = new Client("10.0.0.2", 123);
            var client3 = new Client("10.0.0.3", 123);

            var clientsSendTo = new List<Client>();

            var networkChannel = MockNetworkChannelFactory.GetMockedNetworkChannel()
                                                     .OnSendHandle((Client, paylaod) => clientsSendTo.Add(Client));

            MockObjectMessage sendObject = new MockObjectMessage();
            sendObject.Property1_string = "Test String";
            sendObject.Property2_int = 12;
            sendObject.Property3_decimal = 1.33M;

            ObjectTransport transport = TestObjectTransportFactory.CreateNewObjectTransport(networkChannel);

            networkChannel.SimulateClientConnect(client1);
            networkChannel.SimulateClientConnect(client2);
            networkChannel.SimulateClientConnect(client3);

            //Act 

            transport.Send(sendObject)
                     .Response<MockObjectMessage>(o => o.GetType())
                     .ToAll(client1,client3)
                     .Execute();

            //Assert
            Assert.AreEqual(1, clientsSendTo.Count);
            Assert.AreEqual(clientsSendTo[0].IPAddress, "10.0.0.2");

        }

    }
}
