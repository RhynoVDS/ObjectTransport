using System;
using System.Collections;
using System.Collections.Generic;

namespace OTransport
{
    internal class QueuedMessage
    {
        public Dictionary<Type, Delegate> resonseType_to_actionMatch { get; set; } = new Dictionary<Type, Delegate>();
        public int TimeOutInSeconds { get; set; } = 15;
        public object ObjectToSend { get; set; }
        public string Token { get; set; }
        public Client[] sendTo { get; set; }
        public Delegate TimeOutFunction { get; set; }

        public bool SendReliable { get; set; }
    }
}