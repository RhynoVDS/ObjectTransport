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
        void DisconnectClient(params Client[] client);

        /// <summary>
        /// Start the current network channel on the given ipaddress and port. 
        /// This will either connect as a client or start as a server depending on the 
        /// implementation.
        /// </summary>
        void Start(string ipaddress,int port);
    }
}
