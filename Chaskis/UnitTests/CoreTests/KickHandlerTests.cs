//
//          Copyright Seth Hendrick 2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Chaskis.Core;
using Chaskis.UnitTests.Common;
using Moq;
using NUnit.Framework;
using SethCS.Exceptions;

namespace Chaskis.UnitTests.CoreTests
{
    public class KickHandlerTests
    {
        // ---------------- Fields ----------------

        private KickHandler uut;

        private IrcConfig ircConfig;

        private Mock<IIrcWriter> ircWriter;

        private KickHandlerArgs kickHandlerArgs;

        private const string kickedUser = "kickeduser";

        private const string moderator = "moderator";

        // ---------------- Setup / Teardown ----------------

        [SetUp]
        public void TestSetup()
        {
            this.ircConfig = TestHelpers.GetTestIrcConfig();
            this.ircWriter = new Mock<IIrcWriter>( MockBehavior.Strict );
            this.kickHandlerArgs = null;

            KickHandlerConfig kickHandlerConfig = new KickHandlerConfig
            {
                KickAction = this.KickHandler
            };

            this.uut = new KickHandler( kickHandlerConfig );
        }

        [TearDown]
        public void TestTeardown()
        {
            this.kickHandlerArgs = null;
        }

        // ---------------- Tests ----------------

        /// <summary>
        /// Ensures that if a bad config is passed in, we throw an exception.
        /// </summary>
        [Test]
        public void InvalidConfigTest()
        {
            Assert.Throws<ValidationException>(
                () => new KickHandler( new KickHandlerConfig() )
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

        [Test]
        public void DoSuccessTestWithNoReason()
        {
            this.DoKickSuccessTest( moderator, kickedUser, this.ircConfig.Channels[0], null );
        }

        [Test]
        public void DoSuccessTestWithReason()
        {
            this.DoKickSuccessTest( moderator, kickedUser, this.ircConfig.Channels[0], "Some Reason" );
        }

        /// <summary>
        /// Ensures if the moderator kicks and has a strange name,
        /// it still gets triggered.
        /// </summary>
        [Test]
        public void ModeratorHasStrangeName()
        {
            foreach( string name in TestHelpers.StrangeNames )
            {
                this.DoKickSuccessTest( name, kickedUser, this.ircConfig.Channels[0] );
                this.kickHandlerArgs = null;
            }
        }

        /// <summary>
        /// Ensures if kicked user and has a strange name,
        /// it still gets triggered.
        /// </summary>
        [Test]
        public void KickedUserHasStrangeName()
        {
            foreach( string name in TestHelpers.StrangeNames )
            {
                this.DoKickSuccessTest( moderator, name, this.ircConfig.Channels[0] );
                this.kickHandlerArgs = null;
            }
        }

        /// <summary>
        /// Ensures if the channel has a strange name,
        /// it still gets triggered.
        /// </summary>
        [Test]
        public void ChannelHasStrangeName()
        {
            foreach( string name in TestHelpers.StrangeChannels )
            {
                this.DoKickSuccessTest( moderator, kickedUser, name, "My Reason" );
                this.kickHandlerArgs = null;
            }
        }

        /// <summary>
        /// Ensures we handle all of the prefixes as expected.
        /// </summary>
        [Test]
        public void KickPrefixTest()
        {
            foreach( string prefix in TestHelpers.PrefixTests )
            {
                string ircString = prefix + " " + Core.KickHandler.IrcCommand + " " + this.ircConfig.Channels[0] + " " + kickedUser;
                this.uut.HandleEvent( this.ConstructArgs( ircString ) );

                this.CheckSuccess( "anickname", kickedUser, this.ircConfig.Channels[0], null );
                this.kickHandlerArgs = null;
            }
        }

        /// <summary>
        /// Ensures if the bot is kicked, and respond to self is disabled, nothing happens.
        /// </summary>
        [Test]
        public void BotIsKickedWithRespondToSelfDisabled()
        {
            KickHandlerConfig kickHandlerConfig = new KickHandlerConfig
            {
                KickAction = this.KickHandler,
                RespondToSelfBeingKicked = false,
                RespondToSelfPerformingKick = false
            };

            this.uut = new KickHandler( kickHandlerConfig );

            string ircString = TestHelpers.ConstructKickString(
                moderator,
                this.ircConfig.Nick,
                this.ircConfig.Channels[0]
            );

            this.uut.HandleEvent( this.ConstructArgs( ircString ) );
            Assert.IsNull( this.kickHandlerArgs );
        }

        /// <summary>
        /// Ensures that if the bot is kicked, and respond to self is set to true,
        /// then the event fires as expected.
        /// </summary>
        [Test]
        public void BotIsKickedWithRespondToSelfEnabled()
        {
            KickHandlerConfig kickHandlerConfig = new KickHandlerConfig
            {
                KickAction = this.KickHandler,
                RespondToSelfBeingKicked = true,
                RespondToSelfPerformingKick = false
            };

            this.uut = new KickHandler( kickHandlerConfig );

            this.DoKickSuccessTest( moderator, this.ircConfig.Nick, this.ircConfig.Channels[0], "reason" );
        }

        /// <summary>
        /// Ensures if the bot does the kicking, and respond to self is disabled, nothing happens.
        /// </summary>
        [Test]
        public void BotPerformsKickWithRespondToSelfDisabled()
        {
            KickHandlerConfig kickHandlerConfig = new KickHandlerConfig
            {
                KickAction = this.KickHandler,
                RespondToSelfBeingKicked = false,
                RespondToSelfPerformingKick = false
            };

            this.uut = new KickHandler( kickHandlerConfig );

            string ircString = TestHelpers.ConstructKickString(
                this.ircConfig.Nick,
                moderator,
                this.ircConfig.Channels[0]
            );

            this.uut.HandleEvent( this.ConstructArgs( ircString ) );
            Assert.IsNull( this.kickHandlerArgs );
        }

        /// <summary>
        /// Ensures that if the bot does the kicking, and respond to self is set to true,
        /// then the event fires as expected.
        /// </summary>
        [Test]
        public void BotPerformsKickWithRespondToSelfEnabled()
        {
            KickHandlerConfig kickHandlerConfig = new KickHandlerConfig
            {
                KickAction = this.KickHandler,
                RespondToSelfBeingKicked = false,
                RespondToSelfPerformingKick = true
            };

            this.uut = new KickHandler( kickHandlerConfig );

            this.DoKickSuccessTest( this.ircConfig.Nick, kickedUser, this.ircConfig.Channels[0], "Some Reason 13" );
        }

        /// <summary>
        /// Ensures that if a kick happens, but from a channel that is black-listed,
        /// nothing happens.
        /// </summary>
        [Test]
        public void BlackListTest()
        {
            const string channel = "#blacklist";

            List<string> blackList = new List<string>() { channel };

            string ircString = TestHelpers.ConstructKickString(
                moderator,
                kickedUser,
                channel
            );

            HandlerArgs args = this.ConstructArgs( ircString );
            args.BlackListedChannels = blackList;

            this.uut.HandleEvent( args );

            Assert.IsNull( this.kickHandlerArgs );
        }

        /// <summary>
        /// Ensures that if a PRIVMSG appears, the kick
        /// event isn't fired.
        /// </summary>
        [Test]
        public void MessageCommandAppears()
        {
            string ircString =
                TestHelpers.ConstructIrcString(
                    moderator,
                    PrivateMessageHelper.IrcCommand,
                    this.ircConfig.Channels[0],
                    "A message"
                );

            this.uut.HandleEvent( this.ConstructArgs( ircString ) );

            Assert.IsNull( this.kickHandlerArgs );
        }

        /// <summary>
        /// Ensures that if a JOIN appears, the kick
        /// event isn't fired.
        /// </summary>
        [Test]
        public void JoinCommandAppears()
        {
            string ircString =
                TestHelpers.ConstructIrcString(
                    moderator,
                    JoinHandler.IrcCommand,
                    this.ircConfig.Channels[0],
                    string.Empty
                );

            this.uut.HandleEvent( this.ConstructArgs( ircString ) );

            Assert.IsNull( this.kickHandlerArgs );
        }

        /// <summary>
        /// Ensures that if a PART appears, the kick
        /// event isn't fired.
        /// </summary>
        [Test]
        public void PartCommandAppears()
        {
            string ircString =
                TestHelpers.ConstructIrcString(
                    moderator,
                    PartHandler.IrcCommand,
                    this.ircConfig.Channels[0],
                    string.Empty
                );

            this.uut.HandleEvent( this.ConstructArgs( ircString ) );

            Assert.IsNull( this.kickHandlerArgs );
        }

        /// <summary>
        /// Ensures that if a PING appears, the kick
        /// event isn't fired.
        /// </summary>
        [Test]
        public void PingAppears()
        {
            this.uut.HandleEvent(
                this.ConstructArgs( TestHelpers.ConstructPingString( "12345" ) )
            );
            Assert.IsNull( this.kickHandlerArgs );
        }

        /// <summary>
        /// Ensures that if a PING appears, the kick
        /// event isn't fired.
        /// </summary>
        [Test]
        public void PongAppears()
        {
            this.uut.HandleEvent(
                this.ConstructArgs( TestHelpers.ConstringPongString( "MyServer", "12345" ) )
            );
            Assert.IsNull( this.kickHandlerArgs );
        }

        // ---------------- Test Helpers ----------------

        private void DoKickSuccessTest( string moderator, string kickedUser, string channel, string reason = null )
        {
            string ircString = TestHelpers.ConstructKickString(
                moderator,
                kickedUser,
                channel,
                reason
            );

            this.uut.HandleEvent( this.ConstructArgs( ircString ) );

            this.CheckSuccess( moderator, kickedUser, channel, reason );
        }

        private void CheckSuccess( string moderator, string kickedUser, string channel, string reason = null )
        {
            Assert.IsNotNull( this.kickHandlerArgs );

            Assert.AreEqual( moderator, this.kickHandlerArgs.UserWhoSentKick );
            Assert.AreEqual( kickedUser, this.kickHandlerArgs.UserWhoWasKicked );
            Assert.AreEqual( channel, this.kickHandlerArgs.Channel );
            Assert.AreEqual( reason ?? string.Empty, this.kickHandlerArgs.Reason );
            Assert.AreSame( this.ircWriter.Object, this.kickHandlerArgs.Writer );
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

        private void KickHandler( KickHandlerArgs args )
        {
            this.kickHandlerArgs = args;
        }
    }
}
