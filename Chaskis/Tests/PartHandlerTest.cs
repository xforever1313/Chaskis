//
//          Copyright Seth Hendrick 2016-2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using ChaskisCore;
using Moq;
using NUnit.Framework;

namespace Tests
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
        private IrcResponse responseReceived;

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

            this.uut = new PartHandler( PartFunction );
        }

        // -------- Tests --------

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
            this.DoPartSuccess( RemoteUser );
        }

        [Test]
        public void PartSuccessWithStrangeNames()
        {
            foreach( string name in TestHelpers.StrangeNames )
            {
                this.DoPartSuccess( name );
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
                    MessageHandler.IrcCommand,
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

        private void DoPartSuccess( string name )
        {
            string ircString =
                TestHelpers.ConstructIrcString(
                    name,
                    PartHandler.IrcCommand,
                    this.ircConfig.Channels[0],
                    string.Empty
                );

            this.uut.HandleEvent( this.ConstructArgs( ircString ) );

            Assert.IsNotNull( this.responseReceived );

            // Part handler has no message.
            Assert.AreEqual( string.Empty, this.responseReceived.Message );

            // Channels should match.
            Assert.AreEqual( this.ircConfig.Channels[0], this.responseReceived.Channel );

            // Nicks should match.
            Assert.AreEqual( name, this.responseReceived.RemoteUser );
        }

        /// <summary>
        /// The function that is called
        /// </summary>
        /// <param name="writer">The writer that can be written to.</param>
        /// <param name="response">The response from the server.</param>
        private void PartFunction( IIrcWriter writer, IrcResponse response )
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