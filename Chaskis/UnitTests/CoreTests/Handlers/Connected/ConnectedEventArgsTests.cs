//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Chaskis.Core;
using Moq;
using NUnit.Framework;
using SethCS.Exceptions;

namespace Chaskis.UnitTests.CoreTests.Handlers.Connected
{
    [TestFixture]
    public class ConnectedEventArgsTests
    {
        // ---------------- Fields ----------------

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
            ConnectedEventArgs uut = new ConnectedEventArgs
            {
                Protocol = protocol,
                Server = server,
                Writer = mockWriter.Object
            };
            string xmlString = uut.ToXml();
            ConnectedEventArgs postXml = ConnectedEventArgsExtensions.FromXml( xmlString, mockWriter.Object );

            Assert.AreEqual( uut.Server, postXml.Server );
            Assert.AreEqual( uut.Protocol, postXml.Protocol );
            Assert.AreSame( uut.Writer, postXml.Writer );
        }

        [Test]
        public void InvalidXmlRootName()
        {
            string xmlString = $"<lol><server>{server}</server><protocol>{protocol}</protocol></lol>";

            Assert.Throws<ValidationException>(
                () => ConnectedEventArgsExtensions.FromXml( xmlString, mockWriter.Object )
            );
        }

        [Test]
        public void MissingServerDuringXmlParsing()
        {
            string xmlString = $"<chaskis_connect_event><protocol>{protocol}</protocol></chaskis_connect_event>";

            Assert.Throws<ValidationException>(
                () => ConnectedEventArgsExtensions.FromXml( xmlString, mockWriter.Object )
            );
        }

        [Test]
        public void MissingProtocolDuringXmlParsing()
        {
            string xmlString = $"<chaskis_connect_event><server>{server}</server></chaskis_connect_event>";

            Assert.Throws<ValidationException>(
                () => ConnectedEventArgsExtensions.FromXml( xmlString, mockWriter.Object )
            );
        }
    }
}
