//
//          Copyright Seth Hendrick 2016-2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using ChaskisCore;
using Moq;
using NUnit.Framework;

namespace Tests
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
            this.uut = new JoinHandler( this.JoinFunction );
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
                new JoinHandler( null )
            );

            Assert.Throws<ArgumentNullException>( () =>
                this.uut.HandleEvent( null, new IrcConfig(), this.ircWriter.Object )
            );
            Assert.IsNull( responseReceived ); // Ensure handler didn't get called.

            Assert.Throws<ArgumentNullException>( () =>
                this.uut.HandleEvent( string.Empty, new IrcConfig(), this.ircWriter.Object )
            );
            Assert.IsNull( responseReceived ); // Ensure handler didn't get called.

            Assert.Throws<ArgumentNullException>( () =>
                this.uut.HandleEvent( "hello world", null, this.ircWriter.Object )
            );
            Assert.IsNull( responseReceived ); // Ensure handler didn't get called.

            Assert.Throws<ArgumentNullException>( () =>
                this.uut.HandleEvent( "hello world", new IrcConfig(), null )
            );
            Assert.IsNull( this.responseReceived ); // Ensure handler didn't get called.
        }

        /// <summary>
        /// Ensures if a user joins correctly, the event gets fired.
        /// </summary>
        [Test]
        public void JoinSuccess()
        {
            string ircString =
                TestHelpers.ConstructIrcString(
                    RemoteUser,
                    JoinHandler.IrcCommand,
                    this.ircConfig.Channel,
                    string.Empty
                );

            this.uut.HandleEvent( ircString, this.ircConfig, this.ircWriter.Object );

            Assert.IsNotNull( this.responseReceived );

            // Join handler has no message.
            Assert.AreEqual( string.Empty, this.responseReceived.Message );

            // Channels should match.
            Assert.AreEqual( this.ircConfig.Channel, this.responseReceived.Channel );

            // Nicks should match.
            Assert.AreEqual( RemoteUser, this.responseReceived.RemoteUser );
        }

        /// <summary>
        /// Ensures that if the bot joins, the event isn't fired.
        /// </summary>
        [Test]
        public void BotJoins()
        {
            string ircString =
                TestHelpers.ConstructIrcString(
                    this.ircConfig.Nick,
                    JoinHandler.IrcCommand,
                    this.ircConfig.Channel,
                    string.Empty
                );

            this.uut.HandleEvent( ircString, this.ircConfig, this.ircWriter.Object );

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
                    this.ircConfig.Channel,
                    "A message"
                );

            this.uut.HandleEvent( ircString, this.ircConfig, this.ircWriter.Object );

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
                    this.ircConfig.Channel,
                    string.Empty
                );

            this.uut.HandleEvent( ircString, this.ircConfig, this.ircWriter.Object );

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
                TestHelpers.ConstructPingString( "12345" ),
                this.ircConfig,
                this.ircWriter.Object
            );
            Assert.IsNull( this.responseReceived );
        }

        // -------- Test Helpers --------

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
    }
}