//
//          Copyright Seth Hendrick 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Chaskis.Core;
using NUnit.Framework;
using SethCS.Exceptions;

namespace Chaskis.UnitTests.CoreTests.Handlers.WatchdogFailed
{
    [TestFixture]
    public class WatchdogFailedEventArgsTests
    {
        // ---------------- Tests ----------------

        [Test]
        public void ConstructorTest()
        {
            const string server = "irc.somewhere.net";
            const ChaskisEventProtocol protocol = ChaskisEventProtocol.IRC;

            WatchdogFailedEventArgs uut = new WatchdogFailedEventArgs
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

            WatchdogFailedEventArgs uut = new WatchdogFailedEventArgs
            {
                Protocol = protocol,
                Server = server
            };
            string xmlString = uut.ToXml();
            WatchdogFailedEventArgs postXml = WatchdogFailedEventArgsExtensions.FromXml( xmlString );

            Assert.AreEqual( uut.Server, postXml.Server );
            Assert.AreEqual( uut.Protocol, postXml.Protocol );
        }

        [Test]
        public void InvalidXmlRootName()
        {
            const string xmlString = "<lol><server>irc.somewhere.net</server><protocol>IRC</protocol></lol>";

            Assert.Throws<ValidationException>(
                () => WatchdogFailedEventArgsExtensions.FromXml( xmlString )
            );
        }

        [Test]
        public void MissingServerDuringXmlParsing()
        {
            const string xmlString = "<chaskis_watchdogfailed_event><protocol>IRC</protocol></chaskis_watchdogfailed_event>";

            Assert.Throws<ValidationException>(
                () => WatchdogFailedEventArgsExtensions.FromXml( xmlString )
            );
        }

        [Test]
        public void MissingProtocolDuringXmlParsing()
        {
            const string xmlString = "<chaskis_watchdogfailed_event><server>irc.somewhere.net</server></chaskis_watchdogfailed_event>";

            Assert.Throws<ValidationException>(
                () => WatchdogFailedEventArgsExtensions.FromXml( xmlString )
            );
        }
    }
}
