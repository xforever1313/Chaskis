//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using ChaskisCore;
using NUnit.Framework;
using Tests.Mocks;

namespace Tests
{
    [TestFixture]
    public class PingHandlerTest
    {
        // -------- Fields --------

        /// <summary>
        /// Unit under test.
        /// </summary>
        private PingHandler uut;

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
        /// The user that joined.
        /// </summary>
        private const string RemoteUser = "remoteuser";

        // -------- Setup / Teardown --------

        [SetUp]
        public void TestSetup()
        {
            this.ircConfig = TestHelpers.GetTestIrcConfig();
            ircConnection = new MockIrcConnection( this.ircConfig );
            responseReceived = null;
            this.uut = new PingHandler();
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
        /// Ensures everything that needs to throw argument null
        /// exceptions does.
        /// </summary>
        [Test]
        public void ArgumentNullTest()
        {
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
        /// Ensures the bot PONGS the ping correctly.
        /// </summary>
        [Test]
        public void PongTest()
        {
            const string response = "12345"; // What we must pong back.

            this.uut.HandleEvent(
                TestHelpers.ConstructPingString( response ),
                this.ircConfig,
                ircConnection
            );

            Assert.AreEqual( response, ircConnection.PongResponse );
        }

        /// <summary>
        /// Ensures that if a PRIMSG appears, the PING
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
            Assert.IsEmpty( ircConnection.PongResponse );
        }

        /// <summary>
        /// Ensures that if a PART appears, the PING
        /// event isn't fired.
        /// </summary>
        [Test]
        public void PartCommandAppears()
        {
            string ircString =
                TestHelpers.ConstructIrcString(
                    RemoteUser,
                    PartHandler.IrcCommand,
                    ircConfig.Channel,
                    string.Empty
                );

            this.uut.HandleEvent( ircString, this.ircConfig, ircConnection );

            Assert.IsNull( responseReceived );
            Assert.IsEmpty( ircConnection.PongResponse );
        }

        /// <summary>
        /// Ensures that if a JOIN appears, the PING
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
            Assert.IsEmpty( ircConnection.PongResponse );
        }
    }
}