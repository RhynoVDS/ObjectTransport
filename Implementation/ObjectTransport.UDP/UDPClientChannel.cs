using LiteNetLib;
using LiteNetLib.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OTransport.Implementation
{
    public class UDPClientChannel : INetworkChannel
    {
        private Dictionary<Client, NetPeer> ClientToNetPeerMap = new Dictionary<Client, NetPeer>();
        private IPAddress IPAddress;

        private EventBasedNetListener listener;
        private NetManager clientUDP;

        public int Port;


        Action<ReceivedMessage> onReceiveCallback = null;
        Action<Client> onConnectCallBack = null;
        Action<Client> onDisconnectCallBack = null;

        public void Stop()
        {
            clientUDP.Stop();

            foreach(Client client in ClientToNetPeerMap.Keys)
            {
                onDisconnectCallBack?.Invoke(client);
            }
        }

        public UDPClientChannel(string ipAddress, int port)
        {
            listener = new EventBasedNetListener();
            clientUDP = new NetManager(listener, "ConnectionKey");
            clientUDP.UnsyncedEvents = true;
            clientUDP.Start();
            clientUDP.Connect(ipAddress, port);

            listener.PeerDisconnectedEvent += (c,i) =>
            {
                Client client = GetClientRecord(c);
                onDisconnectCallBack.Invoke(client);
            };

            listener.PeerConnectedEvent += c =>
            {
                Client client = new Client(c.EndPoint.Host, c.EndPoint.Port);
                ClientToNetPeerMap.Add(client, c);
                onConnectCallBack?.Invoke(client);
            };

            listener.NetworkReceiveEvent += (fromPeer, dataReader) =>
            {
                Client client = GetClientRecord(fromPeer);
                var payload = dataReader.GetString();
                ReceivedMessage receivedMessage = new ReceivedMessage(client, payload);
                onReceiveCallback.Invoke(receivedMessage);
            };

            clientUDP.PollEvents();

            WaitTillConnectionMade();
        }
        private void WaitTillConnectionMade()
        {
            int count = 0;
            while(ClientToNetPeerMap.Count() ==0 && count < 1000000000)
            {
                count += 1;
            }
        }
        private Client GetClientRecord(NetPeer peer)
        {
            Client client = ClientToNetPeerMap.First(o => o.Value == peer).Key;
            return client;
        }

        public void CheckReceiveClient(Action<Client> callBack)
        {
            onConnectCallBack = callBack;
            if (ClientToNetPeerMap.Count() > 0)
                onConnectCallBack.Invoke(ClientToNetPeerMap.First().Key);
        }

        public void Receive(Action<ReceivedMessage> callBack)
        {
            onReceiveCallback = callBack;
        }

        public void SendUnreliable(Client client, string message)
        {
            var netPeer = this.ClientToNetPeerMap[client];

            NetDataWriter writer = new NetDataWriter();
            writer.Put(message);
            netPeer.Send(writer,SendOptions.Unreliable);
        }

        public void ClientDisconnect(Action<Client> callBack)
        {
            onDisconnectCallBack = callBack;
        }

        public void SendReliable(Client client, string message)
        {
            var netPeer = this.ClientToNetPeerMap[client];

            NetDataWriter writer = new NetDataWriter();
            writer.Put(message);
            netPeer.Send(writer, SendOptions.ReliableOrdered);
        }
    }
}
