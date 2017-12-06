using OT.Serializer.JSON;
using OTransport.Factory;

namespace OT.TCP
{
    public static class ObjectTransportAssemblyLine_JSONExtension
    {
        /// <summary>
        /// Create a TCP server. This network channel only supports reliable communication.
        /// </summary>
        /// <param name="ipAddress">the IP address to start the server on</param>
        /// <param name="port">the port to listen on</param>
        /// <returns></returns>
        public static ObjectTransportAssemblyLine UseJSONserialization(this ObjectTransportAssemblyLine objectTranposrtAssemblyLine)
        {
            var jsonSerialization = new JSONserializer();
            objectTranposrtAssemblyLine.SetSerializer(jsonSerialization);

            return objectTranposrtAssemblyLine;
        }
    }
}
