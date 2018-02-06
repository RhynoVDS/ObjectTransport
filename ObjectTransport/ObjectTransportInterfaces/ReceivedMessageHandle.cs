using System;
using System.Collections.Generic;
using System.Text;

namespace OTransport
{
    class ReceivedMessageHandle
    {
        public Type RecieveType { get; set; }
        public Delegate ReceiveAction { get; set; }
        public Delegate ReplyFunction { get; set; } 
    }
}
