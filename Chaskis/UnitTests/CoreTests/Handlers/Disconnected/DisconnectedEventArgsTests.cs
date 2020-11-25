//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Chaskis.Core;
using NUnit.Framework;
using SethCS.Exceptions;

namespace Chaskis.UnitTests.CoreTests.Handlers.Disconnected
{
    [TestFixture]
    public class DisconnectedEventArgsTests
    {
        // ---------------- Fields ----------------

        private const string server = "irc.somewhere.net";
        private const ChaskisEventProtocol protocol = ChaskisEventProtocol.IRC;

        // ---------------- Tests ----------------

        [Test]
        public void XmlRoundTripTest()
        {
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
            string xmlString = $"<lol><server>{server}</server><protocol>{protocol}</protocol></lol>";

            Assert.Throws<ValidationException>(
                () => DisconnectedEventArgsExtensions.FromXml( xmlString )
            );
        }

        [Test]
        public void MissingServerDuringXmlParsing()
        {
            string xmlString = $"<chaskis_disconnected_event><protocol>{protocol}</protocol></chaskis_disconnected_event>";

            Assert.Throws<ValidationException>(
                () => DisconnectedEventArgsExtensions.FromXml( xmlString )
            );
        }

        [Test]
        public void MissingProtocolDuringXmlParsing()
        {
            string xmlString = $"<chaskis_disconnected_event><server>{server}</server></chaskis_disconnected_event>";

            Assert.Throws<ValidationException>(
                () => DisconnectedEventArgsExtensions.FromXml( xmlString )
            );
        }
    }
}
