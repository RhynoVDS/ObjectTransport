using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OTransport
{
    public class ObjectTransport
    {
        public static ObjectTransportFactory Factory = new ObjectTransportFactory();
        List<Client> clients = new List<Client>();
        private INetworkChannel NetworkChannel;
        
        private ConcurrentDictionary<string, MessageResponseHandle> ResponseHandle = new ConcurrentDictionary<string, MessageResponseHandle>();
        private ConcurrentDictionary<Type, ReceivedMessageHandle> ReceiveHandle = new ConcurrentDictionary<Type, ReceivedMessageHandle>();
        private Action<Client> OnClientConnectHandler = null;
        private Action<Client> onClientDisconnectHandler = null;

        public ObjectTransport(INetworkChannel networkChannel)
        {
            NetworkChannel = networkChannel;
            TimeOutCheck();
            SetupNetworkReceiveCallback();
            SetUpClientConnectCallback();
        }
        /// <summary>
        /// This function will return a list of all clients that are currently connected.
        /// </summary>
        /// <returns>IEnumerable of connected clients</returns>
        public IEnumerable<Client> GetConnecectedClients()
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

        private Type GetRecievedObjectType(string json)
        {
            dynamic deserialisedObject = JsonConvert.DeserializeObject(json);
            Type returnType = Type.GetType(deserialisedObject.Type.ToString());
            return returnType;
        }

        private void SetupNetworkReceiveCallback()
        {
            //Set a callback for the network channel

            NetworkChannel.Receive((message) =>
            {
                Type receivedObjectType = GetRecievedObjectType(message.Message);
                string objectJson = GetRecievedObjectJSON(message.Message);
                string token = GetReceivedObjectToken(message.Message);

                object receivedObject = JsonConvert.DeserializeObject(objectJson, receivedObjectType);

                if (token != null && ResponseHandle.ContainsKey(token))
                {
                    CheckExecuteResponseHandle(message, receivedObjectType, token, receivedObject);
                }
                else if (ReceiveHandle.ContainsKey(receivedObjectType))
                {
                    CheckExecuteReceiveAction(message, receivedObjectType, receivedObject);
                    CheckExecuteReplyAction(message, receivedObjectType, token, receivedObject);
                }
            });
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
                queueMessage.Token = token;
                Send(queueMessage);
            }
        }

        private string GetReceivedObjectToken(string message)
        {
            dynamic deserialisedObject = JsonConvert.DeserializeObject(message);

            return deserialisedObject.Token;
        }

        private void SetUpClientConnectCallback()
        {
            NetworkChannel.CheckReceiveClient((client)=>
            { 
                clients.Add(client);
                OnClientConnectHandler?.Invoke(client);
            });

            NetworkChannel.ClientDisconnect((client)=>
            {
                clients.Remove(client);
                onClientDisconnectHandler?.Invoke(client);
            });
        }

        private string GetRecievedObjectJSON(string json)
        {
            dynamic deserialisedObject = JsonConvert.DeserializeObject(json);
            return JsonConvert.SerializeObject(deserialisedObject.Object);
        }

        private string GetJSONpayload(QueuedMessage send)
        {
            object objectToSend = send.ObjectToSend;
            Type objectType = objectToSend.GetType();
            string jsonPayload = string.Empty;

            if(send.Token!=null)
            {
                PayLoadWithToken payload = new PayLoadWithToken();
                payload.Object = objectToSend;
                payload.Type = objectType.AssemblyQualifiedName;
                payload.Token = send.Token;
                jsonPayload = JsonConvert.SerializeObject(payload);
            }
            else if (send.resonseType_to_actionMatch.Count == 0)
            {
                PayLoad payload = new PayLoad();
                payload.Object = objectToSend;
                payload.Type = objectType.AssemblyQualifiedName;
                jsonPayload = JsonConvert.SerializeObject(payload);
            }
            else
            {
                string token = Guid.NewGuid().ToString();
                PayLoadWithToken payload = new PayLoadWithToken();
                payload.Object = objectToSend;
                payload.Type = objectType.AssemblyQualifiedName;
                payload.Token = token;
                jsonPayload = JsonConvert.SerializeObject(payload);

                MessageResponseHandle responseHandle = new MessageResponseHandle(send);
                responseHandle.ClientsToRespond.AddRange(send.sendTo);
                ResponseHandle.TryAdd(token, responseHandle);
            }
            return jsonPayload;
        }
        internal void Send(QueuedMessage send)
        {
            if (clients.Count == 0)
                return;

            Client[] clientsTo = send.sendTo;

            if (clientsTo == null || clientsTo.Count() == 0)
                clientsTo = new Client[] { clients[0] };

            send.sendTo = clientsTo;

            string jsonPayload = GetJSONpayload(send);

            foreach (Client client in clientsTo)
                NetworkChannel.Send(client, jsonPayload);
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
    }
}
