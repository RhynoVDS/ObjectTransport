using System;
using System.Collections.Generic;
using System.Text;

namespace OTransport
{
    internal class MessageResponseHandle
    {
        public MessageResponseHandle(QueuedMessage sent)
        {
            sentMessage = sent;
        }

        public List<Client> ClientsToRespond = new List<Client>();

        public QueuedMessage sentMessage { get; set; }
        public int SecondsPassed { get; set; } = 0;
    }
}
