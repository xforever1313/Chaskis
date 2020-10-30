//
//          Copyright Seth Hendrick 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Chaskis.Core;
using NUnit.Framework;
using SethCS.Exceptions;

namespace Chaskis.UnitTests.CoreTests.Handlers.Reconnecting
{
    [TestFixture]
    public class ReconnectingEventArgsTests
    {
        // ---------------- Tests ----------------

        [Test]
        public void ConstructorTest()
        {
            const string server = "irc.somewhere.net";
            const ChaskisEventProtocol protocol = ChaskisEventProtocol.IRC;

            ReconnectingEventArgs uut = new ReconnectingEventArgs
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

            ReconnectingEventArgs uut = new ReconnectingEventArgs
            {
                Protocol = protocol,
                Server = server
            };
            string xmlString = uut.ToXml();
            ReconnectingEventArgs postXml = ReconnectingEventArgsExtensions.FromXml( xmlString );

            Assert.AreEqual( uut.Server, postXml.Server );
            Assert.AreEqual( uut.Protocol, postXml.Protocol );
        }

        [Test]
        public void InvalidXmlRootName()
        {
            const string xmlString = "<lol><server>irc.somewhere.net</server><protocol>IRC</protocol></lol>";

            Assert.Throws<ValidationException>(
                () => ReconnectingEventArgsExtensions.FromXml( xmlString )
            );
        }

        [Test]
        public void MissingServerDuringXmlParsing()
        {
            const string xmlString = "<chaskis_Reconnecting_event><protocol>IRC</protocol></chaskis_Reconnecting_event>";

            Assert.Throws<ValidationException>(
                () => ReconnectingEventArgsExtensions.FromXml( xmlString )
            );
        }

        [Test]
        public void MissingProtocolDuringXmlParsing()
        {
            const string xmlString = "<chaskis_Reconnecting_event><server>irc.somewhere.net</server></chaskis_Reconnecting_event>";

            Assert.Throws<ValidationException>(
                () => ReconnectingEventArgsExtensions.FromXml( xmlString )
            );
        }
    }
}
