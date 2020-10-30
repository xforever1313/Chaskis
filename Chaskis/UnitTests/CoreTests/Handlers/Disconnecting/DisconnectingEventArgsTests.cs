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

namespace Chaskis.UnitTests.CoreTests.Handlers.Disconnecting
{
    [TestFixture]
    public class DisconnectingEventArgsTests
    {
        // ---------------- Tests ----------------

        [Test]
        public void ConstructorTest()
        {
            const string server = "irc.somewhere.net";
            const ChaskisEventProtocol protocol = ChaskisEventProtocol.IRC;

            DisconnectingEventArgs uut = new DisconnectingEventArgs
            {
                Protocol = protocol,
                Server = server
            };

            Assert.AreEqual( uut.Server, server );
            Assert.AreEqual( uut.Protocol, protocol );
        }

        [Test]
        public void XmlRoundTripTest()
        {
            const string server = "irc.somewhere.net";
            const ChaskisEventProtocol protocol = ChaskisEventProtocol.IRC;

            DisconnectingEventArgs uut = new DisconnectingEventArgs
            {
                Protocol = protocol,
                Server = server
            };
            string xmlString = uut.ToXml();
            DisconnectingEventArgs postXml = DisconnectingEventArgsExtensions.FromXml( xmlString );

            Assert.AreEqual( uut.Server, postXml.Server );
            Assert.AreEqual( uut.Protocol, postXml.Protocol );
        }

        [Test]
        public void InvalidXmlRootName()
        {
            const string xmlString = "<lol><server>irc.somewhere.net</server><protocol>IRC</protocol></lol>";

            Assert.Throws<ValidationException>(
                () => DisconnectingEventArgsExtensions.FromXml( xmlString )
            );
        }

        [Test]
        public void MissingServerDuringXmlParsing()
        {
            const string xmlString = "<chaskis_disconnecting_event><protocol>IRC</protocol></chaskis_disconnecting_event>";

            Assert.Throws<ValidationException>(
                () => DisconnectingEventArgsExtensions.FromXml( xmlString )
            );
        }

        [Test]
        public void MissingProtocolDuringXmlParsing()
        {
            const string xmlString = "<chaskis_disconnecting_event><server>irc.somewhere.net</server></chaskis_disconnecting_event>";
            Mock<IIrcWriter> mockWriter = new Mock<IIrcWriter>( MockBehavior.Strict );

            Assert.Throws<ValidationException>(
                () => DisconnectingEventArgsExtensions.FromXml( xmlString )
            );
        }
    }
}
