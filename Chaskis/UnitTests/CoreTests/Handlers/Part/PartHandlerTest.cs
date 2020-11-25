//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using Chaskis.UnitTests.Common;
using Chaskis.Core;
using Moq;
using NUnit.Framework;
using SethCS.Exceptions;

namespace Chaskis.UnitTests.CoreTests.Handlers.Part
{
    [TestFixture]
    public class PartHandlerTest
    {
        // -------- Fields --------

        /// <summary>
        /// Unit under test.
        /// </summary>
        private PartHandler uut;

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
        private PartHandlerArgs responseReceived;

        /// <summary>
        /// The user that parted.
        /// </summary>
        private const string RemoteUser = "remoteuser";

        // -------- Setup / Teardown --------

        [SetUp]
        public void TestSetup()
        {
            this.ircConfig = TestHelpers.GetTestIrcConfig();
            this.ircWriter = new Mock<IIrcWriter>( MockBehavior.Strict );
            this.responseReceived = null;

            PartHandlerConfig config = new PartHandlerConfig
            {
                PartAction = this.PartFunction
            };
            this.uut = new PartHandler( config );
        }

        // -------- Tests --------

        /// <summary>
        /// Ensures that if a bad config is passed in, we throw an exception.
        /// </summary>
        [Test]
        public void InvalidConfigTest()
        {
            Assert.Throws<ValidationException>(
                () => new PartHandler( new PartHandlerConfig() )
            );
        }

        /// <summary>
        /// Ensures that the class is created correctly.
        /// </summary>
        [Test]
        public void ConstructionTest()
        {
            // Keep Handling should be true by default.
            Assert.IsTrue( this.uut.KeepHandling );
        }

        /// <summary>
        /// Ensurs everything that needs to throw argument null
        /// exceptions does.
        /// </summary>
        [Test]
        public void ArgumentNullTest()
        {
            Assert.Throws<ArgumentNullException>( () =>
                new PartHandler( null )
            );

            Assert.Throws<ArgumentNullException>( () =>
                this.uut.HandleEvent( null )
            );

            Assert.IsNull( this.responseReceived ); // Ensure handler didn't get called.
        }

        /// <summary>
        /// Ensures if a user parts correctly, the event gets fired.
        /// </summary>
        [Test]
        public void PartSuccess()
        {
            this.DoPartSuccess( RemoteUser, this.ircConfig.Channels[0] );
        }

        /// <summary>
        /// Ensures that if a user parts correctly with a reason, the event gets fired.
        /// </summary>
        [Test]
        public void PartSuccessWithReason()
        {
            this.DoPartSuccess( RemoteUser, this.ircConfig.Channels[0], "My Reason" );
        }

        [Test]
        public void PartSuccessWithStrangeNames()
        {
            foreach( string name in TestHelpers.StrangeNames )
            {
                this.DoPartSuccess( name, this.ircConfig.Channels[0] );
                this.responseReceived = null;
            }
        }

        /// <summary>
        /// Ensures we handle all prefixes.
        /// </summary>
        [Test]
        public void PartPrefixTest()
        {
            const string channel = "#somechannel";

            foreach( string prefix in TestHelpers.PrefixTests )
            {
                string ircString = prefix + " " + PartHandler.IrcCommand + " " + channel;

                this.uut.HandleEvent( this.ConstructArgs( ircString ) );

                Assert.IsNotNull( this.responseReceived );

                // Part handler has no reason.
                Assert.AreEqual( string.Empty, this.responseReceived.Reason );

                // Channels should match.
                Assert.AreEqual( channel, this.responseReceived.Channel );

                // Nicks should match.
                Assert.AreEqual( "anickname", this.responseReceived.User );

                this.responseReceived = null;
            }
        }

        /// <summary>
        /// Ensures that if a user parts on a strange channel,
        /// the event gets fired.
        /// </summary>
        [Test]
        public void PartSuccessWithStrangeChannel()
        {
            foreach( string channel in TestHelpers.StrangeChannels )
            {
                this.DoPartSuccess( RemoteUser, channel );
                this.responseReceived = null;
            }

            foreach( string name in TestHelpers.StrangeNames )
            {
                this.DoPartSuccess( RemoteUser, name );
                this.responseReceived = null;
            }
        }

        /// <summary>
        /// Ensures that if the bot parts, the event isn't fired.
        /// </summary>
        [Test]
        public void BotParts()
        {
            string ircString =
                TestHelpers.ConstructIrcString(
                    this.ircConfig.Nick,
                    PartHandler.IrcCommand,
                    this.ircConfig.Channels[0],
                    string.Empty
                );

            this.uut.HandleEvent( this.ConstructArgs( ircString ) );

            Assert.IsNull( this.responseReceived );
        }

        /// <summary>
        /// Ensures that if we get a part from a channel
        /// that is black-listed, nothing happens.
        /// </summary>
        [Test]
        public void BlacklistTest()
        {
            const string channel = "#blacklist";

            List<string> blackList = new List<string>() { channel };

            string ircString = TestHelpers.ConstructIrcString(
                RemoteUser,
                PartHandler.IrcCommand,
                channel,
                string.Empty
            );

            HandlerArgs args = this.ConstructArgs( ircString );
            args.BlackListedChannels = blackList;

            this.uut.HandleEvent( args );

            Assert.IsNull( this.responseReceived );
        }

        /// <summary>
        /// Ensures that if a PRIMSG appears, the part
        /// event isn't fired.
        /// </summary>
        [Test]
        public void MessageCommandAppears()
        {
            string ircString =
                TestHelpers.ConstructIrcString(
                    RemoteUser,
                    PrivateMessageHelper.IrcCommand,
                    this.ircConfig.Channels[0],
                    "A message"
                );

            this.uut.HandleEvent( this.ConstructArgs( ircString ) );

            Assert.IsNull( this.responseReceived );
        }

        /// <summary>
        /// Ensures that if a JOIN appears, the part
        /// event isn't fired.
        /// </summary>
        [Test]
        public void JoinCommandAppears()
        {
            string ircString =
                TestHelpers.ConstructIrcString(
                    RemoteUser,
                    JoinHandler.IrcCommand,
                    this.ircConfig.Channels[0],
                    string.Empty
                );

            this.uut.HandleEvent( this.ConstructArgs( ircString ) );

            Assert.IsNull( this.responseReceived );
        }

        /// <summary>
        /// Ensures that if a KICK appears, the event
        /// isn't fired.
        /// </summary>
        [Test]
        public void KickAppears()
        {
            string ircString = TestHelpers.ConstructKickString(
                "moderator",
                "kickeduser",
                this.ircConfig.Channels[0],
                "Some Reason"
            );

            this.uut.HandleEvent( this.ConstructArgs( ircString ) );

            Assert.IsNull( this.responseReceived );
        }

        /// <summary>
        /// Ensures that if a PING appears, the join
        /// event isn't fired.
        /// </summary>
        [Test]
        public void PingAppears()
        {
            string ircString = TestHelpers.ConstructPingString( "12345" );
            this.uut.HandleEvent( this.ConstructArgs( ircString ) );

            Assert.IsNull( this.responseReceived );
        }

        // -------- Test Helpers --------

        private void DoPartSuccess( string name, string channel, string reason = null )
        {
            string ircString =
                TestHelpers.ConstructIrcString(
                    name,
                    PartHandler.IrcCommand,
                    channel,
                    reason ?? null
                );

            this.uut.HandleEvent( this.ConstructArgs( ircString ) );

            Assert.IsNotNull( this.responseReceived );

            // Part handler has no message.
            Assert.AreEqual( reason ?? string.Empty, this.responseReceived.Reason );

            // Channels should match.
            Assert.AreEqual( channel, this.responseReceived.Channel );

            // Nicks should match.
            Assert.AreEqual( name, this.responseReceived.User );
        }

        /// <summary>
        /// The function that is called
        /// </summary>
        private void PartFunction( PartHandlerArgs args )
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