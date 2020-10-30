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

namespace Chaskis.UnitTests.CoreTests.Handlers.Disconnected
{
    [TestFixture]
    public class DisconnectedEventArgsTests
    {
        // ---------------- Tests ----------------

        [Test]
        public void ConstructorTest()
        {
            const string server = "irc.somewhere.net";
            const ChaskisEventProtocol protocol = ChaskisEventProtocol.IRC;

            DisconnectedEventArgs uut = new DisconnectedEventArgs
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

            DisconnectedEventArgs uut = new DisconnectedEventArgs
            {
                Protocol = protocol,
                Server = server
            };
            string xmlString = uut.ToXml();
            DisconnectedEventArgs postXml = DisconnectedEventArgsExtensions.FromXml( xmlString );

            Assert.AreEqual( uut.Server, postXml.Server );
            Assert.AreEqual( uut.Protocol, postXml.Protocol );
        }

        [Test]
        public void InvalidXmlRootName()
        {
            const string xmlString = "<lol><server>irc.somewhere.net</server><protocol>IRC</protocol></lol>";

            Assert.Throws<ValidationException>(
                () => DisconnectedEventArgsExtensions.FromXml( xmlString )
            );
        }

        [Test]
        public void MissingServerDuringXmlParsing()
        {
            const string xmlString = "<chaskis_disconnected_event><protocol>IRC</protocol></chaskis_disconnected_event>";

            Assert.Throws<ValidationException>(
                () => DisconnectedEventArgsExtensions.FromXml( xmlString )
            );
        }

        [Test]
        public void MissingProtocolDuringXmlParsing()
        {
            const string xmlString = "<chaskis_disconnected_event><server>irc.somewhere.net</server></chaskis_disconnected_event>";

            Assert.Throws<ValidationException>(
                () => DisconnectedEventArgsExtensions.FromXml( xmlString )
            );
        }
    }
}
