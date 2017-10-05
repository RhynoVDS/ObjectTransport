namespace OTransport
{
    public class ReceivedConnection
    {
        public string Address { get; set; }
        public int Port { get; set; }
        
        public ReceivedConnection(string address,int port)
        {
            Address = address;
            Port = port;
        }
    }
}