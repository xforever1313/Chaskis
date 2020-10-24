//
//          Copyright Seth Hendrick 2017-2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using Chaskis.UnitTests.Common;
using Chaskis.Core;
using Moq;
using NUnit.Framework;

namespace Chaskis.UnitTests.CoreTests.Handlers.Pong
{
    [TestFixture]
    public class PongHandlerTest
    {
        // -------- Fields --------

        /// <summary>
        /// Unit under test.
        /// </summary>
        private PongHandler uut;

        /// <summary>
        /// Irc Config to use.
        /// </summary>
        private IrcConfig ircConfig;

        /// <summary>
        /// The mock IRC connection to use.
        /// </summary>
        private Mock<IIrcWriter> ircWriter;

        /// <summary>
        /// The user that joined.
        /// </summary>
        private const string remoteUser = "remoteuser";

        // -------- Setup / Teardown --------

        [SetUp]
        public void TestSetup()
        {
            this.ircConfig = TestHelpers.GetTestIrcConfig();
            this.ircWriter = new Mock<IIrcWriter>( MockBehavior.Strict );

            this.uut = new PongHandler();
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
                this.uut.HandleEvent( null )
            );
        }

        /// <summary>
        /// Ensures the bot handles the PONG.
        /// </summary>
        [Test]
        public void PongTest()
        {
            const string server = "irc.shendrick.net";
            const string msg = "watchdog"; // What message we ping

            this.ircWriter.Setup( i => i.ReceivedPong( It.Is<string>( s => s == msg ) ) );

            string ircString = TestHelpers.ConstringPongString( server, msg );

            this.uut.HandleEvent( this.ConstructArgs( ircString ) );
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
                    remoteUser,
                    PrivateMessageHelper.IrcCommand,
                    ircConfig.Channels[0],
                    "A message"
                );

            this.uut.HandleEvent( this.ConstructArgs( ircString ) );
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
                    remoteUser,
                    PartHandler.IrcCommand,
                    ircConfig.Channels[0],
                    string.Empty
                );

            this.uut.HandleEvent( this.ConstructArgs( ircString ) );
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
                    remoteUser,
                    JoinHandler.IrcCommand,
                    ircConfig.Channels[0],
                    string.Empty
                );

            this.uut.HandleEvent( this.ConstructArgs( ircString ) );
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
