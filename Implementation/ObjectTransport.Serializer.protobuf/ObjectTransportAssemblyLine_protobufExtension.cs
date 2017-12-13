using OTransport.Factory;
using OTransport.Serializer.protobuf;

namespace OTransport.Serializer.protobuf
{
    public static class ObjectTransportAssemblyLine_protobufExtension
    {
        /// <summary>
        /// Use Protobuf Serialization to serialize objects
        /// </summary>
        /// <returns></returns>
        public static ObjectTransportAssemblyLine UseProtobufSerialization(this ObjectTransportAssemblyLine objectTranposrtAssemblyLine)
        {
            var protobufSerialization = new ProtobufSerializer();
            objectTranposrtAssemblyLine.SetSerializer(protobufSerialization);

            return objectTranposrtAssemblyLine;
        }
    }
}
