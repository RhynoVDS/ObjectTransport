using Microsoft.VisualStudio.TestTools.UnitTesting;
using OTransport;
using OTransport.NetworkChannel.TCP;
using System;
using System.Collections.Generic;
using System.Text;

namespace OTranport
{
    [TestClass]
    public class TCPNetworkChannelGenericTests : INetworkChannelGenericTests
    {
        [TestInitialize]
        public void SetUpNetworkChannels()
        {
            SetUpNetworkChannels(new TCPClientChannel(), new TCPServerChannel());
        }
    }
}
