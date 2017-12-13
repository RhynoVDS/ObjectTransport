using Newtonsoft.Json;
using OTransport.Serializer;
using System;

namespace OTransport.Serializer.JSON
{
    public class JSONserializer : ISerializer
    {

        public object Deserialize(string objectPayload, Type objectType)
        {
            return JsonConvert.DeserializeObject(objectPayload,objectType);
        }

        public string Serialize(object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }
    }
}
