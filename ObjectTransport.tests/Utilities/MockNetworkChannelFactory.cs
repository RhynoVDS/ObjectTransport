using Moq;
using OTransport;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace Test
{
    class MockNetworkChannelFactory
    {
        public static Tuple<INetworkChannel,INetworkChannel> GetConnectedChannels()
        {
            var channel1 = new Mock<INetworkChannel>();
            var channel2 = new Mock<INetworkChannel>();

            Action<ReceivedMessage> channel1ReceiveFunction = null;
            Action<ReceivedMessage> channel2ReceiveFunction = null;

            var channel1Client = new Client("10.0.0.1", 123);
            var channel2Client = new Client("10.0.0.2", 321);

            Action<Client> channel1OnClient = null;
            Action<Client> channel2OnClient = null;


            channel1.Setup(c => c.Send(It.IsAny<Client>(), It.IsAny<string>())).Callback<Client, string>((c, p) =>
                 {
                     ReceivedMessage message = new ReceivedMessage(channel1Client, p);
                     channel2ReceiveFunction.Invoke(message);
                 });

            channel1.Setup(c => c.Receive(It.IsAny<Action<ReceivedMessage>>())).Callback<Action<ReceivedMessage>>(
               function => channel1ReceiveFunction = function
                );

            channel2.Setup(c => c.Send(It.IsAny<Client>(), It.IsAny<string>())).Callback<Client, string>((c, p) =>
                 {
                     ReceivedMessage message = new ReceivedMessage(channel2Client, p);
                     channel1ReceiveFunction.Invoke(message);
                 });

            channel2.Setup(c => c.Receive(It.IsAny<Action<ReceivedMessage>>())).Callback<Action<ReceivedMessage>>(
               function => channel2ReceiveFunction = function
                );

            channel2.Setup(c => c.CheckReceiveClient(It.IsAny<Action<Client>>())).Callback<Action<Client>>(a =>
            {
                channel2OnClient = a;
                channel2OnClient.Invoke(channel1Client);
            });
            
            channel1.Setup(c => c.CheckReceiveClient(It.IsAny<Action<Client>>())).Callback<Action<Client>>(a =>
            {
                channel1OnClient = a;
                channel1OnClient.Invoke(channel2Client);
            });

            Tuple<INetworkChannel, INetworkChannel> channels = new Tuple<INetworkChannel, INetworkChannel>(channel1.Object,channel2.Object);

            return channels;
        }
        public static MockedNetworkChannel GetMockedNetworkChannel()
        {
            MockedNetworkChannel mock = new MockedNetworkChannel();
            return mock;
        }


    }
    public class MockedNetworkChannel : INetworkChannel
    {
        private Action<Client, string> MockSendFunction = null;
        private Func<Client> MockClientConnectReturn = null;
        private ReceivedMessage MessageToReturn = null;

        private Action<ReceivedMessage> ObjectTransportReceive = null;
        private Action<Client> ObjectTransportClientConnect = null;


        public MockedNetworkChannel SetReceive(ReceivedMessage message)
        {
            MessageToReturn = message;
            return this;
        }
        public MockedNetworkChannel SetSend(Action<Client,string> sendFunction)
        {
            MockSendFunction = sendFunction;
            return this;
        }
        public MockedNetworkChannel SetReceivedClient(Func<Client> receivedClientFunction)
        {
            MockClientConnectReturn = receivedClientFunction;
            return this;
        }

        public ReceivedMessage Receive()
        {
            var returnMessage = MessageToReturn;
            if (returnMessage != null)
                MessageToReturn = null;

            return returnMessage;
        }

        public void Send(Client client, string message)
        {
            if(MockSendFunction!=null)
                MockSendFunction.Invoke(client, message);
        }

        public void SimulateClientConnect()
        {
            ObjectTransportClientConnect.Invoke(MockClientConnectReturn.Invoke());
        }
        public void SimulateReceive()
        {
            ObjectTransportReceive.Invoke(this.MessageToReturn);
        }

        void INetworkChannel.CheckReceiveClient(Action<Client> callBack)
        {
            ObjectTransportClientConnect = callBack;

            if(MockClientConnectReturn !=null)
                ObjectTransportClientConnect.Invoke(MockClientConnectReturn.Invoke());
        }

        void INetworkChannel.Receive(Action<ReceivedMessage> callBack)
        {
            ObjectTransportReceive = callBack;
        }

        public void Stop()
        {
            
        }

        public void ClientDisconnect(Action<Client> callBack)
        {
           
        }
    }
}
