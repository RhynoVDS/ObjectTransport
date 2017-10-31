using OTransport;
using System;

namespace Messenger
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Welcome to this simple Chat room!");
            Console.WriteLine("Would you like to:");
            Console.WriteLine("1) Create a server");
            Console.WriteLine("2) Connect to server");
            string answer = Console.ReadLine();

            ObjectTransport transport = null;

            
            if(answer == "1")
            {
                Console.WriteLine("Please specify an IP address");
                var ipAddress = Console.ReadLine();

                //Setup a TCP Server
                transport = ObjectTransport.Factory.CreateTCPServer(ipAddress, 8888);

                //Receive an object of type "Message"
                transport.Receive<Message>((client,received_message) => {

                        Console.WriteLine("{0} - {1}", client.IPAddress, received_message.Body);

                        //Send the received message to all other clients 
                        //except the client who sent the message
                        transport.Send(received_message)
                                 .ToAll(client)
                                 .Execute();
                    })
                     .Execute();
                Console.WriteLine("Created TCP server on port 8888");
            }

            if(answer == "2")
            {
                Console.WriteLine("Please specify an IP address");
                var ipAddress = Console.ReadLine();

                //Connect to TCP server
                transport = ObjectTransport.Factory.CreateTCPClient(ipAddress, 8888);

                //When client receives an object of type "Message" output to console.
                transport.Receive<Message>((client,received_Message) =>
                {
                    Console.WriteLine("{0} - {1}", client.IPAddress, received_Message.Body);
                })
                .Execute();
                Console.WriteLine("Connected to server.");
            }

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
