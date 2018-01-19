using OTransport.Factory;
using OTransport.Serializer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace OTransport
{
    public class ObjectTransport
    {
        public static ObjectTransportFactory Factory = new ObjectTransportFactory();
        List<Client> clients = new List<Client>();
        private readonly INetworkChannel NetworkChannel;
        private readonly ISerializer Serializer;
        
        private ConcurrentDictionary<string, MessageResponseHandle> ResponseHandle = new ConcurrentDictionary<string, MessageResponseHandle>();
        private ConcurrentDictionary<Type, ReceivedMessageHandle> ReceiveHandle = new ConcurrentDictionary<Type, ReceivedMessageHandle>();
        private Action<Client> OnClientConnectHandler = null;
        private Action<Client> onClientDisconnectHandler = null;
        private Action<ReceivedMessage,Exception> OnFailedReceiveHandler = null;

        internal bool SendReliable = true;
        private readonly int TokenLength = 8;

        public ObjectTransport(INetworkChannel networkChannel, ISerializer serializer)
        {
            NetworkChannel = networkChannel;
            Serializer = serializer;

            TimeOutCheck();
            SetupNetworkReceiveCallback();
            SetUpClientConnectCallback();
        }
        
        /// <summary>
        /// Make any subsequent messages default to reliable. The underlining network channel will throw an exception if it is not supported
        /// </summary>
        public void SetReliable()
        {
            SendReliable = true;
        }

        /// <summary>
        /// Make any subsequent messages default to unreliable. The underlining network channel will throw an exception if it is not supported
        /// </summary>
        public void SetUnreliable()
        {
            SendReliable = false;
        }
        /// <summary>
        /// This function will return a list of all clients that are currently connected.
        /// </summary>
        /// <returns>IEnumerable of connected clients</returns>
        public IEnumerable<Client> GetConnectedClients()
        {
            return clients;
        }

        /// <summary>
        /// Set the function/lambda to be called when a client connects.
        /// </summary>
        /// <param name="onConnectFunction">The function/lambda to call when a client connects</param>
        public void OnClientConnect(Action<Client> onConnectFunction)
        {
            OnClientConnectHandler = onConnectFunction;
        }
        /// <summary>
        /// Set the function/lambda to be called when a client disconnects
        /// </summary>
        /// <param name="onDisconnectFunction">The function/lambda to call when a client disconnects</param>
        public void OnClientDisconnect(Action<Client> onDisconnectFunction)
        {
            onClientDisconnectHandler = onDisconnectFunction;
        }

        private void TimeOutCheck()
        {
            Timer timer = new Timer(o =>
            {
                IEnumerable<string> keys = ResponseHandle.Keys;
                foreach(var key in keys)
                {
                    MessageResponseHandle handler;
                    while(!ResponseHandle.TryGetValue(key, out handler)){ }

                    if (handler == null)
                        continue;

                    handler.SecondsPassed += 1;

                    if(handler.SecondsPassed >= handler.sentMessage.TimeOutInSeconds)
                    {
                        while (!ResponseHandle.TryRemove(key,out handler)) { }

                        if (handler.sentMessage.TimeOutFunction != null)
                            handler.sentMessage.TimeOutFunction.DynamicInvoke(handler.ClientsToRespond.ToArray(), handler.sentMessage.ObjectToSend);
                    }
                        
                }

            }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        }

        private void SetupNetworkReceiveCallback()
        {
            //Set a callback for the network channel

            NetworkChannel.OnReceive((message) =>
            {
                try
                {
                    (Type, string, string) objectType_token_objectPayload = ParseRecievedMessage(message.Message);

                    Type receivedObjectType = objectType_token_objectPayload.Item1;
                    string token = objectType_token_objectPayload.Item2;
                    string objectPayload = objectType_token_objectPayload.Item3;

                    if (receivedObjectType == null)
                        return;

                    object receivedObject = Serializer.Deserialize(objectPayload, receivedObjectType);

                    if (token != null && ResponseHandle.ContainsKey(token))
                    {
                        CheckExecuteResponseHandle(message, receivedObjectType, token, receivedObject);
                    }
                    else if (ReceiveHandle.ContainsKey(receivedObjectType))
                    {
                        CheckExecuteReceiveAction(message, receivedObjectType, receivedObject);
                        CheckExecuteReplyAction(message, receivedObjectType, token, receivedObject);
                    }
                }
                catch(Exception e) 
                {
                    //Error parsing the message. Invoke the OnFailedReceiveHandler.
                    OnFailedReceiveHandler?.Invoke(message, e);
                    return;
                }
            });
        }

        /// <summary>
        /// Use this method to handle the event when receiving a message fails to be processed by object transport.
        /// The first parameter is the Received message. This contains the message body as a string and the client who sent the message.
        /// The Second parameter is the exception that was thrown to cause the receive to fail.
        /// </summary>
        public void OnFailedReceive(Action<ReceivedMessage,Exception> onfail)
        {
            OnFailedReceiveHandler = onfail;
        }

        /// <summary>
        /// Disconnects the given client. If this is a client connected to a server, only the server can be passed in. Call DisconnectClient() instead.
        /// </summary>
        /// <param name="client"></param>
        public void DisconnectClient(params Client[] client)
        {
            NetworkChannel.DisconnectClient(client);
        }
        /// <summary>
        /// Disconnects the first client that is connected. This is best used when the current object transport is a client connected to a server.
        /// </summary>
        public void DisconnectClient()
        {

            if (this.clients.Count() > 0)
            {
                NetworkChannel.DisconnectClient(this.clients.First());
            }
        }

        private (Type, string, string) ParseRecievedMessage(string message)
        {
            int firstDivide = message.IndexOf("::");
            var typeName = message.Substring(0, firstDivide);

            Type returnType = Type.GetType(typeName);
            var payload = message.Substring(firstDivide + 2);
            string token = null;

            int secondDivide = payload.IndexOf("::");

            if(secondDivide > -1 && secondDivide == TokenLength)
            {
                token = payload.Substring(0, secondDivide);
                payload = payload.Substring(secondDivide + 2);
            }

            return (returnType,token,payload);
        }

        private void CheckExecuteResponseHandle(ReceivedMessage message, Type receivedObjectType, string token, object receivedObject)
        {
            var responseType_ActionMatch = ResponseHandle[token].sentMessage.resonseType_to_actionMatch;
            if (responseType_ActionMatch.ContainsKey(receivedObjectType))
            {
                Delegate responseHandle = responseType_ActionMatch[receivedObjectType];
                if (responseHandle.GetMethodInfo().GetParameters().Count() > 1)
                {
                    responseHandle.DynamicInvoke(message.From, receivedObject);
                }
                else
                {
                    responseHandle.DynamicInvoke(receivedObject);
                }

                //Remove the client from the list of clients who still need to respond
                ResponseHandle[token].ClientsToRespond.Remove(message.From);
            }
        }

        private void CheckExecuteReceiveAction(ReceivedMessage message,Type receivedObjectType, object receivedObject)
        {
            Delegate receiveAction = ReceiveHandle[receivedObjectType].ReceiveAction;
            if (receiveAction != null)
            {
                if (receiveAction.GetMethodInfo().GetParameters().Count() > 1)
                {
                    receiveAction.DynamicInvoke(message.From, receivedObject);
                }
                else
                {
                    receiveAction.DynamicInvoke(receivedObject);
                }

            }
        }

        private void CheckExecuteReplyAction(ReceivedMessage message, Type receivedObjectType, string token, object receivedObject)
        {
            Delegate replyFunction = ReceiveHandle[receivedObjectType].ReplyFunction;
            if (replyFunction != null)
            {
                object result;
                if (replyFunction.GetMethodInfo().GetParameters().Count() > 1)
                    result = replyFunction.DynamicInvoke(message.From, receivedObject);
                else
                    result = replyFunction.DynamicInvoke(receivedObject);

                if (result == null)
                    return;

                QueuedMessage queueMessage = new QueuedMessage();
                queueMessage.ObjectToSend = result;
                queueMessage.sendTo = new Client[] { message.From };
                queueMessage.SendReliable = SendReliable;
                queueMessage.Token = token;
                Send(queueMessage);
            }
        }

        private void SetUpClientConnectCallback()
        {
            NetworkChannel.OnClientConnect((client)=>
            { 
                clients.Add(client);
                OnClientConnectHandler?.Invoke(client);
            });

            NetworkChannel.OnClientDisconnect((client)=>
            {
                clients.Remove(client);
                onClientDisconnectHandler?.Invoke(client);
            });
        }

        private string GenerateToken()
        {
            //Get the first 8 characters of a newly generated token
            return Guid.NewGuid().ToString().Split('-')[0];
        }
        private string GetPayload(QueuedMessage send)
        {
            object objectToSend = send.ObjectToSend;
            Type objectType = objectToSend.GetType();

            string object_AssemblyQualifiedName = objectType.AssemblyQualifiedName;
            string serialized_object = Serializer.Serialize(objectToSend);

            string token = send.Token;

            //If this queued message is waiting for a response eg (Send().Response())
            if (send.resonseType_to_actionMatch.Count > 0)
            {
                token = GenerateToken();

                MessageResponseHandle responseHandle = new MessageResponseHandle(send);
                responseHandle.ClientsToRespond.AddRange(send.sendTo);
                ResponseHandle.TryAdd(token, responseHandle);
            }

            string payload = string.Empty;

            if(token == null)
                payload = string.Format("{0}::{1}", object_AssemblyQualifiedName, serialized_object);
            else
                payload = string.Format("{0}::{1}::{2}", object_AssemblyQualifiedName, token, serialized_object);

            return payload;
        }
        internal void Send(QueuedMessage send)
        {
            if (clients.Count == 0)
                return;

            Client[] clientsTo = send.sendTo;

            if (clientsTo == null || clientsTo.Count() == 0)
                clientsTo = new Client[] { clients[0] };

            send.sendTo = clientsTo;

            string payload = GetPayload(send);

            foreach (Client client in clientsTo)
            {
                if (send.SendReliable)
                    NetworkChannel.SetReliable();
                else
                    NetworkChannel.SetUnreliable();
                   
                NetworkChannel.Send(client, payload);
            }
        }
        /// <summary>
        /// Stop the underlying channel
        /// </summary>
        public void Stop()
        {
            NetworkChannel.Stop();
        }

        /// <summary>
        /// Send an object through the network channel
        /// </summary>
        /// <typeparam name="SendType">The type of the object being sent.</typeparam>
        /// <param name="obj">The object to send.</param>
        /// <returns></returns>
        public MessageSend<SendType> Send<SendType>(SendType obj)
        {
            var messageSend = new MessageSend<SendType>(obj,this);

            return messageSend;
        }
        internal void AddToListenerHandles(ReceivedMessageHandle handle)
        {
            Type type = handle.RecieveType;
            if (ReceiveHandle.ContainsKey(type))
                throw new ObjectTransportException("This object type is already being handled");

            while(!ReceiveHandle.TryAdd(type, handle)) { }
        }
        /// <summary>
        /// Setup a listener to execute when an object is received of the given type. This will execute the given function/lambda and pass in the object that was received.
        /// </summary>
        /// <typeparam name="ReceivedType">The received object type to listen for and handle.</typeparam>
        /// <param name="function">The function/lambda to execute when an object of the specified type is received. This function will have the object passed in as a parameter.</param>
        /// <returns></returns>
        public MessageReceive<ReceivedType> Receive<ReceivedType>(Action<ReceivedType> function)
        {
            return new MessageReceive<ReceivedType>(function,this);
        }

        /// <summary>
        /// Setup a listener to execute when an object is received of the given type. This will execute the given function/lambda and pass in the object that was received as well as the client who sent the object.
        /// </summary>
        /// <typeparam name="ReceivedType">The received object type to listen for and handle.</typeparam>
        /// <param name="function">The function/lambda to execute when an object of the specified type is received. This function will have the object passed in as a parameter. It will also have the client passed in.</param>
        /// <returns></returns>
        public MessageReceive<ReceivedType> Receive<ReceivedType>(Action<Client,ReceivedType> obj)
        {
            return new MessageReceive<ReceivedType>(obj,this);
        }
        /// <summary>
        /// Setup a listener to execute when an object is received of the given type. 
        /// </summary>
        /// <typeparam name="ReceivedType">The received object type to listen for and handle.</typeparam>
        /// <returns></returns>
        public MessageReceive<ReceivedType> Receive<ReceivedType>()
        {
            return new MessageReceive<ReceivedType>(this);
        }
        
        public static implicit operator ObjectTransport(ObjectTransportAssemblyLine objectTransportAssemblyLine)
        {
            return objectTransportAssemblyLine.Build();
        }
    }
}
