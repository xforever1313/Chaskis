//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Chaskis.Core;
using Chaskis.UnitTests.Common;
using Moq;
using NUnit.Framework;
using SethCS.Exceptions;

namespace Chaskis.UnitTests.CoreTests.Handlers.CtcpVersion
{
    [TestFixture]
    public sealed class CtcpVersionHandlerTests
    {
        // ---------------- Fields ----------------

        private IrcConfig ircConfig;

        private Mock<IIrcWriter> ircWriter;

        private CtcpVersionHandlerArgs responseReceived;

        private const string remoteUser = "remoteuser";

        // ---------------- Setup / Teardown ----------------

        [SetUp]
        public void TestSetup()
        {
            this.ircConfig = TestHelpers.GetTestIrcConfig();
            this.ircWriter = new Mock<IIrcWriter>( MockBehavior.Strict );
            this.responseReceived = null;
        }

        // ---------------- Tests ----------------

        /// <summary>
        /// Ensures that if a bad config is passed in, we throw an exception.
        /// </summary>
        [Test]
        public void InvalidConfigTest()
        {
            Assert.Throws<ListedValidationException>(
                () => new CtcpVersionHandler( new CtcpVersionHandlerConfig() )
            );
        }

        [Test]
        public void HandleVersionCmdNoArgsTest()
        {
            CtcpVersionHandlerConfig config = new CtcpVersionHandlerConfig
            {
                LineAction = this.MessageFunction,
                RespondToSelf = false
            };

            CtcpVersionHandler uut = new CtcpVersionHandler(
                config
            );

            string ircString = TestHelpers.ConstructCtcpVersionString( remoteUser, this.ircConfig.Nick );
            uut.HandleEvent( this.ConstructArgs( ircString ) );

            Assert.IsNotNull( this.responseReceived );
            Assert.AreEqual( remoteUser, this.responseReceived.Channel );
            Assert.AreEqual( remoteUser, this.responseReceived.User );
            Assert.AreEqual( string.Empty, this.responseReceived.Message );
        }

        [Test]
        public void HandleVersionCmdWithArgsTest()
        {
            CtcpVersionHandlerConfig config = new CtcpVersionHandlerConfig
            {
                LineAction = this.MessageFunction,
                RespondToSelf = false
            };

            CtcpVersionHandler uut = new CtcpVersionHandler(
                config
            );

            const string args = "Hello There!";
            string ircString = TestHelpers.ConstructCtcpVersionString( remoteUser, this.ircConfig.Nick, args );
            uut.HandleEvent( this.ConstructArgs( ircString ) );

            Assert.IsNotNull( this.responseReceived );
            Assert.AreEqual( remoteUser, this.responseReceived.Channel );
            Assert.AreEqual( remoteUser, this.responseReceived.User );
            Assert.AreEqual( args, this.responseReceived.Message );
        }

        /// <summary>
        /// Ensures if we get a PRIVMSG that is not a VERSION, we ignore it.
        /// </summary>
        [Test]
        public void IgnoreMessageTest()
        {
            const string expectedMessage = "!bot help";

            CtcpVersionHandlerConfig config = new CtcpVersionHandlerConfig
            {
                LineAction = this.MessageFunction,
                RespondToSelf = false
            };

            CtcpVersionHandler uut = new CtcpVersionHandler(
                config
            );

            string ircString = TestHelpers.ConstructMessageString( remoteUser, this.ircConfig.Nick, expectedMessage );
            uut.HandleEvent( this.ConstructArgs( ircString ) );

            Assert.IsNull( this.responseReceived );
        }

        /// <summary>
        /// Ensures if we get a CTCP Ping that is not a VERSION, we ignore it.
        /// </summary>
        [Test]
        public void IgnoreCtcpPingTest()
        {
            const string expectedMessage = "\u0001PING 1234567890\u0001";

            CtcpVersionHandlerConfig config = new CtcpVersionHandlerConfig
            {
                LineAction = this.MessageFunction,
                RespondToSelf = false
            };

            CtcpVersionHandler uut = new CtcpVersionHandler(
                config
            );

            string ircString = TestHelpers.ConstructMessageString( remoteUser, this.ircConfig.Nick, expectedMessage );
            uut.HandleEvent( this.ConstructArgs( ircString ) );

            Assert.IsNull( this.responseReceived );
        }

        // ---------------- Test Helpers ----------------

        private void MessageFunction( CtcpVersionHandlerArgs args )
        {
            Assert.AreSame( this.ircWriter.Object, args.Writer );
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
