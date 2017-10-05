using OTransport.Network_Channels;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OTransport.Implementation
{
    public class TCPClientChannel : INetworkChannel
    {
        private IPAddress IPAddress;
        private int port;
        private TcpClient tcpClient = null;
        private Client client = null;

        private Action<ReceivedMessage> onReceiveCallback = null;
        private Task ListenTask = null;
        Action<Client> onConnectCallBack = null;
        Action<Client> onDisconnectCallBack = null;

        public TCPClientChannel(string ipAddress, int Port)
        {
            tcpClient = new TcpClient();
            client = new Client(ipAddress, port);
            IPAddress = IPAddress.Parse(ipAddress);
            port = Port;

            ConnectToServer();
            ListenThread();
        }
        private void ListenThread()
        {
            Task clientTask = new Task((c) =>
            {
                while (true)
                {
                    Byte[] bytes;
                    try
                    {
                        NetworkStream ns = tcpClient.GetStream();
                        if (tcpClient.ReceiveBufferSize > 0)
                        {
                            bytes = new byte[tcpClient.ReceiveBufferSize];
                            ns.Read(bytes, 0, tcpClient.ReceiveBufferSize);
                            string msg = Encoding.ASCII.GetString(bytes);

                            ReceivedMessage message = new ReceivedMessage((Client)c, msg);

                            if (onReceiveCallback != null)
                                onReceiveCallback.Invoke(message);
                        }
                    }
                    catch
                    {
                        if (onDisconnectCallBack != null)
                            onDisconnectCallBack.Invoke((Client)c);

                        break;
                    }

                }
            }, client);
            clientTask.Start();
            ListenTask = clientTask;
        }

        private void ConnectToServer()
        {
            tcpClient.ConnectAsync(IPAddress, port).Wait();
        }
        public void CheckReceiveClient(Action<Client> callBack)
        {
            onConnectCallBack = callBack;
            if (client != null)
                onConnectCallBack.Invoke(client);
        }

        public void Receive(Action<ReceivedMessage> callBack)
        {
            onReceiveCallback = callBack;
        }

        public void Send(Client client, string message)
        {
            Byte[] data = System.Text.Encoding.ASCII.GetBytes(message);

            NetworkStream stream = tcpClient.GetStream();

            stream.Write(data, 0, data.Length);
        }

        public void Stop()
        {
            if (tcpClient.Connected)
            {
                tcpClient.Client.Shutdown(SocketShutdown.Both);
                tcpClient.Client.Dispose();
            }
        }

        public void ClientDisconnect(Action<Client> callBack)
        {
            onDisconnectCallBack = callBack;
        }
    }
}
