using System;
using System.Collections.Generic;
using System.Linq;

namespace OTransport
{
    public class MessageSend<SendType> 
    {
        private QueuedMessage message = new QueuedMessage();
        private ObjectTransport ObjectTransport;

        internal MessageSend(SendType sendObject,ObjectTransport objectTransport)
        {
            message.ObjectToSend = sendObject;
            ObjectTransport = objectTransport;
            message.SendReliable = ObjectTransport.SendReliable;
        }

        /// <summary>
        /// After sending an object, wait for a response back. If the response is of the given object type, execute the given function/lambda. The received object is fed into the function/lambda.
        /// The client who the object was sent to must respond with the reply function
        /// </summary>
        /// <typeparam name="ResponseType">The object type to handle</typeparam>
        /// <param name="onResponseFunction">the function/lambda to execute when an object of the given type is received</param>
        /// <returns></returns>
        public MessageSend<SendType> Response<ResponseType>(Action<ResponseType> onResponseFunction)
        {
            message.resonseType_to_actionMatch.Add(typeof(ResponseType), onResponseFunction);
            return this;
        }

        /// <summary>
        /// When a response is expected after sending an object but the timeout has run out, a handler will execute. This function sets the timeout function/lambda.
        /// </summary>
        /// <param name="function">The function/lambda to execute. An array of Clients who did not respond and the object that was sent is passed as parameters.</param>
        /// <returns></returns>
        public MessageSend<SendType> OnTimeOut(Action<Client[], SendType> function)
        {
            message.TimeOutFunction = function;
            return this;
        }

        /// <summary>
        /// After sending an object, wait for a response back. If the response is of the given object type, execute the given function/lambda. The received object as well as the client who sent it is fed into the function/lambda.
        /// The client who the object was sent to must respond with the reply function
        /// </summary>
        /// <typeparam name="ResponseType">The object type to handle</typeparam>
        /// <param name="onResponseFunction">the function/lambda to execute when an object of the given type is received</param>
        /// <returns></returns>
        public MessageSend<SendType> Response<ResponseType>(Action<Client,ResponseType> onResponseFunction)
        {
            message.resonseType_to_actionMatch.Add(typeof(ResponseType), onResponseFunction);
            return this;
        }

        /// <summary>
        /// If there has been no response for the given amount of time, stop listening for responses
        /// </summary>
        /// <param name="seconds">The number of seconds to wait for a response</param>
        /// <returns></returns>
        public MessageSend<SendType> SetTimeOut(int seconds)
        {
            message.TimeOutInSeconds = seconds;
            return this;
        }
        /// <summary>
        /// The client to send the message to. This is not needed if the message is going to a Server.
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        public MessageSend<SendType> To(params Client[] client)
        {
            message.sendTo = client;
            return this;
        }

        /// <summary>
        /// Send the given object to the client and listen for any responses back from the client
        /// </summary>
        public void Execute()
        {
            ObjectTransport.Send(message);
        }

        /// <summary>
        /// Send the object to all clients.
        /// </summary>
        /// <returns></returns>
        public MessageSend<SendType> ToAll()
        {
            message.sendTo = ObjectTransport.GetConnecectedClients().ToArray();
            return this;
        }
        /// <summary>
        /// Send the object to all clients except the given clients
        /// </summary>
        /// <param name="except">Clients to exclude</param>
        /// <returns></returns>
        public MessageSend<SendType> ToAllExcept(params Client [] except)
        {
            message.sendTo = ObjectTransport.GetConnecectedClients().Where(c => !except.Contains(c)).ToArray();
            return this;
        }

        /// <summary>
        /// Send the object reliably over the given network channel if it is supported. If it is not supported, the channel will throw an exception.
        /// </summary>
        /// <returns></returns>
        public MessageSend<SendType> Reliable()
        {
            message.SendReliable = true;
            return this;
        }

        /// <summary>
        /// Send the object unreliably over the given network channel if it is supported. If it is not supported, the channel will throw an exception.
        /// </summary>
        /// <returns></returns>
        public MessageSend<SendType> Unreliable()
        {
            message.SendReliable = false;
            return this;
        }
    }
}
