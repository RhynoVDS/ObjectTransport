using System;
using System.Collections.Generic;
using System.Text;

namespace OTransport
{
    public interface INetworkChannel
    {
        int LocalPort { get; }
        void Stop();
        void SetReliable();
        void SetUnreliable();
        void OnClientConnect(Action<Client> callBack);
        void OnReceive(Action<ReceivedMessage> callBack);
        void OnClientDisconnect(Action<Client> callBack);
        void Send(Client client, string payload);
        void DisconnectClient(params Client[] client);
        void Start(string ipaddress, int port);
    }
}
