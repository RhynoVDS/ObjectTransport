using System;
using System.Collections.Generic;

namespace OTransport
{
    public class MessageReceive<ReceivedType>
    {
        ReceivedMessageHandle handle = new ReceivedMessageHandle();
        private ObjectTransport ObjectTransport;

        internal MessageReceive(Delegate actionReceived,ObjectTransport objectTransport)
        {
            handle.ReceiveAction = actionReceived;
            ObjectTransport = objectTransport;
        }
        internal MessageReceive(Action<Client,ReceivedType> actionReceived)
        {
            handle.ReceiveAction = actionReceived;
        }

        public MessageReceive(ObjectTransport objectTransport)
        {
            ObjectTransport = objectTransport;
        }

        public MessageReceive<ReceivedType> Reply(Func<ReceivedType, object> replyFunction)
        {
            handle.ReplyFunction = replyFunction;
            return this;
        }
        public MessageReceive<ReceivedType> Reply(Func<Client, ReceivedType, object> replyFunction)
        {
            handle.ReplyFunction = replyFunction;
            return this;
        }

        public void Execute()
        {
            handle.RecieveType = typeof(ReceivedType);
            ObjectTransport.AddToListenerHandles(handle);
        }
    }
}
