//
//          Copyright Seth Hendrick 2016-2018.
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

namespace Chaskis.UnitTests.CoreTests
{
    [TestFixture]
    public class JoinHandlerTest
    {
        // -------- Fields --------

        /// <summary>
        /// Unit under test.
        /// </summary>
        private JoinHandler uut;

        /// <summary>
        /// Irc Config to use.
        /// </summary>
        private IrcConfig ircConfig;

        /// <summary>
        /// Mock IRC writer to use.
        /// </summary>
        private Mock<IIrcWriter> ircWriter;

        /// <summary>
        /// The response received from the event handler (if any).
        /// </summary>
        private IrcResponse responseReceived;

        /// <summary>
        /// The user that joined.
        /// </summary>
        private const string RemoteUser = "remoteuser";

        // -------- Setup / Teardown --------

        [SetUp]
        public void TestSetup()
        {
            this.ircConfig = TestHelpers.GetTestIrcConfig();
            this.ircWriter = new Mock<IIrcWriter>( MockBehavior.Strict );
            this.responseReceived = null;

            JoinHandlerConfig joinHandlerConfig = new JoinHandlerConfig
            {
                JoinAction = this.JoinFunction
            };
            this.uut = new JoinHandler( joinHandlerConfig );
        }

        // -------- Tests --------

        /// <summary>
        /// Ensures that if a bad config is passed in, we throw an exception.
        /// </summary>
        [Test]
        public void InvalidConfigTest()
        {
            Assert.Throws<ValidationException>(
                () => new JoinHandler( new JoinHandlerConfig() )
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
                new JoinHandler( null )
            );

            Assert.Throws<ArgumentNullException>( () =>
                this.uut.HandleEvent( null )
            );

            Assert.IsNull( this.responseReceived ); // Ensure handler didn't get called.
        }

        /// <summary>
        /// Ensures if a user joins correctly, the event gets fired.
        /// </summary>
        [Test]
        public void JoinSuccess()
        {
            this.DoJoinSuccessTest( RemoteUser, this.ircConfig.Channels[0] );
        }

        /// <summary>
        /// Ensures if a user with a strange name joins,
        /// the event gets fired.
        /// </summary>
        [Test]
        public void JoinWithStrangeName()
        {
            foreach( string name in TestHelpers.StrangeNames )
            {
                this.DoJoinSuccessTest( name, this.ircConfig.Channels[0] );
            }
        }

        /// <summary>
        /// Ensures if we are in a strange channel,
        /// the event gets fired.
        /// </summary>
        [Test]
        public void JoinWithStrangeChannels()
        {
            foreach( string channel in TestHelpers.StrangeChannels )
            {
                this.DoJoinSuccessTest( RemoteUser, channel );
            }

            // Private messages use names.
            foreach( string name in TestHelpers.StrangeNames )
            {
                this.DoJoinSuccessTest( RemoteUser, name );
            }
        }

        /// <summary>
        /// Ensures we handle all prefixes.
        /// </summary>
        [Test]
        public void JoinPrefixTest()
        {
            const string channel = "#somechannel";

            foreach( string prefix in TestHelpers.PrefixTests )
            {
                string ircString = prefix + " " + JoinHandler.IrcCommand + " " + channel;

                this.uut.HandleEvent( this.ConstructArgs( ircString ) );

                Assert.IsNotNull( this.responseReceived );

                // Join handler has no message.
                Assert.AreEqual( string.Empty, this.responseReceived.Message );

                // Channels should match.
                Assert.AreEqual( channel, this.responseReceived.Channel );

                // Nicks should match.
                Assert.AreEqual( "anickname", this.responseReceived.RemoteUser );
            }
        }

        [Test]
        public void BotJoinsWithRespondToSelfDisabled()
        {
            string ircString = TestHelpers.ConstructIrcString(
                this.ircConfig.Nick,
                JoinHandler.IrcCommand,
                this.ircConfig.Channels[0],
                string.Empty
            );

            this.uut.HandleEvent( this.ConstructArgs( ircString ) );

            Assert.IsNull( this.responseReceived );
        }

        [Test]
        public void BotJoinsWithRespondToSelfEnabled()
        {
            JoinHandlerConfig joinHandlerConfig = new JoinHandlerConfig
            {
                JoinAction = this.JoinFunction,
                RespondToSelf = true
            };
            JoinHandler uut = new JoinHandler( joinHandlerConfig );

            string ircString = TestHelpers.ConstructIrcString(
                this.ircConfig.Nick,
                JoinHandler.IrcCommand,
                this.ircConfig.Channels[0],
                string.Empty
            );

            uut.HandleEvent( this.ConstructArgs( ircString ) );

            Assert.IsNotNull( this.responseReceived );

            // Join handler has no message.
            Assert.AreEqual( string.Empty, this.responseReceived.Message );

            // Channels should match.
            Assert.AreEqual( this.ircConfig.Channels[0], this.responseReceived.Channel );

            // Nicks should match.
            Assert.AreEqual( this.ircConfig.Nick, this.responseReceived.RemoteUser );
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
                JoinHandler.IrcCommand,
                channel,
                string.Empty
            );

            HandlerArgs args = this.ConstructArgs( ircString );
            args.BlackListedChannels = blackList;

            this.uut.HandleEvent( args );

            Assert.IsNull( this.responseReceived );
        }

        /// <summary>
        /// Ensures that if a PRIMSG appears, the join
        /// event isn't fired.
        /// </summary>
        [Test]
        public void MessageCommandAppears()
        {
            string ircString =
                TestHelpers.ConstructIrcString(
                    RemoteUser,
                    MessageHandler.IrcCommand,
                    this.ircConfig.Channels[0],
                    "A message"
                );

            this.uut.HandleEvent( this.ConstructArgs( ircString ) );

            Assert.IsNull( this.responseReceived );
        }

        /// <summary>
        /// Ensures that if a PART appears, the join
        /// event isn't fired.
        /// </summary>
        [Test]
        public void PartCommandAppears()
        {
            string ircString =
                TestHelpers.ConstructIrcString(
                    RemoteUser,
                    PartHandler.IrcCommand,
                    this.ircConfig.Channels[0],
                    string.Empty
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
            this.uut.HandleEvent(
                this.ConstructArgs( TestHelpers.ConstructPingString( "12345" ) )
            );
            Assert.IsNull( this.responseReceived );
        }

        // -------- Test Helpers --------

        private void DoJoinSuccessTest( string user, string channel )
        {
            string ircString =
            TestHelpers.ConstructIrcString(
                user,
                JoinHandler.IrcCommand,
                channel,
                string.Empty
            );

            this.uut.HandleEvent( this.ConstructArgs( ircString ) );

            Assert.IsNotNull( this.responseReceived );

            // Join handler has no message.
            Assert.AreEqual( string.Empty, this.responseReceived.Message );

            // Channels should match.
            Assert.AreEqual( channel, this.responseReceived.Channel );

            // Nicks should match.
            Assert.AreEqual( user, this.responseReceived.RemoteUser );
        }

        /// <summary>
        /// The function that is called
        /// </summary>
        /// <param name="writer">The writer that can be written to.</param>
        /// <param name="response">The response from the server.</param>
        private void JoinFunction( IIrcWriter writer, IrcResponse response )
        {
            Assert.AreSame( this.ircWriter.Object, writer );
            this.responseReceived = response;
        }

        private HandlerArgs ConstructArgs( string line )
        {
            HandlerArgs args = new HandlerArgs();
            args.Line = line;
            args.IrcWriter = this.ircWriter.Object;
            args.IrcConfig = this.ircConfig;

            return args;
        }
    }
}