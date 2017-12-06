using System;
using System.Collections.Generic;
using System.Text;

namespace OTransport.Serializer
{
    public interface ISerializer
    {
        string Serialize(object obj);
        object Deserialize(string objectPayload, Type objectType);
    }
}
