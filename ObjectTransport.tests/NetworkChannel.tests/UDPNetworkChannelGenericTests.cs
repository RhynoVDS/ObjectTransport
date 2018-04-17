using Microsoft.VisualStudio.TestTools.UnitTesting;
using OTransport;
using OTransport.NetworkChannel.TCP;
using OTransport.NetworkChannel.UDP;
using System;
using System.Collections.Generic;
using System.Text;

namespace OTranport
{
    [TestClass]
    public class UDPNetworkChannelGenericTests : INetworkChannelGenericTests
    {
        [TestInitialize]
        public void SetUpNetworkChannels()
        {
            SetUpNetworkChannels(new UDPClientChannel(), new UDPClientChannel(), new UDPServerChannel());
        }
    }
}
