//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Chaskis.Core;
using Moq;
using NUnit.Framework;
using SethCS.Exceptions;

namespace Chaskis.UnitTests.CoreTests.Handlers.FinishedJoiningChannels
{
    [TestFixture]
    public class FinishedJoiningChannelsEventArgsTests
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
            FinishedJoiningChannelsEventArgs uut = new FinishedJoiningChannelsEventArgs
            {
                Protocol = protocol,
                Server = server,
                Writer = mockWriter.Object
            };
            string xmlString = uut.ToXml();
            FinishedJoiningChannelsEventArgs postXml = FinishedJoiningChannelsEventArgsExtensions.FromXml( xmlString, mockWriter.Object );

            Assert.AreEqual( uut.Server, postXml.Server );
            Assert.AreEqual( uut.Protocol, postXml.Protocol );
            Assert.AreSame( uut.Writer, postXml.Writer );
        }

        [Test]
        public void InvalidXmlRootName()
        {
            string xmlString = $"<lol><server>{server}</server><protocol>{protocol}</protocol></lol>";
            Mock<IIrcWriter> mockWriter = new Mock<IIrcWriter>( MockBehavior.Strict );

            Assert.Throws<ValidationException>(
                () => FinishedJoiningChannelsEventArgsExtensions.FromXml( xmlString, mockWriter.Object )
            );
        }

        [Test]
        public void MissingServerDuringXmlParsing()
        {
            string xmlString = $"<chaskis_finishedjoiningchannels_event><protocol>{protocol}</protocol></chaskis_finishedjoiningchannels_event>";
            Mock<IIrcWriter> mockWriter = new Mock<IIrcWriter>( MockBehavior.Strict );

            Assert.Throws<ValidationException>(
                () => FinishedJoiningChannelsEventArgsExtensions.FromXml( xmlString, mockWriter.Object )
            );
        }

        [Test]
        public void MissingProtocolDuringXmlParsing()
        {
            string xmlString = $"<chaskis_finishedjoiningchannels_event><server>{server}</server></chaskis_finishedjoiningchannels_event>";
            Mock<IIrcWriter> mockWriter = new Mock<IIrcWriter>( MockBehavior.Strict );

            Assert.Throws<ValidationException>(
                () => FinishedJoiningChannelsEventArgsExtensions.FromXml( xmlString, mockWriter.Object )
            );
        }
    }
}
