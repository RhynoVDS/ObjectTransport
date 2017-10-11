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

        internal MessageReceive(ObjectTransport objectTransport)
        {
            ObjectTransport = objectTransport;
        }

        /// <summary>
        /// After receiving an object, execute a function/lambda. What ever this function/lambda returns will be sent back to the client who sent the original message. The original object that was received is passed as a paramter to the function/lambda.
        /// </summary>
        /// <param name="replyFunction">the function/lambda to execute to get a reply object to send back.</param>
        /// <returns></returns>
        public MessageReceive<ReceivedType> Reply(Func<ReceivedType, object> replyFunction)
        {
            handle.ReplyFunction = replyFunction;
            return this;
        }

        /// <summary>
        /// After receiving an object, execute a function/lambda. What ever this function/lambda returns will be sent back to the client who sent the original message. The original object that was received as well as the client who sent the original object is passed as a paramter to the function/lambda.
        /// </summary>
        /// <param name="replyFunction">the function/lambda to execute to get a reply object to send back.</param>
        /// <returns></returns>
        public MessageReceive<ReceivedType> Reply(Func<Client, ReceivedType, object> replyFunction)
        {
            handle.ReplyFunction = replyFunction;
            return this;
        }

        /// <summary>
        /// Execute and begin listening for objects of the given type to be received.
        /// </summary>
        public void Execute()
        {
            handle.RecieveType = typeof(ReceivedType);
            ObjectTransport.AddToListenerHandles(handle);
        }
    }
}
