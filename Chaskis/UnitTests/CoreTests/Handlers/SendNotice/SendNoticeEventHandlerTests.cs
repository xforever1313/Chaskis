//
//          Copyright Seth Hendrick 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Chaskis.Core;
using Chaskis.UnitTests.Common;
using Moq;
using NUnit.Framework;
using SethCS.Exceptions;

namespace Chaskis.UnitTests.CoreTests.Handlers.SendNotice
{
    [TestFixture]
    public class SendNoticeEventHandlerTests
    {
        // ---------------- Fields ----------------

        private SendNoticeEventHandler uut;

        private IrcConfig ircConfig;

        private Mock<IIrcWriter> ircWriter;

        private SendNoticeEventArgs responseReceived;

        private const string channel = "#somechannel";
        private const string message = "Some Message";

        // ---------------- Setup / Teardown ----------------

        [SetUp]
        public void TestSetup()
        {
            this.ircConfig = TestHelpers.GetTestIrcConfig();
            this.ircWriter = new Mock<IIrcWriter>( MockBehavior.Strict );
            this.responseReceived = null;

            SendNoticeEventConfig config = new SendNoticeEventConfig
            {
                LineAction = SendNoticeFunction
            };

            this.uut = new SendNoticeEventHandler( config );
        }

        [TearDown]
        public void TestTeardown()
        {
        }

        // ---------------- Tests ----------------

        [Test]
        public void InvalidConfigTest()
        {
            Assert.Throws<ListedValidationException>(
                () => new SendNoticeEventHandler( new SendNoticeEventConfig() )
            );
        }

        [Test]
        public void ConstructionTest()
        {
            // Keep Handling should be true by default.
            Assert.IsTrue( this.uut.KeepHandling );
        }

        [Test]
        public void SuccessTest()
        {
            SendNoticeEventArgs expectedArgs = new SendNoticeEventArgs
            {
                Protocol = ChaskisEventProtocol.IRC,
                Server = "server",
                Writer = this.ircWriter.Object,

                ChannelOrUser = channel,
                Message = message
            };

            HandlerArgs handlerArgs = ConstructArgs( expectedArgs.ToXml() );

            this.uut.HandleEvent( handlerArgs );

            Assert.IsNotNull( this.responseReceived );
            Assert.AreEqual( expectedArgs.Server, this.responseReceived.Server );
            Assert.AreEqual( expectedArgs.Protocol, this.responseReceived.Protocol );
            Assert.AreEqual( expectedArgs.ChannelOrUser, this.responseReceived.ChannelOrUser );
            Assert.AreEqual( expectedArgs.Message, this.responseReceived.Message );
            Assert.AreSame( expectedArgs.Writer, this.responseReceived.Writer );
        }

        [Test]
        public void FailureTest()
        {
            HandlerArgs handlerArgs = ConstructArgs( "lol" );

            this.uut.HandleEvent( handlerArgs );

            Assert.IsNull( this.responseReceived );
        }

        // ---------------- Test Helpers ----------------

        private void SendNoticeFunction( SendNoticeEventArgs args )
        {
            this.responseReceived = args;
        }

        private HandlerArgs ConstructArgs( string line )
        {
            HandlerArgs args = new HandlerArgs
            {
                Line = line,
                IrcWriter = this.ircWriter.Object,
                IrcConfig = this.ircConfig
            };

            return args;
        }
    }
}
