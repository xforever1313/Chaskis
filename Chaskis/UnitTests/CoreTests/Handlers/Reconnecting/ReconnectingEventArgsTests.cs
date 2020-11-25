//
//          Copyright Seth Hendrick 2016-2020.
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
        // ---------------- Fields ----------------
        
        private const string server = "irc.somewhere.net";
        private const ChaskisEventProtocol protocol = ChaskisEventProtocol.IRC;

        // ---------------- Tests ----------------

        [Test]
        public void XmlRoundTripTest()
        {
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
            string xmlString = $"<lol><server>{server}</server><protocol>{protocol}</protocol></lol>";

            Assert.Throws<ValidationException>(
                () => ReconnectingEventArgsExtensions.FromXml( xmlString )
            );
        }

        [Test]
        public void MissingServerDuringXmlParsing()
        {
            string xmlString = $"<chaskis_reconnecting_event><protocol>{protocol}</protocol></chaskis_reconnecting_event>";

            Assert.Throws<ValidationException>(
                () => ReconnectingEventArgsExtensions.FromXml( xmlString )
            );
        }

        [Test]
        public void MissingProtocolDuringXmlParsing()
        {
            string xmlString = $"<chaskis_reconnecting_event><server>{server}</server></chaskis_reconnecting_event>";

            Assert.Throws<ValidationException>(
                () => ReconnectingEventArgsExtensions.FromXml( xmlString )
            );
        }
    }
}
