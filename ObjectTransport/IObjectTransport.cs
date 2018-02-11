using System;
using System.Collections.Generic;

namespace OTransport
{
    public interface IObjectTransport
    {
        /// <summary>
        /// Disconnects the first client that is connected. This is best used when the current object transport is a client connected to a server.
        /// </summary>
        void DisconnectClient();
        /// <summary>
        /// Disconnects the given client. If this is a client connected to a server, only the server can be passed in. Call DisconnectClient() instead.
        /// </summary>
        /// <param name="client"></param>
        void DisconnectClient(params Client[] client);
        /// <summary>
        /// This function will return a list of all clients that are currently connected.
        /// </summary>
        /// <returns>IEnumerable of connected clients</returns>
        IEnumerable<Client> GetConnectedClients();
        /// <summary>
        /// Set the function/lambda to be called when a client connects.
        /// </summary>
        /// <param name="onConnectFunction">The function/lambda to call when a client connects</param>
        void OnClientConnect(Action<Client> onConnectFunction);
        /// <summary>
        /// Set the function/lambda to be called when a client disconnects
        /// </summary>
        /// <param name="onDisconnectFunction">The function/lambda to call when a client disconnects</param>
        void OnClientDisconnect(Action<Client> onDisconnectFunction);
        /// <summary>
        /// Use this method to handle the event when receiving a message fails to be processed by object transport.
        /// The first parameter is the Received message. This contains the message body as a string and the client who sent the message.
        /// The Second parameter is the exception that was thrown to cause the receive to fail.
        /// </summary>
        void OnFailedReceive(Action<ReceivedMessage, Exception> onfail);
        /// <summary>
        /// Setup a listener to execute when an object is received of the given type. This will execute the given function/lambda and pass in the object that was received.
        /// </summary>
        /// <typeparam name="ReceivedType">The received object type to listen for and handle.</typeparam>
        /// <param name="function">The function/lambda to execute when an object of the specified type is received. This function will have the object passed in as a parameter.</param>
        /// <returns></returns>
        MessageReceive<ReceivedType> Receive<ReceivedType>();
        /// <summary>
        /// Setup a listener to execute when an object is received of the given type. This will execute the given function/lambda and pass in the object that was received as well as the client who sent the object.
        /// </summary>
        /// <typeparam name="ReceivedType">The received object type to listen for and handle.</typeparam>
        /// <param name="function">The function/lambda to execute when an object of the specified type is received. This function will have the object passed in as a parameter. It will also have the client passed in.</param>
        /// <returns></returns>
        MessageReceive<ReceivedType> Receive<ReceivedType>(Action<Client, ReceivedType> obj);
        /// <summary>
        /// Setup a listener to execute when an object is received of the given type. This will execute the given function/lambda and pass in the object that was received.
        /// </summary>
        /// <typeparam name="ReceivedType">The received object type to listen for and handle.</typeparam>
        /// <param name="function">The function/lambda to execute when an object of the specified type is received. This function will have the object passed in as a parameter.</param>
        /// <returns></returns>
        MessageReceive<ReceivedType> Receive<ReceivedType>(Action<ReceivedType> function);
        /// <summary>
        /// Send an object through the network channel
        /// </summary>
        /// <typeparam name="SendType">The type of the object being sent.</typeparam>
        /// <param name="obj">The object to send.</param>
        /// <returns></returns>
        MessageSend<SendType> Send<SendType>(SendType obj);
        /// <summary>
        /// Make any subsequent messages default to reliable. The underlining network channel will throw an exception if it is not supported
        /// </summary>
        void SetReliable();
        /// <summary>
        /// Make any subsequent messages default to unreliable. The underlining network channel will throw an exception if it is not supported
        /// </summary>
        void SetUnreliable();
        /// <summary>
        /// Stop the underlying channel
        /// </summary>
        void Stop();
    }
}