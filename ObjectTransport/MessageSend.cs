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
        }
        public MessageSend<SendType> Response<ResponseType>(Action<ResponseType> onResponseFunction)
        {
            message.resonseType_to_actionMatch.Add(typeof(ResponseType), onResponseFunction);
            return this;
        }
        public MessageSend<SendType> Response<ResponseType>(Action<Client,ResponseType> onResponseFunction)
        {
            message.resonseType_to_actionMatch.Add(typeof(ResponseType), onResponseFunction);
            return this;
        }

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
        public void Execute()
        {
            ObjectTransport.Send(message);
        }

        public MessageSend<SendType> ToAll()
        {
            message.sendTo = ObjectTransport.GetConnecectedClients().ToArray();
            return this;
        }
        public MessageSend<SendType> ToAll(params Client [] except)
        {
            message.sendTo = ObjectTransport.GetConnecectedClients().Where(c => !except.Contains(c)).ToArray();
            return this;
        }
    }
}
