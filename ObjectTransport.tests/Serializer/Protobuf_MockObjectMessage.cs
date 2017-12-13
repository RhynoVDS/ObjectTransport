using ProtoBuf;

namespace OTransport.tests
{
    [ProtoContract]
    public class Protobuf_MockObjectMessage
    {
        [ProtoMember(1)]
        public string Property1_string { get; set; }
        [ProtoMember(2)]
        public int Property2_int { get; set; }
        [ProtoMember(3)]
        public decimal Property3_decimal { get; set; }
    }
}
