using System;
using System.Collections.Generic;
using System.Text;

namespace OTransport
{
    public interface INetworkChannel
    {
        void Stop();
        void SendUnreliable(Client client, string message);
        void CheckReceiveClient(Action<Client> callBack);
        void Receive(Action<ReceivedMessage> callBack);
        void ClientDisconnect(Action<Client> callBack);
        void SendReliable(Client client, string jsonPayload);
    }
}
