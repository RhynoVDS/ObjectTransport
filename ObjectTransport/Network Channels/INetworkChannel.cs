using System;
using System.Collections.Generic;
using System.Text;

namespace OTransport
{
    public interface INetworkChannel
    {
        void Stop();
        void SetReliable();
        void SetUnreliable();
        void OnClientConnect(Action<Client> callBack);
        void OnReceive(Action<ReceivedMessage> callBack);
        void OnClientDisconnect(Action<Client> callBack);
        void Send(Client client, string payload);
    }
}
