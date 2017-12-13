using System;
using System.IO;
using System.Text;

namespace OTransport.Serializer.protobuf
{
    public class ProtobufSerializer : ISerializer
    {
        public object Deserialize(string objectPayload, Type objectType)
        {
            byte[] byteArray = Encoding.UTF8.GetBytes(objectPayload);
            //byte[] byteArray = Encoding.ASCII.GetBytes(contents);
            MemoryStream stream = new MemoryStream(byteArray);
            return ProtoBuf.Serializer.Deserialize(objectType, stream);
        }

        public string Serialize(object obj)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                ProtoBuf.Serializer.Serialize(stream, obj);
                return System.Text.Encoding.UTF8.GetString(stream.ToArray());
            }
        }
    }
}
