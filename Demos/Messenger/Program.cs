using OTransport.NetworkChannel.TCP;
using OTransport;
using System;
using OTransport.NetworkChannel.UDP;
using OTransport.Serializer.JSON;

namespace Messenger
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to this simple Chat room!");
            Console.WriteLine("Would you like to:");
            Console.WriteLine("1) Create a TCP server");
            Console.WriteLine("2) Connect to TCP server");

            IObjectTransport transport;

            string answer = Console.ReadLine();

            //Create Server
            if (answer == "1")
            {
                transport = ObjectTransport.Factory.CreateTCPServer()
                                                   .UseJSONserialization()
                                                   .Build()
                                                   .Start("127.0.0.1", 1234);

                //Receive a receive an object of type Message. c= Client, m = Object that was received
                transport.Receive<Message>((c, m) =>
                {
                    Console.WriteLine("{0} - {1}", c.IPAddress, m.Body);

                    //We want all clients to see the message that was sent to the server.
                    //However, we do not want the original client to receive the message.
                    transport.Send(m)
                             .ToAllExcept(c)
                             .Execute();
                            
                })
                .Execute();
            }
            else
            {
                //Create Client
                transport = ObjectTransport.Factory.CreateTCPClient()
                                                   .UseJSONserialization()
                                                   .Build()
                                                   .Start("127.0.0.1", 1234);
                

                transport.Receive<Message>((c, m) =>
                    {
                        Console.WriteLine("{0} - {1}", c.IPAddress, m.Body);
                    }
                )
                .Execute();
            }

            //Write to console when a client connects
            transport.OnClientConnect(c => Console.WriteLine("{0} - Client connected", c.IPAddress));

            //Write to console when a client disconnects
            transport.OnClientDisconnect(c => Console.WriteLine("{0} - Client disconnected", c.IPAddress));


            Console.WriteLine("Begin Chatting!");

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
