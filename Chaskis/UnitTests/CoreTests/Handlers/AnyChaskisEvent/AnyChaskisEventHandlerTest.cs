﻿//
//          Copyright Seth Hendrick 2020.
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

namespace Chaskis.UnitTests.CoreTests.Handlers.AnyChaskisEvent
{
    [TestFixture]
    public class AnyChaskisEventHandlerTest
    {
        // -------- Fields --------

        /// <summary>
        /// Unit Under Test.
        /// </summary>
        private AnyChaskisEventHandler uut;

        /// <summary>
        /// Irc Config to use.
        /// </summary>
        private IrcConfig ircConfig;

        /// <summary>
        /// Mock IRC writer to use.
        /// </summary>
        private Mock<IIrcWriter> ircWriter;

        /// <summary>
        /// The response AnyChaskisEventd from the event handler (if any).
        /// </summary>
        private AnyChaskisEventHandlerArgs responseReceived;

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

            AnyChaskisEventHandlerConfig allHandlerConfig = new AnyChaskisEventHandlerConfig
            {
                LineAction = this.AllFunction
            };
            this.uut = new AnyChaskisEventHandler( allHandlerConfig );
        }

        // -------- Tests --------

        /// <summary>
        /// Ensures that if a bad config is passed in, we throw an exception.
        /// </summary>
        [Test]
        public void InvalidConfigTest()
        {
            Assert.Throws<ValidationException>(
                () => new AnyChaskisEventHandler( new AnyChaskisEventHandlerConfig() )
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
                new AnyChaskisEventHandler( null )
            );

            Assert.Throws<ArgumentNullException>( () =>
                this.uut.HandleEvent( null )
            );

            Assert.IsNull( this.responseReceived ); // Ensure handler didn't get called.
        }

        /// <summary>
        /// Ensures that if the bot parts, the event is not fired.
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
            Assert.IsNull( this.responseReceived );
        }

        /// <summary>
        /// Ensures that if a PRIMSG appears, the event is not fired.
        /// </summary>
        [Test]
        public void MessageCommandAppears()
        {
            string ircString = TestHelpers.ConstructIrcString(
                remoteUser,
                PrivateMessageHelper.IrcCommand,
                this.ircConfig.Channels[0],
                "A message"
            );

            this.uut.HandleEvent( this.ConstructArgs( ircString ) );
            Assert.IsNull( this.responseReceived );
        }

        /// <summary>
        /// Ensures that if a JOIN appears, the event is not fired.
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
            Assert.IsNull( this.responseReceived );
        }

        /// <summary>
        /// Ensures that if a PING appears, the event is not fired.
        /// </summary>
        [Test]
        public void PingAppears()
        {
            string ircString = TestHelpers.ConstructPingString( "12345" );
            this.uut.HandleEvent( this.ConstructArgs( ircString ) );
            Assert.IsNull( this.responseReceived );
        }

        [Test]
        public void SendNoticeAppears()
        {
            SendNoticeEventArgs e = new SendNoticeEventArgs
            {
                Protocol = ChaskisEventProtocol.IRC,
                Server = "irc.somewhere.net",
                Writer = this.ircWriter.Object,

                ChannelOrUser = "#channel",
                Message = "A message"
            };

            string eventString = e.ToXml();
            this.uut.HandleEvent( this.ConstructArgs( eventString ) );
            this.CheckResponse( eventString );
        }

        [Test]
        public void SendJoinAppears()
        {
            SendJoinEventArgs e = new SendJoinEventArgs
            {
                Protocol = ChaskisEventProtocol.IRC,
                Server = "irc.somewhere.net",
                Writer = this.ircWriter.Object
            };

            string eventString = e.ToXml();
            this.uut.HandleEvent( this.ConstructArgs( eventString ) );
            this.CheckResponse( eventString );
        }

        // -------- Test Helpers --------

        /// <summary>
        /// The function that is called
        /// </summary>
        /// <param name="writer">The writer that can be written to.</param>
        /// <param name="response">The response from the server.</param>
        private void AllFunction( AnyChaskisEventHandlerArgs args )
        {
            Assert.AreSame( this.ircWriter.Object, args.Writer );
            this.responseReceived = args;
        }

        /// <summary>
        /// Ensures the bot responds accordingly.
        /// </summary>
        /// <param name="rawString">The raw string we expect.</param>
        private void CheckResponse( string rawString )
        {
            Assert.IsNotNull( this.responseReceived );

            // Ensure its the raw string.
            Assert.AreEqual( rawString, this.responseReceived.Line );
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
    }
}