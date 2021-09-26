//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Chaskis.Core;
using NUnit.Framework;
using SethCS.Exceptions;

namespace Chaskis.UnitTests.CoreTests.Handlers.Disconnecting
{
    [TestFixture]
    public sealed class DisconnectingEventArgsTests
    {
        // ---------------- Fields ----------------

        private const string server = "irc.somewhere.net";
        private const ChaskisEventProtocol protocol = ChaskisEventProtocol.IRC;

        // ---------------- Tests ----------------

        [Test]
        public void XmlRoundTripTest()
        {
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
            string xmlString = $"<lol><server>{server}</server><protocol>{protocol}</protocol></lol>";

            Assert.Throws<ValidationException>(
                () => DisconnectingEventArgsExtensions.FromXml( xmlString )
            );
        }

        [Test]
        public void MissingServerDuringXmlParsing()
        {
            string xmlString = $"<chaskis_disconnecting_event><protocol>{protocol}</protocol></chaskis_disconnecting_event>";

            Assert.Throws<ValidationException>(
                () => DisconnectingEventArgsExtensions.FromXml( xmlString )
            );
        }

        [Test]
        public void MissingProtocolDuringXmlParsing()
        {
            string xmlString = $"<chaskis_disconnecting_event><server>{server}</server></chaskis_disconnecting_event>";

            Assert.Throws<ValidationException>(
                () => DisconnectingEventArgsExtensions.FromXml( xmlString )
            );
        }
    }
}
