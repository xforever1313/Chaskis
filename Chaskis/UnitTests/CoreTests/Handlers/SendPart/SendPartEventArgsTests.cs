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

namespace Chaskis.UnitTests.CoreTests.Handlers.SendPart
{
    [TestFixture]
    public class SendPartEventArgsTests
    {
        // ---------------- Fields ----------------

        private const string channel = "#somechannel";
        private const string reason = "Some Reason";
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
            SendPartEventArgs uut = new SendPartEventArgs
            {
                Protocol = protocol,
                Server = server,
                Writer = mockWriter.Object,

                Channel = channel,
                Reason = reason
            };

            DoRoundTripTest( uut );
        }

        [Test]
        public void XmlRoundTripWillNullReasonTest()
        {
            SendPartEventArgs uut = new SendPartEventArgs
            {
                Protocol = protocol,
                Server = server,
                Writer = mockWriter.Object,

                Channel = channel,
                Reason = null
            };

            DoRoundTripTest( uut );
        }

        [Test]
        public void InvalidXmlRootName()
        {
            string xmlString = $"<lol><server>{server}</server><protocol>{protocol}</protocol><channel>{channel}</channel><reason>{reason}</reason></lol>";

            Assert.Throws<ValidationException>(
                () => SendPartEventArgsExtensions.FromXml( xmlString, mockWriter.Object )
            );
        }

        [Test]
        public void MissingServerDuringXmlParsing()
        {
            string xmlString = $"<chaskis_sendpart_event><protocol>{protocol}</protocol><channel>{channel}</channel><reason>{reason}</reason></chaskis_sendpart_event>";

            Assert.Throws<ValidationException>(
                () => SendPartEventArgsExtensions.FromXml( xmlString, mockWriter.Object )
            );
        }

        [Test]
        public void MissingProtocolDuringXmlParsing()
        {
            string xmlString = $"<chaskis_sendpart_event><server>{server}</server><channel>{channel}</channel><reason>{reason}</reason></chaskis_sendpart_event>";

            Assert.Throws<ValidationException>(
                () => SendPartEventArgsExtensions.FromXml( xmlString, mockWriter.Object )
            );
        }

        [Test]
        public void MissingChannelDuringXmlParsing()
        {
            string xmlString = $"<chaskis_sendpart_event><server>{server}</server><protocol>{protocol}</protocol><reason>{reason}</reason></chaskis_sendpart_event>";

            Assert.Throws<ValidationException>(
                () => SendPartEventArgsExtensions.FromXml( xmlString, mockWriter.Object )
            );
        }

        [Test]
        public void MissingReasonDuringXmlParsing()
        {
            string xmlString = $"<chaskis_sendpart_event><server>{server}</server><protocol>{protocol}</protocol><channel>{channel}</channel></chaskis_sendpart_event>";

            Assert.Throws<ValidationException>(
                () => SendPartEventArgsExtensions.FromXml( xmlString, mockWriter.Object )
            );
        }

        // ---------------- Test Helpers ----------------

        private void DoRoundTripTest( SendPartEventArgs uut )
        {
            string xmlString = uut.ToXml();
            SendPartEventArgs postXml = SendPartEventArgsExtensions.FromXml( xmlString, mockWriter.Object );

            Console.WriteLine( xmlString );

            Assert.AreEqual( uut.Server, postXml.Server );
            Assert.AreEqual( uut.Protocol, postXml.Protocol );
            Assert.AreEqual( uut.Channel, postXml.Channel );
            
            // Should never be null.  Should always be empty string.
            if( uut.Reason == null )
            {
                Assert.AreEqual( string.Empty, postXml.Reason );
            }
            else
            {
                Assert.AreEqual( uut.Reason, postXml.Reason );
            }
            Assert.AreSame( uut.Writer, postXml.Writer );
        }
    }
}
