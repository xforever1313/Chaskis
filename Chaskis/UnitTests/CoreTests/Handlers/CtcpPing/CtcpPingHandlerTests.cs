//
//          Copyright Seth Hendrick 2016-2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;
using Chaskis.Core;
using Chaskis.UnitTests.Common;
using Moq;
using NUnit.Framework;
using SethCS.Exceptions;

namespace Chaskis.UnitTests.CoreTests.Handlers.CtcpPing
{
    [TestFixture]
    public class CtcpPingHandlerTests
    {
        // ---------------- Fields ----------------

        private IrcConfig ircConfig;

        private Mock<IIrcWriter> ircWriter;

        private CtcpPingHandlerArgs responseReceived;

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
                () => new CtcpPingHandler( new CtcpPingHandlerConfig() )
            );
        }

        /// <summary>
        /// Ensures that the class is created correctly.
        /// </summary>
        [Test]
        public void ConstructionTest()
        {
            CtcpPingHandlerConfig config = new CtcpPingHandlerConfig
            {
                LineRegex = @"!bot\s+help",
                LineAction = this.MessageFunction
            };

            CtcpPingHandler uut = new CtcpPingHandler(
                config
            );

            // Keep Handling should be true by default.
            Assert.IsTrue( uut.KeepHandling );
        }

        /// <summary>
        /// Ensures a good message goes through.
        /// </summary>
        [Test]
        public void TestGoodMessage()
        {
            this.DoGoodMessageTest( remoteUser, this.ircConfig.Nick );
        }

        /// <summary>
        /// Ensures good messages work if the users have strange names.
        /// </summary>
        [Test]
        public void TestGoodMessageWithStrangeNames()
        {
            foreach ( string name in TestHelpers.StrangeNames )
            {
                this.DoGoodMessageTest( name, this.ircConfig.Nick );
            }
        }

        // Message Handler tests also test CoolDown and prefixes.  We're probably okay
        // to not test those here since that is all shared code between messages and CTCP Ping.

        /// <summary>
        /// Ensures a message that does not fit the pattern does not go through.
        /// </summary>
        [Test]
        public void TestMisMatchedMessage()
        {
            CtcpPingHandlerConfig config = new CtcpPingHandlerConfig
            {
                LineRegex = @"\d+",
                LineAction = this.MessageFunction
            };

            CtcpPingHandler uut = new CtcpPingHandler(
                config
            );

            // Does not match pattern.  No response expected.
            const string expectedMessage = "hello world!";
            string ircString = TestHelpers.ConstructCtcpPingString( remoteUser, this.ircConfig.Nick, expectedMessage );
            uut.HandleEvent( this.ConstructArgs( ircString ) );

            Assert.IsNull( this.responseReceived );
        }

        // PM Only, Channel Only, and RespondToSelf are tested in MessageHandlerTests, which is also shared code with this class.

        /// <summary>
        /// Ensures if we get a PRIVMSG that is not a PING, we ignore it.
        /// </summary>
        [Test]
        public void IgnoreMessageTest()
        {
            const string expectedMessage = "!bot help";

            CtcpPingHandlerConfig config = new CtcpPingHandlerConfig
            {
                LineRegex = @".+",
                LineAction = this.MessageFunction,
                RespondToSelf = false
            };

            CtcpPingHandler uut = new CtcpPingHandler(
                config
            );

            // Instead of action string, create a message string.
            string ircString = TestHelpers.ConstructMessageString( remoteUser, this.ircConfig.Nick, expectedMessage );
            uut.HandleEvent( this.ConstructArgs( ircString ) );

            Assert.IsNull( this.responseReceived );
        }

        // ---------------- Test Helpers ----------------

        private void DoGoodMessageTest( string user, string channel )
        {
            CtcpPingHandlerConfig config = new CtcpPingHandlerConfig
            {
                LineRegex = @"!bot\s+help",
                LineAction = this.MessageFunction
            };

            CtcpPingHandler uut = new CtcpPingHandler(
                config
            );

            const string expectedMessage = "!bot help";

            string ircString = TestHelpers.ConstructCtcpPingString( user, channel, expectedMessage );
            uut.HandleEvent( this.ConstructArgs( ircString ) );

            Assert.AreEqual( user, this.responseReceived.Channel ); // Need to send a message to the user that PM'ed us.
            Assert.AreEqual( user, this.responseReceived.User );
            Assert.AreEqual( expectedMessage, this.responseReceived.Message );
        }

        private void MessageFunction( CtcpPingHandlerArgs args )
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
