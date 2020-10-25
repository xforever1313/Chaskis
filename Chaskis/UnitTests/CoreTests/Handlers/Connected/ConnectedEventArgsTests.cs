//
//          Copyright Seth Hendrick 2020.
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
        // ---------------- Tests ----------------

        [Test]
        public void ConstructorTest()
        {
            const string server = "irc.somewhere.net";
            const ChaskisEventProtocol protocol = ChaskisEventProtocol.IRC;
            Mock<IIrcWriter> mockWriter = new Mock<IIrcWriter>( MockBehavior.Strict );

            ConnectedEventArgs uut = new ConnectedEventArgs
            {
                Protocol = protocol,
                Server = server,
                Writer = mockWriter.Object
            };

            Assert.AreSame( mockWriter.Object, uut.Writer );
            Assert.AreEqual( uut.Server, server );
            Assert.AreEqual( uut.Protocol, protocol );
        }

        [Test]
        public void XmlRoundTripTest()
        {
            const string server = "irc.somewhere.net";
            const ChaskisEventProtocol protocol = ChaskisEventProtocol.IRC;
            Mock<IIrcWriter> mockWriter = new Mock<IIrcWriter>( MockBehavior.Strict );

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
            const string xmlString = "<lol><server>irc.somewhere.net</server><protocol>IRC</protocol></lol>";
            Mock<IIrcWriter> mockWriter = new Mock<IIrcWriter>( MockBehavior.Strict );

            Assert.Throws<ValidationException>(
                () => ConnectedEventArgsExtensions.FromXml( xmlString, mockWriter.Object )
            );
        }

        [Test]
        public void MissingServerDuringXmlParsing()
        {
            const string xmlString = "<chaskis_connect_event><protocol>IRC</protocol></chaskis_connect_event>";
            Mock<IIrcWriter> mockWriter = new Mock<IIrcWriter>( MockBehavior.Strict );

            Assert.Throws<ValidationException>(
                () => ConnectedEventArgsExtensions.FromXml( xmlString, mockWriter.Object )
            );
        }

        [Test]
        public void MissingProtocolDuringXmlParsing()
        {
            const string xmlString = "<chaskis_connect_event><server>irc.somewhere.net</server></chaskis_connect_event>";
            Mock<IIrcWriter> mockWriter = new Mock<IIrcWriter>( MockBehavior.Strict );

            Assert.Throws<ValidationException>(
                () => ConnectedEventArgsExtensions.FromXml( xmlString, mockWriter.Object )
            );
        }
    }
}
