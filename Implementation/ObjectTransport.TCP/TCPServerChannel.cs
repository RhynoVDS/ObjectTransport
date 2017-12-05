using OTransport;
using OTransport.Network_Channels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OT.TCP.Implementation
{
    public class TCPServerChannel : INetworkChannel
    {
        private Dictionary<Client, TcpClient> ClientToTCPMap = new Dictionary<Client, TcpClient>();
        private IPAddress IPAddress;
        private TcpListener Server;
        public int Port;

        Action<ReceivedMessage> onReceiveCallback = null;
        List<Task> clientTasks = new List<Task>();
        Action<Client> onConnectCallBack = null;
        Action<Client> onDisconnectCallBack = null;

        public void Stop()
        {
            var clients = ClientToTCPMap.Keys.ToArray();
            foreach(var client in clients)
            {
                ClientToTCPMap[client].Client.Shutdown(SocketShutdown.Both);
                ClientToTCPMap[client].Client.Dispose();
                ClientToTCPMap.Remove(client);
            }
            Server.Stop();
        }
        public TCPServerChannel(string ipAddress, int port)
        {
            IPAddress = IPAddress.Parse(ipAddress);

            Server = new TcpListener(IPAddress, port);
            Server.Start();
            Port = int.Parse(Server.LocalEndpoint.ToString().Split(':')[1]);

            StartListeningThread();
        }
        private void StartListeningThread()
        {
            Thread listenConnection = new Thread(async() =>
            {
                while (true)
                {
                    TcpClient tcpClient = null;
                    try
                    {
                        tcpClient = await Server.AcceptTcpClientAsync();
                    }
                    catch (ObjectDisposedException) { break; }

                    string[] addressArray = tcpClient.Client.RemoteEndPoint.ToString().Split(':');
                    Client client = new Client(addressArray[0], int.Parse(addressArray[1]));
                    ClientToTCPMap.Add(client, tcpClient);

                    if(onConnectCallBack !=null)
                        onConnectCallBack.Invoke(client);


                    Task clientTask = new Task((c) =>
                    {
                        Byte[] bytes;
                        while (true)
                        {
                            try
                            {
                                if(!TCPUtilities.IsConnected(tcpClient))
                                    throw new Exception("Client Disconnected");

                                NetworkStream ns = tcpClient.GetStream();

                                if (tcpClient.ReceiveBufferSize > 0)
                                {
                                    bytes = new byte[tcpClient.ReceiveBufferSize];
                                    ns.Read(bytes, 0, tcpClient.ReceiveBufferSize);
                                    string msg = Encoding.ASCII.GetString(bytes);

                                    ReceivedMessage message = null;
                                    message = new ReceivedMessage((Client)c, msg);


                                    if (onReceiveCallback != null)
                                        onReceiveCallback.Invoke(message);
                                }
                            }
                            catch
                            {
                                ClientToTCPMap.Remove((Client)c);
                                if (onDisconnectCallBack != null)
                                    onDisconnectCallBack.Invoke((Client)c);
                                break;
                            }

                        }
                    },client);
                    clientTask.Start();
                    clientTasks.Add(clientTask);
                }
            });
            listenConnection.Start();
        }

        public void CheckReceiveClient(Action<Client> callBack)
        {
            onConnectCallBack = callBack;
        }

        public void Receive(Action<ReceivedMessage> callBack)
        {
            onReceiveCallback = callBack;
        }

        public void ClientDisconnect(Action<Client> callBack)
        {
            onDisconnectCallBack = callBack;
        }

        public void SendReliable(Client client, string message)
        {
            var tcpClient = ClientToTCPMap[client];
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

            NetworkStream stream = tcpClient.GetStream();

            stream.Write(data, 0, data.Length);
        }
        public void SendUnreliable(Client client, string message)
        {
            throw new NotSupportedException("This network channel does not support un-reliable sending");
        }
    }
}
