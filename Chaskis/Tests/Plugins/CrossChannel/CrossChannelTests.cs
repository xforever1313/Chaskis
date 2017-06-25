//
//          Copyright Seth Hendrick 2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;
using ChaskisCore;
using Moq;
using NUnit.Framework;

namespace Tests.Plugins.CrossChannel
{
    [TestFixture]
    public class CrossChannelTests
    {
        // ---------------- Fields ----------------

        private Mock<IIrcWriter> mockWriter;

        private IrcConfig ircConfig;

        private Chaskis.Plugins.CrossChannel.CrossChannel uut;

        private const string remoteUser = "auser";

        // ---------------- Setup / Teardown ----------------

        [SetUp]
        public void TestSetup()
        {
            this.mockWriter = new Mock<IIrcWriter>( MockBehavior.Strict );
            this.ircConfig = TestHelpers.GetTestIrcConfig();
            this.ircConfig.Channels.Add( "#channel2" );

            this.uut = new Chaskis.Plugins.CrossChannel.CrossChannel();

            PluginInitor initor = new PluginInitor();
            initor.IrcConfig = this.ircConfig;
            this.uut.Init( initor );
        }

        [TearDown]
        public void TestTeardown()
        {
            this.uut?.Dispose();
        }

        // ---------------- Tests ----------------

        /// <summary>
        /// Ensures our message gets broadcasted to all channels the bot is in.
        /// </summary>
        [Test]
        public void BCastTest()
        {
            string message = "My Message!";
            string expectedMessage = this.GetExpectedString( message, this.ircConfig.Channels[0] );

            this.mockWriter.Setup(
                w => w.SendBroadcastMessage( expectedMessage )
            );

            IIrcHandler bcastHandler = this.uut.GetHandlers()[0];

            HandlerArgs args = this.GetHandlerArgs( "!broadcast " + message, this.ircConfig.Channels[0] );
            bcastHandler.HandleEvent( args );

            this.mockWriter.VerifyAll();
        }

        /// <summary>
        /// Ensures our event does NOT occur if our regex does not match.
        /// </summary>
        [Test]
        public void BadBCastCommandTest()
        {
            string message = "My Message!";

            IIrcHandler bcastHandler = this.uut.GetHandlers()[0];

            HandlerArgs args = this.GetHandlerArgs( " !broadcast " + message, this.ircConfig.Channels[0] );
            bcastHandler.HandleEvent( args );

            this.mockWriter.VerifyAll();
        }

        /// <summary>
        /// Ensures our message gets broadcasted to all channels the bot is in.
        /// </summary>
        [Test]
        public void CcTest()
        {
            string message = "My Message!";
            string expectedMessage = this.GetExpectedString( message, this.ircConfig.Channels[0] );

            this.mockWriter.Setup(
                w => w.SendMessage( expectedMessage, this.ircConfig.Channels[1] )
            );

            IIrcHandler ccHandler = this.uut.GetHandlers()[1];

            HandlerArgs args = this.GetHandlerArgs(
                "!cc <" + this.ircConfig.Channels[1] + "> " + message,
                this.ircConfig.Channels[0]
            );
            ccHandler.HandleEvent( args );

            this.mockWriter.VerifyAll();
        }

        /// <summary>
        /// Ensures our message gets broadcasted to all channels the bot is in.
        /// </summary>
        [Test]
        public void CcTestChannelDoesNotExist()
        {
            string dummyChannel = "#dumb";
            string message = "My Message!";
            string expectedMessage = string.Format(
                "@{0}: I am not in {1}, sorry :(",
                remoteUser,
                dummyChannel
            );

            this.mockWriter.Setup(
                w => w.SendMessage( expectedMessage, this.ircConfig.Channels[0] )
            );

            IIrcHandler ccHandler = this.uut.GetHandlers()[1];

            HandlerArgs args = this.GetHandlerArgs(
                "!cc <" + dummyChannel + "> " + message,
                this.ircConfig.Channels[0]
            );
            ccHandler.HandleEvent( args );

            this.mockWriter.VerifyAll();
        }

        /// <summary>
        /// Ensures our event does NOT occur if our regex does not match.
        /// </summary>
        [Test]
        public void BadCcCommandTest()
        {
            string message = "My Message!";

            IIrcHandler ccHandler = this.uut.GetHandlers()[1];

            HandlerArgs args = this.GetHandlerArgs(
                " !cc <" + this.ircConfig.Channels[1] + "> " + message, // Note the whitespace.
                this.ircConfig.Channels[0]
            );
            ccHandler.HandleEvent( args );

            this.mockWriter.VerifyAll();
        }

        /// <summary>
        /// Gets the expected string the bot should sendout.
        /// </summary>
        /// <param name="message">The broadcasted message.</param>
        /// <param name="channel">The channel things were broadcasted from.</param>
        private string GetExpectedString( string message, string channel )
        {
            return string.Format(
                "<{0}@{1}> {2}",
                remoteUser,
                channel,
                message
            );
        }

        /// <summary>
        /// Gets the handler args.
        /// </summary>
        /// <param name="message">The message we received from the server.</param>
        /// <param name="channel">The channel we received the message from.</param>
        /// <returns>The handler args.</returns>
        private HandlerArgs GetHandlerArgs( string message, string channel )
        {
            HandlerArgs args = new HandlerArgs();

            args.BlackListedChannels = new List<string>(); // No blacklisted channels.
            args.IrcConfig = this.ircConfig;
            args.IrcWriter = this.mockWriter.Object;
            args.Line = TestHelpers.ConstructMessageString( remoteUser, channel, message );

            return args;
        }
    }
}
