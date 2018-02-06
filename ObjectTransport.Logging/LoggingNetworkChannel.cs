using OTransport;
using System;

namespace ObjectTransport.Logging
{
    public class LoggingNetworkChannel : INetworkChannel
    {
        private readonly INetworkChannel NetworkChannel;
        public LoggingNetworkChannel(INetworkChannel networkChannel)
        {
            NetworkChannel = networkChannel;
        }
        public void DisconnectClient(params Client[] client)
        {
            NetworkChannel.DisconnectClient(client);
        }

        public void OnClientConnect(Action<Client> callBack)
        {
            NetworkChannel.OnClientConnect(callBack);
        }

        public void OnClientDisconnect(Action<Client> callBack)
        {
            NetworkChannel.OnClientDisconnect(callBack);
        }

        public void OnReceive(Action<ReceivedMessage> callBack)
        {
            NetworkChannel.OnReceive(callBack);
        }

        public void Send(Client client, string payload)
        {
            NetworkChannel.Send(client, payload);
        }

        public void SetReliable()
        {
            NetworkChannel.SetReliable();
        }

        public void SetUnreliable()
        {
            NetworkChannel.SetUnreliable();
        }

        public void Stop()
        {
            NetworkChannel.Stop();
        }
    }
}
