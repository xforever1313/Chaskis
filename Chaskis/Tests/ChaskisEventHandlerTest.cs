//
//          Copyright Seth Hendrick 2017.
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
    public class ChaskisEventHandlerTest
    {
        // ---------------- Fields ---------------

        private ChaskisEventHandlerLineActionArgs argsCaptured;

        private IrcConfig ircConfig;

        private Mock<IIrcWriter> mockWriter;

        // ---------------- Setup / Teardown ---------------

        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            this.ircConfig = TestHelpers.GetTestIrcConfig();
        }

        [TestFixtureTearDown]
        public void TestFixtureTeardown()
        {
        }

        [SetUp]
        public void TestSetup()
        {
            this.argsCaptured = null;
            this.mockWriter = new Mock<IIrcWriter>( MockBehavior.Strict );
        }

        [TearDown]
        public void TestTeardown()
        {
        }

        // ---------------- Tests ---------------

        /// <summary>
        /// Ensure that if we give bad args, exceptions get thrown.
        /// </summary>
        [Test]
        public void InvalidConstructorTest()
        {
            // Core source must be a valid protocol.
            Assert.Throws<ArgumentException>(
                () => new ChaskisEventHandler( ".+", ChaskisEventSource.CORE, "DERP", this.LineAction )
            );

            Assert.Throws<ArgumentNullException>(
                () => new ChaskisEventHandler( ".+", ChaskisEventSource.PLUGIN, "DERPBOT", null )
            );

            Assert.Throws<ArgumentNullException>(
                () => new ChaskisEventHandler( null, ChaskisEventSource.PLUGIN, "DERPBOT", this.LineAction )
            );

            Assert.Throws<ArgumentNullException>(
                () => new ChaskisEventHandler( ".+", ChaskisEventSource.PLUGIN, null, this.LineAction )
            );

            Assert.Throws<ArgumentNullException>(
                () => new ChaskisEventHandler( ".+", ChaskisEventSource.PLUGIN, string.Empty, this.LineAction )
            );
        }

        /// <summary>
        /// Tests to ensure we can capture a core event.
        /// </summary>
        [Test]
        public void CaptureCoreIrcEvent()
        {
            string expectedArgs = string.Format(
                "DISCONNECT FROM {0} AS {1}",
                this.ircConfig.Server,
                this.ircConfig.Nick
            );

            string disconnectEventStr = "CHASKIS CORE IRC " + expectedArgs;

            ChaskisEventHandler handler = new ChaskisEventHandler(
                Regexes.ChaskisIrcDisconnectEvent,
                ChaskisEventProtocol.IRC,
                this.LineAction
            );

            HandlerArgs handlerArgs = this.CreateHandlerArgs( disconnectEventStr );

            handler.HandleEvent( handlerArgs );

            Assert.IsNotNull( this.argsCaptured );
            Assert.AreEqual( expectedArgs, this.argsCaptured.EventArgs );
            Assert.AreEqual( "IRC", this.argsCaptured.PluginName );
        }

        /// <summary>
        /// Ensures that if we are not expecting a core event, we ignore it.
        /// </summary>
        [Test]
        public void IgnoresCoreIrcEvent()
        {
            string expectedArgs = string.Format(
                "DISCONNECT FROM {0} AS {1}",
                this.ircConfig.Server,
                this.ircConfig.Nick
            );

            string disconnectEventStr = "CHASKIS CORE IRC " + expectedArgs;

            ChaskisEventHandler handler = new ChaskisEventHandler(
                @"USERLIST\s+(?<users>.+)",
                ChaskisEventSource.PLUGIN,
                "userlistbot",
                this.LineAction
            );

            HandlerArgs handlerArgs = this.CreateHandlerArgs( disconnectEventStr );

            handler.HandleEvent( handlerArgs );

            Assert.IsNull( this.argsCaptured );
        }

        /// <summary>
        /// Ensures that if we are expecting a plugin event, we capture it.
        /// </summary>
        [Test]
        public void CapturePluginEvent()
        {
            string expectedArgs = "USERLIST EVERGREEN MARKEM HARRIS";

            string disconnectEventStr = "CHASKIS PLUGIN USERLISTBOT " + expectedArgs;

            ChaskisEventHandler handler = new ChaskisEventHandler(
                @"USERLIST\s+(?<users>.+)",
                ChaskisEventSource.PLUGIN,
                "userlistbot",
                this.LineAction
            );

            HandlerArgs handlerArgs = this.CreateHandlerArgs( disconnectEventStr );

            handler.HandleEvent( handlerArgs );

            Assert.IsNotNull( this.argsCaptured );
            Assert.AreEqual( expectedArgs, this.argsCaptured.EventArgs );
            Assert.AreEqual( "USERLISTBOT", this.argsCaptured.PluginName );
            Assert.AreEqual( "EVERGREEN MARKEM HARRIS", this.argsCaptured.Match.Groups["users"].Value );
        }

        /// <summary>
        /// Ensures that if we ignore a plugin if we weren't expecting it.
        /// </summary>
        [Test]
        public void IgnoreWrongPlugin()
        {
            string expectedArgs = "USERLIST EVERGREEN MARKEM HARRIS";

            string someOtherPluginStr = "CHASKIS PLUGIN SOMEOTHERPLUGIN " + expectedArgs;

            ChaskisEventHandler handler = new ChaskisEventHandler(
                @"USERLIST\s+(?<users>.+)",
                ChaskisEventSource.PLUGIN,
                "userlistbot",
                this.LineAction
            );

            HandlerArgs handlerArgs = this.CreateHandlerArgs( someOtherPluginStr );
            handler.HandleEvent( handlerArgs );

            Assert.IsNull( this.argsCaptured );
        }

        /// <summary>
        /// Ensures if we get an unknown protocol/plugin, we don't fire.
        /// </summary>
        [Test]
        public void IgnoreUnknownProtocol()
        {
            string expectedArgs = "USERLIST EVERGREEN MARKEM HARRIS";

            string unknownProtocolString = "CHASKIS UNKNOWN USERLISTBOT " + expectedArgs;

            ChaskisEventHandler handler = new ChaskisEventHandler(
                @".+",
                ChaskisEventSource.PLUGIN,
                "userlistbot",
                this.LineAction
            );

            HandlerArgs handlerArgs = this.CreateHandlerArgs( unknownProtocolString );
            handler.HandleEvent( handlerArgs );

            Assert.IsNull( this.argsCaptured );
        }

        /// <summary>
        /// Ensure that if everything matches BUT our pattern, we don't fire.
        /// </summary>
        [Test]
        public void IgnoreNoMatch()
        {
            string expectedArgs = "USERLIST EVERGREEN MARKEM HARRIS";

            string messageStr = "CHASKIS PLUGIN USERLISTBOT " + expectedArgs;

            ChaskisEventHandler handler = new ChaskisEventHandler(
                @"SOMETHINGELSE\s+(?<users>.+)",
                ChaskisEventSource.PLUGIN,
                "userlistbot",
                this.LineAction
            );

            HandlerArgs handlerArgs = this.CreateHandlerArgs( messageStr );
            handler.HandleEvent( handlerArgs );

            Assert.IsNull( this.argsCaptured );
        }

        /// <summary>
        /// Ensures we ignore join.
        /// </summary>
        [Test]
        public void IgnoreJoin()
        {
            ChaskisEventHandler handler = new ChaskisEventHandler(
                Regexes.ChaskisIrcDisconnectEvent,
                ChaskisEventProtocol.IRC,
                this.LineAction
            );

            string ircString = TestHelpers.ConstructIrcString(
                "user",
                JoinHandler.IrcCommand,
                "#channel",
                null
            );
            HandlerArgs handlerArgs = this.CreateHandlerArgs( ircString );
            handler.HandleEvent( handlerArgs );

            Assert.IsNull( this.argsCaptured );
        }

        /// <summary>
        /// Ensures we ignore a part.
        /// </summary>
        [Test]
        public void IgnorePart()
        {
            ChaskisEventHandler handler = new ChaskisEventHandler(
                Regexes.ChaskisIrcDisconnectEvent,
                ChaskisEventProtocol.IRC,
                this.LineAction
            );

            string ircString = TestHelpers.ConstructIrcString(
                "user",
                PartHandler.IrcCommand,
                "#channel",
                null
            );
            HandlerArgs handlerArgs = this.CreateHandlerArgs( ircString );
            handler.HandleEvent( handlerArgs );

            Assert.IsNull( this.argsCaptured );
        }

        /// <summary>
        /// Ensures we ignore a message.
        /// </summary>
        [Test]
        public void IgnoreMessage()
        {
            ChaskisEventHandler handler = new ChaskisEventHandler(
                Regexes.ChaskisIrcDisconnectEvent,
                ChaskisEventProtocol.IRC,
                this.LineAction
            );

            string ircString = TestHelpers.ConstructIrcString(
                "user",
                MessageHandler.IrcCommand,
                "#channel",
                "This is my message!"
            );
            HandlerArgs handlerArgs = this.CreateHandlerArgs( ircString );
            handler.HandleEvent( handlerArgs );

            Assert.IsNull( this.argsCaptured );
        }

        /// <summary>
        /// Ensures we ignore a ping.
        /// </summary>
        [Test]
        public void IgnorePing()
        {
            ChaskisEventHandler handler = new ChaskisEventHandler(
                Regexes.ChaskisIrcDisconnectEvent,
                ChaskisEventProtocol.IRC,
                this.LineAction
            );

            string ircString = TestHelpers.ConstructPingString( "irc.somewhere.com" );

            HandlerArgs handlerArgs = this.CreateHandlerArgs( ircString );
            handler.HandleEvent( handlerArgs );

            Assert.IsNull( this.argsCaptured );
        }

        /// <summary>
        /// Ensures we ignore a pong.
        /// </summary>
        [Test]
        public void IgnorePong()
        {
            ChaskisEventHandler handler = new ChaskisEventHandler(
                Regexes.ChaskisIrcDisconnectEvent,
                ChaskisEventProtocol.IRC,
                this.LineAction
            );

            string ircString = TestHelpers.ConstringPongString( "irc.somewhere.com", "myMessage" );

            HandlerArgs handlerArgs = this.CreateHandlerArgs( ircString );
            handler.HandleEvent( handlerArgs );

            Assert.IsNull( this.argsCaptured );
        }

        // ---------------- Test Helpers -----------------

        private void LineAction( ChaskisEventHandlerLineActionArgs args )
        {
            this.argsCaptured = args;
        }

        private HandlerArgs CreateHandlerArgs( string line )
        {
            HandlerArgs handlerArgs = new HandlerArgs();
            handlerArgs.IrcConfig = this.ircConfig;
            handlerArgs.Line = line;
            handlerArgs.BlackListedChannels = null;
            handlerArgs.IrcWriter = this.mockWriter.Object;

            return handlerArgs;
        }
    }
}
