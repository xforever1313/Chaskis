//
//          Copyright Seth Hendrick 2016-2020.
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

namespace Chaskis.UnitTests.CoreTests.Handlers.Action
{
    [TestFixture]
    public class ActionHandlerTests
    {
        // ---------------- Fields ----------------

        /// <summary>
        /// Irc Config to use.
        /// </summary>
        private IrcConfig ircConfig;

        /// <summary>
        /// The mock IRC connection to use.
        /// </summary>
        private Mock<IIrcWriter> ircWriter;

        /// <summary>
        /// The response received from the event handler (if any).
        /// </summary>
        private ActionHandlerArgs responseReceived;

        /// <summary>
        /// The user that joined.
        /// </summary>
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
                () => new ActionHandler( new ActionHandlerConfig() )
            );
        }

        /// <summary>
        /// Ensures that the class is created correctly.
        /// </summary>
        [Test]
        public void ConstructionTest()
        {
            ActionHandlerConfig config = new ActionHandlerConfig
            {
                LineRegex = @"!bot\s+help",
                LineAction = this.MessageFunction
            };

            ActionHandler uut = new ActionHandler(
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
            this.DoGoodMessageTest( remoteUser, this.ircConfig.Channels[0] );
        }

        /// <summary>
        /// Ensures good messages work if the users have strange names.
        /// </summary>
        [Test]
        public void TestGoodMessageWithStrangeNames()
        {
            foreach ( string name in TestHelpers.StrangeNames )
            {
                this.DoGoodMessageTest( name, this.ircConfig.Channels[0] );
            }
        }

        /// <summary>
        /// Ensures good messages work if the channels have strange names.
        /// </summary>
        [Test]
        public void TestGoodMessageWithStrangeChannelNames()
        {
            // Do channels.
            foreach ( string channel in TestHelpers.StrangeChannels )
            {
                this.DoGoodMessageTest( remoteUser, channel );
            }

            // Do users (users can be a channel for a private message).
            foreach ( string channel in TestHelpers.StrangeNames )
            {
                this.DoGoodMessageTest( remoteUser, channel );
            }
        }

        // Message Handler tests also test CoolDown and prefixes.  We're probably okay
        // to not test those here since that is all shared code between messages and actions.

        /// <summary>
        /// Ensures a message that does not fit the pattern does not go through.
        /// </summary>
        [Test]
        public void TestMisMatchedMessage()
        {
            ActionHandlerConfig config = new ActionHandlerConfig
            {
                LineRegex = @"!bot\s+help",
                LineAction = this.MessageFunction
            };

            ActionHandler uut = new ActionHandler(
                config
            );

            // Does not match pattern.  No response expected.
            const string expectedMessage = "hello world!";
            string ircString = this.GenerateMessage( remoteUser, this.ircConfig.Channels[0], expectedMessage );
            uut.HandleEvent( this.ConstructArgs( ircString ) );

            Assert.IsNull( this.responseReceived );
        }

        /// <summary>
        /// Ensures that if we get a message from a channel
        /// that is black-listed, nothing happens.
        /// </summary>
        [Test]
        public void BlacklistTest()
        {
            ActionHandlerConfig config = new ActionHandlerConfig
            {
                LineRegex = @"!bot\s+help",
                LineAction = this.MessageFunction
            };

            ActionHandler uut = new ActionHandler(
                config
            );

            const string channel = "#blacklist";
            const string expectedMessage = "!bot help";

            List<string> blackList = new List<string>() { channel };

            string ircString = this.GenerateMessage( remoteUser, channel, expectedMessage );

            HandlerArgs args = this.ConstructArgs( ircString );
            args.BlackListedChannels = blackList;

            uut.HandleEvent( args );

            Assert.IsNull( this.responseReceived );
        }

        /// <summary>
        /// Ensures {%user%}, {%channel%}, and {%nick%} get replaced in the line
        /// regex properly.
        /// </summary>
        [Test]
        public void TestLiquification()
        {
            ActionHandlerConfig config = new ActionHandlerConfig
            {
                LineRegex = @"!{%nick%} {%channel%} {%user%}",
                LineAction = this.MessageFunction
            };

            ActionHandler uut = new ActionHandler(
                config
            );

            string expectedMessage = string.Format(
                "!{0} {1} {2}",
                this.ircConfig.Nick,
                this.ircConfig.Channels[0],
                remoteUser
            );

            string ircString = this.GenerateMessage( remoteUser, this.ircConfig.Channels[0], expectedMessage );
            uut.HandleEvent( this.ConstructArgs( ircString ) );

            Assert.IsNotNull( this.responseReceived );
            Assert.AreEqual( this.ircConfig.Channels[0], this.responseReceived.Channel );
            Assert.AreEqual( remoteUser, this.responseReceived.User );
            Assert.AreEqual( expectedMessage, this.responseReceived.Message );
        }

        // PM Only, Channel Only, and RespondToSelf are tested in MessageHandlerTests, which is also shared code with this class.

        /// <summary>
        /// Ensures if we get a PRIVMSG that is not an ACTION, we ignore it.
        /// </summary>
        [Test]
        public void IgnoreMessageTest()
        {
            const string expectedMessage = "!bot help";

            ActionHandlerConfig config = new ActionHandlerConfig
            {
                LineRegex = @".+",
                LineAction = this.MessageFunction,
                RespondToSelf = false
            };

            ActionHandler uut = new ActionHandler(
                config
            );

            // Instead of action string, create a message string.
            string ircString = TestHelpers.ConstructMessageString( remoteUser, this.ircConfig.Channels[0], expectedMessage );
            uut.HandleEvent( this.ConstructArgs( ircString ) );

            Assert.IsNull( this.responseReceived );
        }

        // ---------------- Test Helpers ----------------

        private void DoGoodMessageTest( string user, string channel )
        {
            ActionHandlerConfig config = new ActionHandlerConfig
            {
                LineRegex = @"!bot\s+help",
                LineAction = this.MessageFunction
            };

            ActionHandler uut = new ActionHandler(
                config
            );

            const string expectedMessage = "!bot help";

            string ircString = this.GenerateMessage( user, channel, expectedMessage );
            uut.HandleEvent( this.ConstructArgs( ircString ) );

            Assert.AreEqual( channel, this.responseReceived.Channel );
            Assert.AreEqual( user, this.responseReceived.User );
            Assert.AreEqual( expectedMessage, this.responseReceived.Message );
        }

        /// <summary>
        /// Generates the message from IRC.
        /// </summary>
        /// <param name="nick">The nick name that sent the message</param>
        /// <param name="channel">The channel it was sent on.</param>
        /// <param name="message">The message that was sent.</param>
        /// <returns>The string from an IRC server.</returns>
        private string GenerateMessage( string nick, string channel, string message )
        {
            return TestHelpers.ConstructActionString( nick, channel, message );
        }

        /// <summary>
        /// The function that is called
        /// </summary>
        /// <param name="writer">The writer that can be written to.</param>
        /// <param name="args">The response from the server.</param>
        private void MessageFunction( ActionHandlerArgs args )
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
