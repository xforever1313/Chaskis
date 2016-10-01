//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using GenericIrcBot;
using NUnit.Framework;
using Tests.Mocks;

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
        private static MockIrcConnection ircConnection;

        /// <summary>
        /// The response received from the event handler (if any).
        /// </summary>
        private static IrcResponse responseReceived;

        /// <summary>
        /// The user that parted.
        /// </summary>
        private const string RemoteUser = "remoteuser";

        // -------- Setup / Teardown --------

        [SetUp]
        public void TestSetup()
        {
            this.ircConfig = TestHelpers.GetTestIrcConfig();
            ircConnection = new MockIrcConnection( this.ircConfig );
            responseReceived = null;
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
                this.uut.HandleEvent( null, new IrcConfig(), ircConnection )
            );
            Assert.IsNull( responseReceived ); // Ensure handler didn't get called.

            Assert.Throws<ArgumentNullException>( () =>
                this.uut.HandleEvent( string.Empty, new IrcConfig(), ircConnection )
            );
            Assert.IsNull( responseReceived ); // Ensure handler didn't get called.

            Assert.Throws<ArgumentNullException>( () =>
                this.uut.HandleEvent( "hello world", null, ircConnection )
            );
            Assert.IsNull( responseReceived ); // Ensure handler didn't get called.

            Assert.Throws<ArgumentNullException>( () =>
                this.uut.HandleEvent( "hello world", new IrcConfig(), null )
            );
            Assert.IsNull( responseReceived ); // Ensure handler didn't get called.
        }

        /// <summary>
        /// Ensures if a user parts correctly, the event gets fired.
        /// </summary>
        [Test]
        public void PartSuccess()
        {
            string ircString =
                TestHelpers.ConstructIrcString(
                    RemoteUser,
                    PartHandler.IrcCommand,
                    ircConfig.Channel,
                    string.Empty
                );

            this.uut.HandleEvent( ircString, this.ircConfig, ircConnection );

            Assert.IsNotNull( responseReceived );

            // Part handler has no message.
            Assert.AreEqual( string.Empty, responseReceived.Message );

            // Channels should match.
            Assert.AreEqual( this.ircConfig.Channel, responseReceived.Channel );

            // Nicks should match.
            Assert.AreEqual( RemoteUser, responseReceived.RemoteUser );
        }

        /// <summary>
        /// Ensures that if the bot parts, the event isn't fired.
        /// </summary>
        [Test]
        public void BotParts()
        {
            string ircString =
                TestHelpers.ConstructIrcString(
                    ircConfig.Nick,
                    PartHandler.IrcCommand,
                    ircConfig.Channel,
                    string.Empty
                );

            this.uut.HandleEvent( ircString, this.ircConfig, ircConnection );

            Assert.IsNull( responseReceived );
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
                    ircConfig.Channel,
                    "A message"
                );

            this.uut.HandleEvent( ircString, this.ircConfig, ircConnection );

            Assert.IsNull( responseReceived );
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
                    ircConfig.Channel,
                    string.Empty
                );

            this.uut.HandleEvent( ircString, this.ircConfig, ircConnection );

            Assert.IsNull( responseReceived );
        }

        /// <summary>
        /// Ensures that if a PING appears, the join
        /// event isn't fired.
        /// </summary>
        [Test]
        public void PingAppears()
        {
            this.uut.HandleEvent(
                TestHelpers.ConstructPingString( "12345" ),
                this.ircConfig,
                ircConnection
            );
            Assert.IsNull( responseReceived );
        }

        // -------- Test Helpers --------

        /// <summary>
        /// The function that is called
        /// </summary>
        /// <param name="writer">The writer that can be written to.</param>
        /// <param name="response">The response from the server.</param>
        private static void PartFunction( IIrcWriter writer, IrcResponse response )
        {
            Assert.AreSame( ircConnection, writer );
            responseReceived = response;
        }
    }
}