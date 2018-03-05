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

            channel1.Setup(c => c.OnReceive(It.IsAny<Action<ReceivedMessage>>())).Callback<Action<ReceivedMessage>>(
               function => channel1ReceiveFunction = function
                );

            channel2.Setup(c => c.Send(It.IsAny<Client>(), It.IsAny<string>())).Callback<Client, string>((c, p) =>
                 {
                     ReceivedMessage message = new ReceivedMessage(channel2Client, p);
                     channel1ReceiveFunction.Invoke(message);
                 });

            channel2.Setup(c => c.OnReceive(It.IsAny<Action<ReceivedMessage>>())).Callback<Action<ReceivedMessage>>(
               function => channel2ReceiveFunction = function
                );

            channel2.Setup(c => c.OnClientConnect(It.IsAny<Action<Client>>())).Callback<Action<Client>>(a =>
            {
                channel2OnClient = a;
                channel2OnClient.Invoke(channel1Client);
            });
            
            channel1.Setup(c => c.OnClientConnect(It.IsAny<Action<Client>>())).Callback<Action<Client>>(a =>
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

        public void Stop()
        {
            
        }

        public void SetReliable()
        {
        }

        public void SetUnreliable()
        {
        }

        public void OnClientConnect(Action<Client> callBack)
        {
            ObjectTransportClientConnect = callBack;
        }

        public void OnReceive(Action<ReceivedMessage> callBack)
        {
            ObjectTransportReceive = callBack;
        }

        public void OnClientDisconnect(Action<Client> callBack)
        {
        }

        public void Send(Client client, string payload)
        {
            if(MockSendFunction!=null)
                MockSendFunction.Invoke(client, payload);
        }

        public void DisconnectClient(params Client[] client)
        {
            throw new NotImplementedException();
        }

        public void Start(string ipaddress, int port)
        {
            throw new NotImplementedException();
        }
    }
}
