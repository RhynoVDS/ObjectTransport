using OTransport.NetworkChannel.TCP;
using OTransport;
using System;
using OTransport.NetworkChannel.UDP;

namespace Messenger
{
    class Program
    {
        static ObjectTransport CheckCreateServer(string answer)
        {
            ObjectTransport objectTransportServer = null;
            
            if(answer == "1")
            {
                Console.WriteLine("Please specify an IP address");
                var ipAddress = Console.ReadLine();

                //Setup a TCP Server
                objectTransportServer = ObjectTransport.Factory.CreateTCPServer(ipAddress, 8888);

                Console.WriteLine("Created TCP server on port 8888");
            }

            if(answer == "3")
            {
                Console.WriteLine("Please specify an IP address");
                var ipAddress = Console.ReadLine();

                //Setup a TCP Server
                objectTransportServer = ObjectTransport.Factory.CreateUDPServer(ipAddress, 8888);

                Console.WriteLine("Created UDP server on port 8888");
            }

            //Receive an object of type "Message"
            objectTransportServer?.Receive<Message>((client,received_message) => {

                    Console.WriteLine("{0} - {1}", client.IPAddress, received_message.Body);

                    //Send the received message to all other clients 
                    //except the client who sent the message
                    objectTransportServer.Send(received_message)
                             .ToAllExcept(client)
                             .Execute();
                })
                 .Execute();

            return objectTransportServer;
        }

        static ObjectTransport CheckCreateClient(string answer)
        {
            ObjectTransport objectTransportClient = null;
            
            if(answer == "2")
            {
                Console.WriteLine("Please specify an IP address");
                var ipAddress = Console.ReadLine();

                //Setup a TCP Server
                objectTransportClient = ObjectTransport.Factory.CreateTCPClient(ipAddress, 8888);

                Console.WriteLine("Created TCP server on port 8888");
            }

            if(answer == "4")
            {
                Console.WriteLine("Please specify an IP address");
                var ipAddress = Console.ReadLine();

                //Setup a TCP Server
                objectTransportClient = ObjectTransport.Factory.CreateUDPClient(ipAddress, 8888);

                Console.WriteLine("Created UDP server on port 8888");
            }

            //Receive an object of type "Message"
            //When client receives an object of type "Message" output to console.
            objectTransportClient?.Receive<Message>((client,received_Message) =>
            {
                Console.WriteLine("{0} - {1}", client.IPAddress, received_Message.Body);
            })
            .Execute();

            return objectTransportClient;
        }

        static ObjectTransport CheckCreateTransport(string answer)
        {
            ObjectTransport transport = null;

            transport = CheckCreateTransport(answer);

            return transport;
        }

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to this simple Chat room!");
            Console.WriteLine("Would you like to:");
            Console.WriteLine("1) Create a TCP server");
            Console.WriteLine("2) Connect to TCP server");
            Console.WriteLine("3) Creaet a UDP server");
            Console.WriteLine("4) Connect to UDP server");
            string answer = Console.ReadLine();

            ObjectTransport transport = transport = CheckCreateTransport(answer);

            if (transport == null)
                return;

            //Write to console when a client connects
            transport.OnClientConnect(c => Console.WriteLine("{0} - Client connected", c.IPAddress));

            //Write to console when a client disconnects
            transport.OnClientDisconnect(c => Console.WriteLine("{0} - Client disconnected", c.IPAddress));


            string message;
            while (true)
            {
                message = Console.ReadLine();

                if (message == "exit")
                    break;

                transport.Send(new Message() { Body = message })
                        .ToAll()
                        .Execute();
            }
        }
    }
    public class Message
    {
        public string Body { get; set; }
    }
}
