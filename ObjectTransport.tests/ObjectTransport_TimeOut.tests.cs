using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Text.RegularExpressions;
using Test;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using OTransport.Test.Utilities;

namespace OTransport.tests
{
    [TestClass]
    public class ObjectTransport_TimeOut
    {
        [TestMethod]
        public void TimeOut_TimeOutFunctionSet_TimeOutFunctionCalled()
        {
            //Arrange
            var client = new Client("10.0.0.1",123);
            Client[] clientsWhoDidNotRespond = null;
            MockObjectMessage messageThatTimedOut = null;

            var sentPayload = string.Empty;
            var networkChannel = MockNetworkChannelFactory.GetMockedNetworkChannel()
                                               .OnSendHandle((Client, payload) => sentPayload = payload);

            MockObjectMessage sendObject = new MockObjectMessage();
            sendObject.Property1_string = "Test String";
            sendObject.Property2_int = 12;
            sendObject.Property3_decimal = 1.33M;

            //Act 
            ObjectTransport transport = TestObjectTransportFactory.CreateNewObjectTransport(networkChannel);
            networkChannel.SimulateClientConnect(client);

            transport.Send(sendObject)
                     .Response<MockObjectMessage>(o => { })
                     .SetTimeOut(1)
                     .OnTimeOut(
                        (c,o)=> {
                             clientsWhoDidNotRespond = c;
                             messageThatTimedOut = o;
                         })
                     .Execute();

            Utilities.WaitFor(ref clientsWhoDidNotRespond);

            //Assert
            Assert.AreEqual(1,clientsWhoDidNotRespond.Length);
            Assert.AreEqual(sendObject,messageThatTimedOut);
        }

        [TestMethod]
        public void TimeOut_MessageSentToMultipleClientsWithSomeResponding_TimeOutFunctionReturnsClientsWhoDidNotRespond()
        {
            //Arrange
            var client1 = new Client("10.0.0.1",123);
            var client2 = new Client("10.0.0.2",123);
            var client3 = new Client("10.0.0.3",123);

            Client[] clientsWhoDidNotRespond = null;
            MockObjectMessage messageThatTimedOut = null;

            var sentPayload = string.Empty;
            var networkChannel = MockNetworkChannelFactory.GetMockedNetworkChannel()
                                               .OnSendHandle((Client, payload) => sentPayload = payload);

            MockObjectMessage sendObject = new MockObjectMessage();
            sendObject.Property1_string = "Test String";
            sendObject.Property2_int = 12;
            sendObject.Property3_decimal = 1.33M;

            ObjectTransport transport = TestObjectTransportFactory.CreateNewObjectTransport(networkChannel);
            networkChannel.SimulateClientConnect(client1);
            networkChannel.SimulateClientConnect(client2);
            networkChannel.SimulateClientConnect(client3);

            //Act 
            transport.Send(sendObject)
                     .Response<MockObjectMessage>(o => { })
                     .SetTimeOut(2)
                     .OnTimeOut(
                        (c,o)=> {
                             clientsWhoDidNotRespond = c;
                             messageThatTimedOut = o;
                         })
                     .ToAll()
                     .Execute();

            //Echo back the message
            networkChannel.SimulateClientResponse(client2, sentPayload);

            Utilities.WaitFor(ref messageThatTimedOut);

            //Assert
            Assert.AreEqual(2,clientsWhoDidNotRespond.Length);
            Assert.IsTrue(clientsWhoDidNotRespond.Contains(client1));
            Assert.IsTrue(clientsWhoDidNotRespond.Contains(client3));
            Assert.AreEqual(sendObject,messageThatTimedOut);
        }
    }
}
