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

namespace OTransport.NetworkChannel.UDP
{
    public class UDPClientChannel : INetworkChannel
    {
        //This should technically only ever have on entry as this is the client.
        private Dictionary<Client, NetPeer> ClientToNetPeerMap = new Dictionary<Client, NetPeer>();
        private IPAddress IPAddress;

        private EventBasedNetListener listener;
        private NetManager clientUDP;

        private int _LocalPort;
        private bool ReliableTransport = false;


        Action<ReceivedMessage> onReceiveCallback = null;
        Action<Client> onConnectCallBack = null;
        Action<Client> onDisconnectCallBack = null;

        public int LocalPort { get { return _LocalPort; } }

        public void Stop()
        {
            if(clientUDP?.IsRunning != null && clientUDP?.IsRunning == true)
                clientUDP?.Stop();

            foreach (Client client in ClientToNetPeerMap.Keys)
            {
                onDisconnectCallBack?.Invoke(client);
            }
        }
        private void WaitTillConnectionMade()
        {

            int count = 0;

            //Wait until the connection is made and make sure that it doesn't time out.
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

        public void OnClientConnect(Action<Client> callBack)
        {
            onConnectCallBack = callBack;
            if (ClientToNetPeerMap.Count() > 0)
                onConnectCallBack.Invoke(ClientToNetPeerMap.First().Key);
        }

        public void OnReceive(Action<ReceivedMessage> callBack)
        {
            onReceiveCallback = callBack;
        }

        public void OnClientDisconnect(Action<Client> callBack)
        {
            onDisconnectCallBack = callBack;
        }

        public void SetReliable()
        {
            ReliableTransport = true;
        }

        public void SetUnreliable()
        {
            ReliableTransport = false;
        }

        public void Send(Client client, string payload)
        {
            var netPeer = this.ClientToNetPeerMap[client];

            NetDataWriter writer = new NetDataWriter();
            writer.Put(payload);
            if (ReliableTransport)
                netPeer.Send(writer, SendOptions.ReliableOrdered);
            else
                netPeer.Send(writer, SendOptions.Sequenced);
        }

        public void DisconnectClient(params Client[] clients)
        {
            //This is the client so stop the connection completely.
            Stop();
        }

        public void Start(string ipaddress, int port)
        {
            listener = new EventBasedNetListener();
            clientUDP = new NetManager(listener, "ConnectionKey");
            clientUDP.UnsyncedEvents = true;
            clientUDP.Start();
            clientUDP.Connect(ipaddress, port);

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
            this._LocalPort = clientUDP.LocalPort;
        }
    }
}
