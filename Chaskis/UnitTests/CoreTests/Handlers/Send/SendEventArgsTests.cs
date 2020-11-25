//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using Chaskis.Core;
using Moq;
using NUnit.Framework;
using SethCS.Exceptions;

namespace Chaskis.UnitTests.CoreTests.Handlers.Send
{
    [TestFixture]
    public class SendEventArgsTests
    {
        // ---------------- Fields ----------------

        private const string command = "!Some Command";
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
            SendEventArgs uut = new SendEventArgs
            {
                Protocol = protocol,
                Server = server,
                Writer = mockWriter.Object,

                Command = command
            };

            DoRoundTripTest( uut );
        }

        [Test]
        public void XmlRoundTripWithInvalidCharsTest()
        {
            SendEventArgs uut = new SendEventArgs
            {
                Protocol = protocol,
                Server = server,
                Writer = mockWriter.Object,

                Command = $"\u0001{command}\u0001[0x01][]]["
            };

            DoRoundTripTest( uut );
        }

        [Test]
        public void InvalidXmlRootName()
        {
            string xmlString = $"<lol><server>{server}</server><protocol>{protocol}</protocol><command>{command}</command></lol>";

            Assert.Throws<ValidationException>(
                () => SendEventArgsExtensions.FromXml( xmlString, mockWriter.Object )
            );
        }

        [Test]
        public void MissingServerDuringXmlParsing()
        {
            string xmlString = $"<chaskis_send_event><protocol>{protocol}</protocol><command>{command}</command></chaskis_send_event>";

            Assert.Throws<ValidationException>(
                () => SendEventArgsExtensions.FromXml( xmlString, mockWriter.Object )
            );
        }

        [Test]
        public void MissingProtocolDuringXmlParsing()
        {
            string xmlString = $"<chaskis_send_event><server>{server}</server><command>{command}</command></chaskis_send_event>";

            Assert.Throws<ValidationException>(
                () => SendEventArgsExtensions.FromXml( xmlString, mockWriter.Object )
            );
        }

        [Test]
        public void MissingCommandDuringXmlParsing()
        {
            string xmlString = $"<chaskis_send_event><server>{server}</server><protocol>{protocol}</protocol></chaskis_send_event>";

            Assert.Throws<ValidationException>(
                () => SendEventArgsExtensions.FromXml( xmlString, mockWriter.Object )
            );
        }

        // ---------------- Test Helpers ----------------

        private void DoRoundTripTest( SendEventArgs uut )
        {
            string xmlString = uut.ToXml();
            SendEventArgs postXml = SendEventArgsExtensions.FromXml( xmlString, mockWriter.Object );

            Console.WriteLine( xmlString );

            Assert.AreEqual( uut.Server, postXml.Server );
            Assert.AreEqual( uut.Protocol, postXml.Protocol );
            Assert.AreEqual( uut.Command, postXml.Command );
           
            Assert.AreSame( uut.Writer, postXml.Writer );
        }
    }
}
