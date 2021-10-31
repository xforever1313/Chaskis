//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using Chaskis.Core;
using Moq;
using NUnit.Framework;
using SethCS.Exceptions;

namespace Chaskis.UnitTests.CoreTests.Handlers.SendCtcpVersion
{
    [TestFixture]
    public sealed class SendCtcpVersionEventArgsTests
    {
        // ---------------- Fields ----------------

        private const string channel = "#somechannel";
        private const string message = "Some Message";
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
            SendCtcpVersionEventArgs uut = new SendCtcpVersionEventArgs
            {
                Protocol = protocol,
                Server = server,
                Writer = mockWriter.Object,

                ChannelOrUser = channel,
                Message = message
            };

            DoRoundTripTest( uut );
        }

        [Test]
        public void InvalidXmlRootName()
        {
            string xmlString = $"<lol><server>{server}</server><protocol>{protocol}</protocol><channel>{channel}</channel><message>{message}</message></lol>";

            Assert.Throws<ValidationException>(
                () => SendCtcpVersionEventArgsExtensions.FromXml( xmlString, mockWriter.Object )
            );
        }

        [Test]
        public void MissingServerDuringXmlParsing()
        {
            string xmlString = $"<chaskis_sendctcpversion_event><protocol>{protocol}</protocol><channel>{channel}</channel><message>{message}</message></chaskis_sendctcpversion_event>";

            Assert.Throws<ValidationException>(
                () => SendCtcpVersionEventArgsExtensions.FromXml( xmlString, mockWriter.Object )
            );
        }

        [Test]
        public void MissingProtocolDuringXmlParsing()
        {
            string xmlString = $"<chaskis_sendctcpversion_event><server>{server}</server><channel>{channel}</channel><message>{message}</message></chaskis_sendctcpversion_event>";

            Assert.Throws<ValidationException>(
                () => SendCtcpVersionEventArgsExtensions.FromXml( xmlString, mockWriter.Object )
            );
        }

        [Test]
        public void MissingChannelDuringXmlParsing()
        {
            string xmlString = $"<chaskis_sendctcpversion_event><server>{server}</server><protocol>{protocol}</protocol><message>{message}</message></chaskis_sendctcpversion_event>";

            Assert.Throws<ValidationException>(
                () => SendCtcpVersionEventArgsExtensions.FromXml( xmlString, mockWriter.Object )
            );
        }

        [Test]
        public void MissingMessageDuringXmlParsing()
        {
            string xmlString = $"<chaskis_sendctcpversion_event><server>{server}</server><protocol>{protocol}</protocol><channel>{channel}</channel></chaskis_sendctcpversion_event>";

            Assert.Throws<ValidationException>(
                () => SendCtcpVersionEventArgsExtensions.FromXml( xmlString, mockWriter.Object )
            );
        }

        // ---------------- Test Helpers ----------------

        private void DoRoundTripTest( SendCtcpVersionEventArgs uut )
        {
            string xmlString = uut.ToXml();
            SendCtcpVersionEventArgs postXml = SendCtcpVersionEventArgsExtensions.FromXml( xmlString, mockWriter.Object );

            Console.WriteLine( xmlString );

            Assert.AreEqual( uut.Server, postXml.Server );
            Assert.AreEqual( uut.Protocol, postXml.Protocol );
            Assert.AreEqual( uut.ChannelOrUser, postXml.ChannelOrUser );
            Assert.AreEqual( uut.Message, postXml.Message );
            Assert.AreSame( uut.Writer, postXml.Writer );
        }
    }
}
