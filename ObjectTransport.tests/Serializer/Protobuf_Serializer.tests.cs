using Microsoft.VisualStudio.TestTools.UnitTesting;
using OTransport;
using OTransport.Serializer.protobuf;
using OTransport.Test.Utilities;
using OTransport.tests;
using System;
using System.Collections.Generic;
using System.Text;
using Test;

namespace OTransport.Test.Serializer
{
    [TestClass]
    public class Protobuf_Serializer
    {
        [TestMethod]
        public void Protobuff_UsingProtobuffSerializer_objectIsSentAndReceived()
        {
            //Arrange
            var joinedNetworkChannels = MockNetworkChannelFactory.GetConnectedChannels();

            Protobuf_MockObjectMessage sendMessage = new Protobuf_MockObjectMessage();
            sendMessage.Property1_string = "test send";
            sendMessage.Property2_int = 123;
            sendMessage.Property3_decimal = 56M;

            Protobuf_MockObjectMessage receivedMessage = null;
            Client receivedClient = null;

            //Act 
            ObjectTransport client1 = TestObjectTransportFactory.CreateNewObjectTransport(joinedNetworkChannels.Item1,new ProtobufSerializer());
            client1.Receive<Protobuf_MockObjectMessage>((c, o) =>
            {
                receivedClient = c;
                receivedMessage = o;
            })
                     .Execute();


            ObjectTransport client2 = TestObjectTransportFactory.CreateNewObjectTransport(joinedNetworkChannels.Item2,new ProtobufSerializer());
            client2.Send(sendMessage)
                   .Execute();

            //Assert
            Assert.AreEqual(sendMessage.Property3_decimal, receivedMessage.Property3_decimal);
            Assert.AreEqual(sendMessage.Property1_string, receivedMessage.Property1_string);
            Assert.AreEqual(sendMessage.Property2_int, receivedMessage.Property2_int);
            Assert.AreEqual("10.0.0.2", receivedClient.IPAddress);
        }
    }
}
