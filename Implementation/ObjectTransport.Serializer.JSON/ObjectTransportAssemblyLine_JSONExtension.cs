using OTransport.Serializer.JSON;
using OTransport.Factory;

namespace OTransport.Serializer.JSON
{
    public static class ObjectTransportAssemblyLine_JSONExtension
    {
        /// <summary>
        /// Use Json serialization to serialize objects
        /// </summary>
        /// <returns></returns>
        public static ObjectTransportAssemblyLine UseJSONserialization(this ObjectTransportAssemblyLine objectTranposrtAssemblyLine)
        {
            var jsonSerialization = new JSONserializer();
            objectTranposrtAssemblyLine.SetSerializer(jsonSerialization);

            return objectTranposrtAssemblyLine;
        }
    }
}
