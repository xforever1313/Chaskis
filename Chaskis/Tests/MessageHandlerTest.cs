//
//          Copyright Seth Hendrick 2016-2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Threading;
using ChaskisCore;
using Moq;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class MessageHandlerTest
    {
        // -------- Fields --------

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
        /// The user that joined.
        /// </summary>
        private const string remoteUser = "remoteuser";

        /// <summary>
        /// The message the user sends.
        /// </summary>
        private const string defaultMessage = "This is a message!";

        // -------- Setup / Teardown --------

        [SetUp]
        public void TestSetup()
        {
            this.ircConfig = TestHelpers.GetTestIrcConfig();
            this.ircWriter = new Mock<IIrcWriter>( MockBehavior.Strict );
            this.responseReceived = null;
        }

        // -------- Tests --------

        /// <summary>
        /// Ensures that the class is created correctly.
        /// </summary>
        [Test]
        public void ConstructionTest()
        {
            MessageHandler uut = new MessageHandler(
                @"!bot\s+help",
                this.MessageFunction
            );

            // Keep Handling should be true by default.
            Assert.IsTrue( uut.KeepHandling );
        }

        /// <summary>
        /// Ensures a good message goes through.
        /// </summary>
        [Test]
        public void TestGoodMessage()
        {
            MessageHandler uut = new MessageHandler(
                @"!bot\s+help",
                this.MessageFunction
            );

            const string expectedMessage = "!bot help";

            uut.HandleEvent(
                this.GenerateMessage( remoteUser, this.ircConfig.Channel, expectedMessage ),
                this.ircConfig,
                this.ircWriter.Object
            );

            Assert.AreEqual( this.ircConfig.Channel, this.responseReceived.Channel );
            Assert.AreEqual( remoteUser, this.responseReceived.RemoteUser );
            Assert.AreEqual( expectedMessage, this.responseReceived.Message );
        }

        /// <summary>
        /// Ensures that if a message goes through before the cool down,
        /// it will not be launched.
        /// </summary>
        [Test]
        public void TestGoodMessageBeforeCoolDown()
        {
            const string expectedMessage = "!bot hello";

            MessageHandler uut = new MessageHandler(
                @"(!bot\s+help)|(!bot\s+hello)",
                this.MessageFunction,
                10000
            );

            uut.HandleEvent(
                this.GenerateMessage( remoteUser, this.ircConfig.Channel, expectedMessage ),
                this.ircConfig,
                this.ircWriter.Object
            );

            // Quickly send it out.  Nothing should happen since the cooldown is so high.
            uut.HandleEvent(
                this.GenerateMessage( remoteUser, this.ircConfig.Channel, "!bot help" ),
                this.ircConfig,
                this.ircWriter.Object
            );

            Assert.AreEqual( this.ircConfig.Channel, this.responseReceived.Channel );
            Assert.AreEqual( remoteUser, this.responseReceived.RemoteUser );
            Assert.AreEqual( expectedMessage, this.responseReceived.Message );
        }

        /// <summary>
        /// Ensures that if a message goes through after the cool down,
        /// it will be launched.
        /// </summary>
        [Test]
        public void TestGoodMessageAfterCoolDown()
        {
            const string expectedMessage = "!bot help";

            MessageHandler uut = new MessageHandler(
                @"(!bot\s+help)|(!bot\s+hello)",
                this.MessageFunction,
                1
            );

            uut.HandleEvent(
                this.GenerateMessage( remoteUser, this.ircConfig.Channel, "!bot hello" ),
                this.ircConfig,
                this.ircWriter.Object
            );

            Thread.Sleep( 2000 );

            // Quickly send it out.  Nothing should happen since the cooldown is so high.
            uut.HandleEvent(
                this.GenerateMessage( remoteUser, this.ircConfig.Channel, expectedMessage ),
                this.ircConfig,
                this.ircWriter.Object
            );

            Assert.AreEqual( this.ircConfig.Channel, this.responseReceived.Channel );
            Assert.AreEqual( remoteUser, this.responseReceived.RemoteUser );
            Assert.AreEqual( expectedMessage, this.responseReceived.Message );
        }

        /// <summary>
        /// Ensures a message that does not fit the pattern does not go through.
        /// </summary>
        [Test]
        public void TestMisMatchedMessage()
        {
            MessageHandler uut = new MessageHandler(
                @"!bot\s+help",
                this.MessageFunction
            );

            // Does not match pattern.  No response expected.
            const string expectedMessage = "hello world!";

            uut.HandleEvent(
                this.GenerateMessage( remoteUser, this.ircConfig.Channel, expectedMessage ),
                this.ircConfig,
                this.ircWriter.Object
            );

            Assert.IsNull( this.responseReceived );
        }

        /// <summary>
        /// Tests to make sure the bridge bots work.
        /// </summary>
        [Test]
        public void TestBridgeUser()
        {
            const string expectedMessage = "!bot help";

            MessageHandler uut = new MessageHandler(
                @"!bot\s+help",
               this.MessageFunction
            );

            uut.HandleEvent(
                this.GenerateMessage( TestHelpers.BridgeBotUser, this.ircConfig.Channel, remoteUser + ": " + expectedMessage ),
                this.ircConfig,
                this.ircWriter.Object
            );

            Assert.AreEqual( this.ircConfig.Channel, this.responseReceived.Channel );
            Assert.AreEqual( remoteUser, this.responseReceived.RemoteUser );
            Assert.AreEqual( expectedMessage, this.responseReceived.Message );
        }

        /// <summary>
        /// Tests to make sure bridge bots work with regexes.
        /// Useful if the bridge bot leaves and rejoins with a number tacked on the end.
        /// </summary>
        [Test]
        public void TestBridgeUserRegex()
        {
            const string expectedMessage = "!bot help";

            this.ircConfig.BridgeBots.Remove( TestHelpers.BridgeBotUser );
            this.ircConfig.BridgeBots.Add( TestHelpers.BridgeBotUser + @"\d*", @"(?<bridgeUser>[\w\d]+):\s+(?<bridgeMessage>.+)" );

            MessageHandler uut = new MessageHandler(
                @"!bot\s+help",
                MessageFunction
            );

            for( int i = 0; i < 5; ++i )
            {
                string bridgeBotNick;
                if( i == 0 )
                {
                    bridgeBotNick = TestHelpers.BridgeBotUser;
                }
                else
                {
                    bridgeBotNick = TestHelpers.BridgeBotUser + ( 10 * i );
                }

                uut.HandleEvent(
                    this.GenerateMessage( bridgeBotNick, this.ircConfig.Channel, remoteUser + ": " + expectedMessage ),
                    this.ircConfig,
                    this.ircWriter.Object
                );

                Assert.AreEqual( this.ircConfig.Channel, this.responseReceived.Channel );
                Assert.AreEqual( remoteUser, this.responseReceived.RemoteUser );
                Assert.AreEqual( expectedMessage, this.responseReceived.Message );
            }
        }

        /// <summary>
        /// Tests to make sure bridge bots work with regexes with multiple bridge bots.
        /// </summary>
        [Test]
        public void TestBridgeUserRegexMultipleBridgeBots()
        {
            const string expectedMessage = "!bot help";
            const string bridgeBotUser2 = "slackbot";

            this.ircConfig.BridgeBots.Remove( TestHelpers.BridgeBotUser );
            this.ircConfig.BridgeBots.Add( TestHelpers.BridgeBotUser, @"(?<bridgeUser>\w+):\s+(?<bridgeMessage>.+)" );
            this.ircConfig.BridgeBots.Add( bridgeBotUser2 + @"\d*", @"\<(?<bridgeUser>[\w\d]+)\>\s+(?<bridgeMessage>.+)" );

            MessageHandler uut = new MessageHandler(
                @"!bot\s+help",
                this.MessageFunction
            );

            for( int i = 0; i < 5; ++i )
            {
                string bridgeBotNick;
                if( i == 0 )
                {
                    bridgeBotNick = bridgeBotUser2;
                }
                else
                {
                    bridgeBotNick = bridgeBotUser2 + i;
                }

                // This is the wrong regex, ensure the remote user is set to the bot, not the bridged user.
                {
                    string expectedReceivedMessage = remoteUser + ": " + expectedMessage;
                    uut.HandleEvent(
                        this.GenerateMessage( bridgeBotNick, this.ircConfig.Channel, expectedReceivedMessage ),
                        this.ircConfig,
                        this.ircWriter.Object
                    );

                    Assert.AreEqual( this.ircConfig.Channel, this.responseReceived.Channel );
                    Assert.AreEqual( bridgeBotNick, this.responseReceived.RemoteUser );
                    Assert.AreEqual( expectedReceivedMessage, this.responseReceived.Message );

                    this.responseReceived = null;
                }

                // Next, use the right regex.
                {
                    uut.HandleEvent(
                        this.GenerateMessage( bridgeBotNick, this.ircConfig.Channel, "<" + remoteUser + "> " + expectedMessage ),
                        this.ircConfig,
                        this.ircWriter.Object
                    );

                    Assert.AreEqual( this.ircConfig.Channel, this.responseReceived.Channel );
                    Assert.AreEqual( remoteUser, this.responseReceived.RemoteUser );
                    Assert.AreEqual( expectedMessage, this.responseReceived.Message );

                    this.responseReceived = null;
                }
            }
        }

        /// <summary>
        /// Tests to make sure the bridge bots work if
        /// the message matches the bridge bot user.
        /// </summary>
        [Test]
        public void TestBridgeUserMessageIsSameAsBotName()
        {
            const string expectedMessage = TestHelpers.BridgeBotUser;

            MessageHandler uut = new MessageHandler(
                @".+",
                this.MessageFunction
            );

            uut.HandleEvent(
                this.GenerateMessage( TestHelpers.BridgeBotUser, this.ircConfig.Channel, remoteUser + ": " + expectedMessage ),
                this.ircConfig,
                this.ircWriter.Object
            );

            Assert.AreEqual( this.ircConfig.Channel, this.responseReceived.Channel );
            Assert.AreEqual( remoteUser, this.responseReceived.RemoteUser );
            Assert.AreEqual( expectedMessage, this.responseReceived.Message );
        }

        /// <summary>
        /// Tests to make sure the bridge bots work if
        /// the message matches the remote user.
        /// </summary>
        [Test]
        public void TestBridgeUserMessageIsSameAsRemoteName()
        {
            const string expectedMessage = remoteUser;

            MessageHandler uut = new MessageHandler(
                @".+",
                this.MessageFunction
            );

            uut.HandleEvent(
                this.GenerateMessage( TestHelpers.BridgeBotUser, this.ircConfig.Channel, remoteUser + ": " + expectedMessage ),
                this.ircConfig,
                this.ircWriter.Object
            );

            Assert.AreEqual( this.ircConfig.Channel, this.responseReceived.Channel );
            Assert.AreEqual( remoteUser, this.responseReceived.RemoteUser );
            Assert.AreEqual( expectedMessage, this.responseReceived.Message );
        }

        /// <summary>
        /// Tests to make sure the bridge bots do not respond if the regex does not match..
        /// </summary>
        [Test]
        public void TestBridgeUserNoResponse()
        {
            const string expectedMessage = "hello world!";

            MessageHandler uut = new MessageHandler(
                @"!bot\s+help",
                this.MessageFunction
            );

            uut.HandleEvent(
                this.GenerateMessage( TestHelpers.BridgeBotUser, this.ircConfig.Channel, remoteUser + ": " + expectedMessage ),
                this.ircConfig,
                this.ircWriter.Object
            );

            Assert.IsNull( this.responseReceived );
        }

        /// <summary>
        /// Ensures that if a bridge bot message
        /// sends a message not in the form of the regex its designed to produce
        /// messages in, Chaskis creates a response object with the bridge bot
        /// as the user.
        /// </summary>
        [Test]
        public void TestBridgeBotNotBridgedMessage()
        {
            MessageHandler uut = new MessageHandler(
                @"!bot\s+help",
                this.MessageFunction
            );

            const string expectedMessage = "!bot help";

            uut.HandleEvent(
                this.GenerateMessage( TestHelpers.BridgeBotUser, this.ircConfig.Channel, expectedMessage ),
                this.ircConfig,
                this.ircWriter.Object
            );

            Assert.AreEqual( this.ircConfig.Channel, this.responseReceived.Channel );
            Assert.AreEqual( TestHelpers.BridgeBotUser, this.responseReceived.RemoteUser );
            Assert.AreEqual( expectedMessage, this.responseReceived.Message );
        }

        /// <summary>
        /// Ensures {%user%}, {%channel%}, and {%nick%} get replaced in the line
        /// regex properly.
        /// </summary>
        [Test]
        public void TestLiquification()
        {
            MessageHandler uut = new MessageHandler(
                @"!{%nick%} {%channel%} {%user%}",
                this.MessageFunction
            );

            string expectedMessage = string.Format(
                "!{0} {1} {2}",
                this.ircConfig.Nick,
                this.ircConfig.Channel,
                remoteUser
            );

            uut.HandleEvent(
                this.GenerateMessage( remoteUser, this.ircConfig.Channel, expectedMessage ),
                this.ircConfig,
                this.ircWriter.Object
            );

            Assert.IsNotNull( this.responseReceived );
            Assert.AreEqual( this.ircConfig.Channel, this.responseReceived.Channel );
            Assert.AreEqual( remoteUser, this.responseReceived.RemoteUser );
            Assert.AreEqual( expectedMessage, this.responseReceived.Message );
        }

        /// <summary>
        /// Ensures {%user%}, {%channel%}, and {%nick%} get replaced in the line
        /// regex properly with bridge bots.
        /// </summary>
        [Test]
        public void TestLiquificationWithBridgeBots()
        {
            MessageHandler uut = new MessageHandler(
                @"!{%nick%} {%channel%} {%user%}",
                this.MessageFunction
            );

            string expectedMessage = string.Format(
                "!{0} {1} {2}",
                this.ircConfig.Nick,
                this.ircConfig.Channel,
                remoteUser
            );

            uut.HandleEvent(
                this.GenerateMessage( TestHelpers.BridgeBotUser, this.ircConfig.Channel, remoteUser + ": " + expectedMessage ),
                this.ircConfig,
                this.ircWriter.Object
            );

            Assert.AreEqual( this.ircConfig.Channel, this.responseReceived.Channel );
            Assert.AreEqual( remoteUser, this.responseReceived.RemoteUser );
            Assert.AreEqual( expectedMessage, this.responseReceived.Message );
        }

        // -------- Test Helpers --------

        /// <summary>
        /// Generates the message from IRC.
        /// </summary>
        /// <param name="nick">The nick name that sent the message</param>
        /// <param name="channel">The channel it was sent on.</param>
        /// <param name="message">The message that was sent.</param>
        /// <returns>The string from an IRC server.</returns>
        private string GenerateMessage( string nick, string channel, string message )
        {
            return TestHelpers.ConstructMessageString( nick, channel, message );
        }

        /// <summary>
        /// The function that is called
        /// </summary>
        /// <param name="writer">The writer that can be written to.</param>
        /// <param name="response">The response from the server.</param>
        private void MessageFunction( IIrcWriter writer, IrcResponse response )
        {
            Assert.AreSame( this.ircWriter.Object, writer );
            this.responseReceived = response;
        }
    }
}