using OTransport;
using System;
using System.Collections.Generic;

namespace ObjectTransport.Logging
{
    public class LoggingObjectTransport : IObjectTransport
    {
        private readonly IObjectTransport objectTransport;

        public LoggingObjectTransport(IObjectTransport ObjectTransport)
        {
            objectTransport = ObjectTransport;
        }

        public void DisconnectClient()
        {
            objectTransport.DisconnectClient();
        }

        public void DisconnectClient(params Client[] clients)
        {
            objectTransport.DisconnectClient(clients);
        }

        public IEnumerable<Client> GetConnectedClients()
        {
            return objectTransport.GetConnectedClients();
        }

        public void OnClientConnect(Action<Client> onConnectFunction)
        {
            objectTransport.OnClientConnect(onConnectFunction);
        }

        public void OnClientDisconnect(Action<Client> onDisconnectFunction)
        {
            objectTransport.OnClientDisconnect(onDisconnectFunction);
        }

        public void OnFailedReceive(Action<ReceivedMessage, Exception> onfail)
        {
            objectTransport.OnFailedReceive(onfail);
        }

        public MessageReceive<ReceivedType> Receive<ReceivedType>()
        {
            return objectTransport.Receive<ReceivedType>();
        }

        public MessageReceive<ReceivedType> Receive<ReceivedType>(Action<Client, ReceivedType> obj)
        {
            return objectTransport.Receive(obj);
        }

        public MessageReceive<ReceivedType> Receive<ReceivedType>(Action<ReceivedType> function)
        {
            return objectTransport.Receive(function);
        }

        public MessageSend<SendType> Send<SendType>(SendType obj)
        {
            return objectTransport.Send(obj);
        }

        public void SetReliable()
        {
            objectTransport.SetReliable();
        }

        public void SetUnreliable()
        {
            objectTransport.SetUnreliable();
        }

        public void Stop()
        {
            objectTransport.Stop();
        }
    }
}
