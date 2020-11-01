//
//          Copyright Seth Hendrick 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using Chaskis.Core;
using Moq;
using NUnit.Framework;
using SethCS.Exceptions;

namespace Chaskis.UnitTests.CoreTests.Handlers.SendJoin
{
    [TestFixture]
    public class SendJoinEventArgsTests
    {
        // ---------------- Fields ----------------

        private const string channel = "#somechannel";
        private const string server = "irc.somewhere.net";
        private const ChaskisEventProtocol protocol = ChaskisEventProtocol.IRC;

        private Mock<IIrcWriter> mockWriter;

        // ---------------- Setup / Teardown ----------------

        [SetUp]
        public void TestSetup()
        {
            this.mockWriter = new Mock<IIrcWriter>( MockBehavior.Strict );
        }

        [TearDown]
        public void TestTeardown()
        {
            this.mockWriter = null;
        }

        // ---------------- Tests ----------------

        [Test]
        public void XmlRoundTripTest()
        {
            SendJoinEventArgs uut = new SendJoinEventArgs
            {
                Protocol = protocol,
                Server = server,
                Writer = mockWriter.Object,

                Channel = channel
            };

            DoRoundTripTest( uut );
        }

        [Test]
        public void XmlRoundTripWillNullReasonTest()
        {
            SendJoinEventArgs uut = new SendJoinEventArgs
            {
                Protocol = protocol,
                Server = server,
                Writer = mockWriter.Object,

                Channel = channel
            };

            DoRoundTripTest( uut );
        }

        [Test]
        public void InvalidXmlRootName()
        {
            string xmlString = $"<lol><server>{server}</server><protocol>{protocol}</protocol><channel>{channel}</channel></lol>";

            Assert.Throws<ValidationException>(
                () => SendJoinEventArgsExtensions.FromXml( xmlString, mockWriter.Object )
            );
        }

        [Test]
        public void MissingServerDuringXmlParsing()
        {
            string xmlString = $"<chaskis_sendjoin_event><protocol>{protocol}</protocol><channel>{channel}</channel></chaskis_sendjoin_event>";

            Assert.Throws<ValidationException>(
                () => SendJoinEventArgsExtensions.FromXml( xmlString, mockWriter.Object )
            );
        }

        [Test]
        public void MissingProtocolDuringXmlParsing()
        {
            string xmlString = $"<chaskis_sendjoin_event><server>{server}</server><channel>{channel}</channel></chaskis_sendjoin_event>";

            Assert.Throws<ValidationException>(
                () => SendJoinEventArgsExtensions.FromXml( xmlString, mockWriter.Object )
            );
        }

        [Test]
        public void MissingChannelDuringXmlParsing()
        {
            string xmlString = $"<chaskis_sendjoin_event><server>{server}</server><protocol>{protocol}</protocol></chaskis_sendjoin_event>";

            Assert.Throws<ValidationException>(
                () => SendJoinEventArgsExtensions.FromXml( xmlString, mockWriter.Object )
            );
        }

        // ---------------- Test Helpers ----------------

        private void DoRoundTripTest( SendJoinEventArgs uut )
        {
            string xmlString = uut.ToXml();
            SendJoinEventArgs postXml = SendJoinEventArgsExtensions.FromXml( xmlString, mockWriter.Object );

            Console.WriteLine( xmlString );

            Assert.AreEqual( uut.Server, postXml.Server );
            Assert.AreEqual( uut.Protocol, postXml.Protocol );
            Assert.AreEqual( uut.Channel, postXml.Channel );
           
            Assert.AreSame( uut.Writer, postXml.Writer );
        }
    }
}
