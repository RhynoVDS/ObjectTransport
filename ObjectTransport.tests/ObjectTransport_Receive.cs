using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;
using Test;
using System.Text;

namespace OTransport.tests
{
    [TestClass]
    public class ObjectTransport_Receive
    {

        [TestMethod]
        public void Receive_ObjectType_ObjectReceiveFunctionExecuted()
        {
            //Arrange

            Client client = new Client("10.0.0.1",123);
            var networkChannel = MockNetworkChannelFactory.GetMockedNetworkChannel();

            MockObjectMessage receive = new MockObjectMessage();

            //Act 
            ObjectTransport transport = new ObjectTransport(networkChannel);
            transport.Receive<MockObjectMessage>(o =>
                        {
                            receive = o;
                        }
                    )
                    .Execute();

            networkChannel.SimulateClientConnect(client);
            networkChannel.SimulateClientResponse(client,
                 "{\"Type\":\"OTransport.tests.MockObjectMessage, ObjectTransport.Test, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\",\"Object\":{\"Property1_string\":\"Test String\",\"Property2_int\":12,\"Property3_decimal\":1.33}}"
                );
            //Assert
            Assert.AreEqual("Test String", receive.Property1_string);
            Assert.AreEqual(12, receive.Property2_int);
            Assert.AreEqual(1.33M, receive.Property3_decimal);
        }
        
        [TestMethod]
        public void Receive_InvalidJson_JsonNotProcessed()
        {
            //Arrange

            Client client = new Client("10.0.0.1",123);
            var networkChannel = MockNetworkChannelFactory.GetMockedNetworkChannel();

            MockObjectMessage receive = null;

            //Act 
            ObjectTransport transport = new ObjectTransport(networkChannel);
            transport.Receive<MockObjectMessage>(o =>
                        {
                            receive = o;
                        }
                    )
                    .Execute();

            networkChannel.SimulateClientConnect(client);
            networkChannel.SimulateClientResponse(client,
                 "{\"Type\":\"OTransport.tests.MockObjectMessage, ObjectTransport.Test, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\",:{\"Property1_string\":\"Test String\",\"Property2_int\":12,\"Property3_decimal\":1.33}}"
                );

            //Assert
            Assert.IsNull(receive);
        }

        [TestMethod]
        [ExpectedException(typeof(ObjectTransportException))]
        public void Receve_RegisterSameTypeTwice_ObjectTransportExceptionThrown()
        {
            //Arrange
            var networkChannel = MockNetworkChannelFactory.GetMockedNetworkChannel();

            //Act 
            ObjectTransport transport = new ObjectTransport(networkChannel);
            transport.Receive<MockObjectMessage>().Execute();
            transport.Receive<MockObjectMessage>().Execute();
        }

        [TestMethod]
        public void Receive_ReplyToReceivedObject_ObjectIsSentBack()
        {
            //Arrange
            string replyJson = null;
            Client client = new Client("10.0.0.1",123);
            var networkChannel = MockNetworkChannelFactory.GetMockedNetworkChannel()
                                                          .OnSendHandle((Client, json) => replyJson = json);


            //Act 
            ObjectTransport transport = new ObjectTransport(networkChannel);
            transport.Receive<MockObjectMessage>()
                    .Reply(o=>
                    {
                        MockObjectMessage sendBack = new MockObjectMessage();
                        sendBack.Property1_string = "Reply message";
                        sendBack.Property2_int = 12;
                        sendBack.Property3_decimal = 1.33M;

                        return sendBack;

                    })
                     .Execute();


            networkChannel.SimulateClientConnect(client);
            networkChannel.SimulateClientResponse(client,
                "{\"Type\":\"OTransport.tests.MockObjectMessage, ObjectTransport.Test, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\",\"Object\":{\"Property1_string\":\"Test String\",\"Property2_int\":12,\"Property3_decimal\":1.33}}"
                );

            //Assert
            Assert.AreEqual("{\"Type\":\"OTransport.tests.MockObjectMessage, ObjectTransport.Test, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null\",\"Object\":{\"Property1_string\":\"Reply message\",\"Property2_int\":12,\"Property3_decimal\":1.33}}", replyJson);
        }

        [TestMethod]
        public void SendReceiveThroughChannel_ObjectTransportConnectedChannel_ObjectSentAndReceivedBetweenClients()
        {
            //Arrange
            var joinedNetworkChannels = MockNetworkChannelFactory.GetConnectedChannels();

            MockObjectMessage sendMessage = new MockObjectMessage();
            sendMessage.Property1_string = "test send";
            sendMessage.Property2_int = 123;
            sendMessage.Property3_decimal = 56M;

            MockObjectMessage receivedMessage = null;

            //Act 
            ObjectTransport client1 = new ObjectTransport(joinedNetworkChannels.Item1);
            client1.Receive<MockObjectMessage>(o=>
                    {
                                    receivedMessage = o;
                    })
                     .Execute();


            ObjectTransport client2 = new ObjectTransport(joinedNetworkChannels.Item2);
            client2.Send(sendMessage)
                   .Execute();

            Utilities.WaitFor(ref receivedMessage);

            //Assert
            Assert.AreEqual(sendMessage.Property3_decimal, receivedMessage.Property3_decimal);
            Assert.AreEqual(sendMessage.Property1_string, receivedMessage.Property1_string);
            Assert.AreEqual(sendMessage.Property2_int, receivedMessage.Property2_int);
        }

        [TestMethod]
        public void Receive_WithClientAction_ObjectAndClientInjectedInAction()
        {
            //Arrange
            var joinedNetworkChannels = MockNetworkChannelFactory.GetConnectedChannels();

            MockObjectMessage sendMessage = new MockObjectMessage();
            sendMessage.Property1_string = "test send";
            sendMessage.Property2_int = 123;
            sendMessage.Property3_decimal = 56M;

            MockObjectMessage receivedMessage = null;
            Client receivedClient = null;

            //Act 
            ObjectTransport client1 = new ObjectTransport(joinedNetworkChannels.Item1);
            client1.Receive<MockObjectMessage>((c,o)=>
                    {
                        receivedClient = c;
                        receivedMessage = o;
                    })
                     .Execute();


            ObjectTransport client2 = new ObjectTransport(joinedNetworkChannels.Item2);
            client2.Send(sendMessage)
                   .Execute();

            //Assert
            Assert.AreEqual(sendMessage.Property3_decimal, receivedMessage.Property3_decimal);
            Assert.AreEqual(sendMessage.Property1_string, receivedMessage.Property1_string);
            Assert.AreEqual(sendMessage.Property2_int, receivedMessage.Property2_int);
            Assert.AreEqual("10.0.0.2", receivedClient.IPAddress);
        }

        [TestMethod]
        public void ReceiveReply_WithClientAction_ObjectAndClientInjectedInToAction()
        {
            //Arrange
            var joinedNetworkChannels = MockNetworkChannelFactory.GetConnectedChannels();

            MockObjectMessage sendMessage = new MockObjectMessage();
            sendMessage.Property1_string = "test send";
            sendMessage.Property2_int = 123;
            sendMessage.Property3_decimal = 56M;

            MockObjectMessage receivedMessage = null;
            Client receivedClient = null;

            //Act 
            ObjectTransport client1 = new ObjectTransport(joinedNetworkChannels.Item1);
            client1.Receive<MockObjectMessage>()
                    .Reply((c,o) =>
                    {
                        receivedMessage = o;
                        receivedClient = c;
                        return null;
                    })
                     .Execute();


            ObjectTransport client2 = new ObjectTransport(joinedNetworkChannels.Item2);
            client2.Send(sendMessage)
                   .Execute();

            Utilities.WaitFor(ref receivedMessage);

            //Assert
            Assert.AreEqual(sendMessage.Property3_decimal, receivedMessage.Property3_decimal);
            Assert.AreEqual(sendMessage.Property1_string, receivedMessage.Property1_string);
            Assert.AreEqual(sendMessage.Property2_int, receivedMessage.Property2_int);
            Assert.AreEqual("10.0.0.2", receivedClient.IPAddress);
        }

        [TestMethod]
        public void SendReceiveReplyThroughChannel_TwoObjectTransportsConnected_SameTokenUsed()
        {
            //Arrange
            var client2ReceiveFunctionCalled = false;
            var client2RespondFunctionCalled = false;
            var joinedNetworkChannels = MockNetworkChannelFactory.GetConnectedChannels();

            MockObjectMessage sendMessage = new MockObjectMessage();
            sendMessage.Property1_string = "test send";
            sendMessage.Property2_int = 123;
            sendMessage.Property3_decimal = 56M;

            MockObjectMessage receivedMessage = null;

            //Act 
            ObjectTransport client1 = new ObjectTransport(joinedNetworkChannels.Item1);
            client1.Receive<MockObjectMessage>()
                    .Reply((c,o) =>
                    {
                        o.Property1_string = "replied";
                        return o;
                    })
                     .Execute();


            ObjectTransport client2 = new ObjectTransport(joinedNetworkChannels.Item2);
            client2.Send(sendMessage)
                    .Response<MockObjectMessage>(o =>
                    {
                        client2RespondFunctionCalled = true;
                        receivedMessage = o;
                    })
                   .Execute();

            client2.Receive<MockObjectMessage>(o =>
            {
                client2ReceiveFunctionCalled = true;
                receivedMessage = o;
            }).Execute();

            Utilities.WaitFor(ref receivedMessage);

            //Assert
            Assert.IsFalse(client2ReceiveFunctionCalled);
            Assert.IsTrue(client2RespondFunctionCalled);
        }
        [TestMethod]
        public void Receive_ObjectWithBinaryProperty_BinaryReceived()
        {
            //Arrange
            var joinedNetworkChannels = MockNetworkChannelFactory.GetConnectedChannels();

            MockObjectMessageWithBinary sendMessage = new MockObjectMessageWithBinary();
            sendMessage.Property2_String = "Message with binary";
            sendMessage.Property1_Bytes = Encoding.ASCII.GetBytes("hello world");

            MockObjectMessageWithBinary receivedMessage = null;
            Client receivedClient = null;

            //Act 
            ObjectTransport client1 = new ObjectTransport(joinedNetworkChannels.Item1);
            client1.Receive<MockObjectMessageWithBinary>((c, o) =>
            {
                receivedClient = c;
                receivedMessage = o;
            })
             .Execute();


            ObjectTransport client2 = new ObjectTransport(joinedNetworkChannels.Item2);
            client2.Send(sendMessage)
                   .Execute();

            //Assert
            Assert.AreEqual("hello world", System.Text.Encoding.UTF8.GetString(receivedMessage.Property1_Bytes));
            Assert.AreEqual(receivedMessage.Property2_String, "Message with binary");
            Assert.AreEqual("10.0.0.2", receivedClient.IPAddress);
        }
    }
}
