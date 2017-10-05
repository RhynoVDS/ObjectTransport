namespace OTransport
{
    public class ReceivedMessage
    {
        public Client From { get; }
        public string Message { get; }

        public ReceivedMessage(Client client, string message)
        {
            From = client;
            Message = message;
        }
    }
}