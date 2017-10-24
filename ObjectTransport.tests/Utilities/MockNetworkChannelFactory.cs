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


            channel1.Setup(c => c.SendReliable(It.IsAny<Client>(), It.IsAny<string>())).Callback<Client, string>((c, p) =>
                 {
                     ReceivedMessage message = new ReceivedMessage(channel1Client, p);
                     channel2ReceiveFunction.Invoke(message);
                 });

            channel1.Setup(c => c.SendUnreliable(It.IsAny<Client>(), It.IsAny<string>())).Callback<Client, string>((c, p) =>
                 {
                     ReceivedMessage message = new ReceivedMessage(channel1Client, p);
                     channel2ReceiveFunction.Invoke(message);
                 });

            channel1.Setup(c => c.Receive(It.IsAny<Action<ReceivedMessage>>())).Callback<Action<ReceivedMessage>>(
               function => channel1ReceiveFunction = function
                );

            channel2.Setup(c => c.SendReliable(It.IsAny<Client>(), It.IsAny<string>())).Callback<Client, string>((c, p) =>
                 {
                     ReceivedMessage message = new ReceivedMessage(channel2Client, p);
                     channel1ReceiveFunction.Invoke(message);
                 });
            channel2.Setup(c => c.SendUnreliable(It.IsAny<Client>(), It.IsAny<string>())).Callback<Client, string>((c, p) =>
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

        private Action<ReceivedMessage> ObjectTransportReceive = null;
        private Action<Client> ObjectTransportClientConnect = null;


        public MockedNetworkChannel OnSendHandle(Action<Client,string> sendFunction)
        {
            MockSendFunction = sendFunction;
            return this;
        }

        public void SendUnreliable(Client client, string message)
        {
            if(MockSendFunction!=null)
                MockSendFunction.Invoke(client, message);
        }

        public void SimulateClientConnect(Client client)
        {
            ObjectTransportClientConnect.Invoke(client);
        }

        public void SimulateClientResponse(Client client,string message)
        {
            ReceivedMessage receivedMessage = new ReceivedMessage(client,message);
            ObjectTransportReceive.Invoke(receivedMessage);
        }

        void INetworkChannel.CheckReceiveClient(Action<Client> callBack)
        {
            ObjectTransportClientConnect = callBack;
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

        public void SendReliable(Client client, string message)
        {
            if (MockSendFunction != null)
                MockSendFunction.Invoke(client, message); ;
        }
    }
}
