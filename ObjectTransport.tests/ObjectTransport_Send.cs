using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ObjectTransport.tests
{
    public class MockObjectToSend
    {
        public string Property1_string { get; set; }
        public int Property2_int { get; set; }
        public decimal Property3_decimal { get; set; }
    }
    [TestClass]
    public class ObjectTransport_Send
    {
        [TestMethod]
        public void SendExecute_ObjectWithProperties_ObjectJSONWithType()
        {
            //Arrange
            var sentJson = string.Empty;
            var networkChannel = new Mock<INetworkChannel>();
            networkChannel.Setup(m => m.Send(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
                .Callback<string, int, string>((address, port, json) => sentJson = json);

            MockObjectToSend sendObject = new MockObjectToSend();
            sendObject.Property1_string = "Test String";
            sendObject.Property2_int = 12;
            sendObject.Property3_decimal = 1.33M;

            //Act 
            ObjectTransport transport = new ObjectTransport(networkChannel.Object);
            transport.Send(sendObject)
                .To(new Client("1.1.1",123))
                     .Execute();


            while(sentJson == string.Empty) { }
            //Assert
            Assert.AreEqual("{type='',object={}}", sentJson);
        }
    }
}
