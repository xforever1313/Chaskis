//
//          Copyright Seth Hendrick 2016-2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using Chaskis.UnitTests.Common;
using Chaskis.Core;
using Moq;
using NUnit.Framework;
using SethCS.Exceptions;

namespace Chaskis.UnitTests.CoreTests
{
    [TestFixture]
    public class AllHandlerTest
    {
        // -------- Fields --------

        /// <summary>
        /// Unit Under Test.
        /// </summary>
        private AllHandler uut;

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
        /// A user.
        /// </summary>
        private const string remoteUser = "remoteuser";

        // -------- Setup / Teardown --------

        [SetUp]
        public void TestSetup()
        {
            this.ircConfig = TestHelpers.GetTestIrcConfig();
            this.ircWriter = new Mock<IIrcWriter>( MockBehavior.Strict );
            this.responseReceived = null;

            AllHandlerConfig allHandlerConfig = new AllHandlerConfig()
            {
                AllAction = this.AllFunction
            };
            this.uut = new AllHandler( allHandlerConfig );
        }

        // -------- Tests --------

        /// <summary>
        /// Ensures that if a bad config is passed in, we throw an exception.
        /// </summary>
        [Test]
        public void InvalidConfigTest()
        {
            Assert.Throws<ValidationException>(
                () => new AllHandler( new AllHandlerConfig() )
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
        /// Ensures that if the bot parts, the event is fired.
        /// </summary>
        [Test]
        public void BotParts()
        {
            string ircString = TestHelpers.ConstructIrcString(
                this.ircConfig.Nick,
                PartHandler.IrcCommand,
                this.ircConfig.Channels[0],
                string.Empty
            );

            this.uut.HandleEvent( this.ConstructArgs( ircString ) );
            this.CheckResponse( ircString );
        }

        /// <summary>
        /// Ensures that if a PRIMSG appears, the event is fired.
        /// </summary>
        [Test]
        public void MessageCommandAppears()
        {
            string ircString = TestHelpers.ConstructIrcString(
                remoteUser,
                MessageHandler.IrcCommand,
                this.ircConfig.Channels[0],
                "A message"
            );

            this.uut.HandleEvent( this.ConstructArgs( ircString ) );
            this.CheckResponse( ircString );
        }

        /// <summary>
        /// Ensures that if a JOIN appears, the event is fired.
        /// </summary>
        [Test]
        public void JoinCommandAppears()
        {
            string ircString = TestHelpers.ConstructIrcString(
                remoteUser,
                JoinHandler.IrcCommand,
                this.ircConfig.Channels[0],
                string.Empty
            );

            this.uut.HandleEvent( this.ConstructArgs( ircString ) );
            this.CheckResponse( ircString );
        }

        /// <summary>
        /// Ensures that if a PING appears, the event is fired.
        /// </summary>
        [Test]
        public void PingAppears()
        {
            string ircString = TestHelpers.ConstructPingString( "12345" );
            this.uut.HandleEvent( this.ConstructArgs( ircString ) );
            this.CheckResponse( ircString );
        }

        // -------- Test Helpers --------

        /// <summary>
        /// The function that is called
        /// </summary>
        /// <param name="writer">The writer that can be written to.</param>
        /// <param name="response">The response from the server.</param>
        private void AllFunction( IIrcWriter writer, IrcResponse response )
        {
            Assert.AreSame( this.ircWriter.Object, writer );
            this.responseReceived = response;
        }

        /// <summary>
        /// Ensures the bot responds accordingly.
        /// </summary>
        /// <param name="rawIrcString">The raw IRC string we expect.</param>
        private void CheckResponse( string rawIrcString )
        {
            Assert.IsNotNull( this.responseReceived );

            // Ensure its the raw string.
            Assert.AreEqual( rawIrcString, this.responseReceived.Message );

            // Channel should be empty, its up to the user to parse it.
            Assert.AreEqual( string.Empty, this.responseReceived.Channel );

            // Nick should be empty.  Its up to the user to parse it.
            Assert.AreEqual( string.Empty, this.responseReceived.RemoteUser );
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