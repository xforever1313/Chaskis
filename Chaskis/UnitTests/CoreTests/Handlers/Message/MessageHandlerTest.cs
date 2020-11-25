//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;
using System.Threading;
using Chaskis.UnitTests.Common;
using Chaskis.Core;
using Moq;
using NUnit.Framework;
using SethCS.Exceptions;

namespace Chaskis.UnitTests.CoreTests.Handlers.Message
{
    [TestFixture]
    public class MessageHandlerTest
    {
        // ---------------- Fields ----------------

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
        private MessageHandlerArgs responseReceived;

        /// <summary>
        /// The user that joined.
        /// </summary>
        private const string remoteUser = "remoteuser";

        // ---------------- Setup / Teardown ----------------

        [SetUp]
        public void TestSetup()
        {
            this.ircConfig = TestHelpers.GetTestIrcConfig();
            this.ircWriter = new Mock<IIrcWriter>( MockBehavior.Strict );
            this.responseReceived = null;
        }

        // ---------------- Tests ----------------

        /// <summary>
        /// Ensures that if a bad config is passed in, we throw an exception.
        /// </summary>
        [Test]
        public void InvalidConfigTest()
        {
            Assert.Throws<ListedValidationException>(
                () => new MessageHandler( new MessageHandlerConfig() )
            );
        }

        /// <summary>
        /// Ensures that the class is created correctly.
        /// </summary>
        [Test]
        public void ConstructionTest()
        {
            MessageHandlerConfig config = new MessageHandlerConfig
            {
                LineRegex = @"!bot\s+help",
                LineAction = this.MessageFunction
            };

            MessageHandler uut = new MessageHandler(
                config
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
            this.DoGoodMessageTest( remoteUser, this.ircConfig.Channels[0] );
        }

        /// <summary>
        /// Ensures good messages work if the users have strange names.
        /// </summary>
        [Test]
        public void TestGoodMessageWithStrangeNames()
        {
            foreach( string name in TestHelpers.StrangeNames )
            {
                this.DoGoodMessageTest( name, this.ircConfig.Channels[0] );
            }
        }

        /// <summary>
        /// Ensures good messages work if the channels have strange names.
        /// </summary>
        [Test]
        public void TestGoodMessageWithStrangeChannelNames()
        {
            // Do channels.
            foreach( string channel in TestHelpers.StrangeChannels )
            {
                this.DoGoodMessageTest( remoteUser, channel );
            }

            // Do users (users can be a channel for a private message).
            foreach( string channel in TestHelpers.StrangeNames )
            {
                this.DoGoodMessageTest( remoteUser, channel );
            }
        }

        /// <summary>
        /// Ensures we handle all prefixes.
        /// </summary>
        [Test]
        public void MessagetPrefixTest()
        {
            MessageHandlerConfig config = new MessageHandlerConfig
            {
                LineRegex = @"!bot\s+help",
                LineAction = this.MessageFunction
            };

            MessageHandler uut = new MessageHandler(
                config
            );

            const string channel = "#somechannel";
            const string expectedMessage = "!bot help";

            foreach( string prefix in TestHelpers.PrefixTests )
            {
                string ircString = prefix + " " + PrivateMessageHelper.IrcCommand + " " + channel + " :" + expectedMessage;

                uut.HandleEvent( this.ConstructArgs( ircString ) );

                Assert.IsNotNull( this.responseReceived );

                // Part handler has no message.
                Assert.AreEqual( expectedMessage, this.responseReceived.Message );

                // Channels should match.
                Assert.AreEqual( channel, this.responseReceived.Channel );

                // Nicks should match.
                Assert.AreEqual( "anickname", this.responseReceived.User );
            }
        }

        /// <summary>
        /// Ensures that if a message goes through before the cool down,
        /// it will not be launched.
        /// </summary>
        [Test]
        public void TestGoodMessageBeforeCoolDown()
        {
            const string expectedMessage = "!bot hello";

            MessageHandlerConfig config = new MessageHandlerConfig
            {
                LineRegex = @"(!bot\s+help)|(!bot\s+hello)",
                LineAction = this.MessageFunction,
                CoolDown = 10000
            };

            MessageHandler uut = new MessageHandler(
                config
            );

            string ircString = this.GenerateMessage( remoteUser, this.ircConfig.Channels[0], expectedMessage );

            uut.HandleEvent( this.ConstructArgs( ircString ) );

            // Quickly send it out.  Nothing should happen since the cooldown is so high.
            ircString = this.GenerateMessage( remoteUser, this.ircConfig.Channels[0], "!bot help" );
            uut.HandleEvent( this.ConstructArgs( ircString ) );

            Assert.AreEqual( this.ircConfig.Channels[0], this.responseReceived.Channel );
            Assert.AreEqual( remoteUser, this.responseReceived.User );
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

            MessageHandlerConfig config = new MessageHandlerConfig
            {
                LineRegex = @"(!bot\s+help)|(!bot\s+hello)",
                LineAction = this.MessageFunction,
                CoolDown = 1
            };

            MessageHandler uut = new MessageHandler(
                config
            );

            string ircString = this.GenerateMessage( remoteUser, this.ircConfig.Channels[0], "!bot hello" );
            uut.HandleEvent( this.ConstructArgs( ircString ) );

            Thread.Sleep( 2000 );

            // Quickly send it out.  Nothing should happen since the cooldown is so high.
            ircString = this.GenerateMessage( remoteUser, this.ircConfig.Channels[0], expectedMessage );
            uut.HandleEvent( this.ConstructArgs( ircString ) );

            Assert.AreEqual( this.ircConfig.Channels[0], this.responseReceived.Channel );
            Assert.AreEqual( remoteUser, this.responseReceived.User );
            Assert.AreEqual( expectedMessage, this.responseReceived.Message );
        }

        /// <summary>
        /// Ensures cool downs are on a per-channel basis.
        /// </summary>
        [Test]
        public void CooldownMultiChannelTest()
        {
            const string expectedMessage = "!bot help";

            MessageHandlerConfig config = new MessageHandlerConfig
            {
                LineRegex = @"(!bot\s+help)|(!bot\s+hello)",
                LineAction = this.MessageFunction,
                CoolDown = int.MaxValue
            };

            MessageHandler uut = new MessageHandler(
                config
            );

            this.ircConfig.Channels.Add( this.ircConfig.Channels[0] + "2" );

            // Fire on channel 1.
            {
                string ircString = this.GenerateMessage( remoteUser, this.ircConfig.Channels[0], expectedMessage );
                uut.HandleEvent( this.ConstructArgs( ircString ) );

                Assert.IsNotNull( this.responseReceived );
                Assert.AreEqual( this.ircConfig.Channels[0], this.responseReceived.Channel ); // <- Should fire.
                Assert.AreEqual( remoteUser, this.responseReceived.User );
                Assert.AreEqual( expectedMessage, this.responseReceived.Message );
                this.responseReceived = null;
            }

            // Fire on channel 2.
            {
                string ircString = this.GenerateMessage( remoteUser, this.ircConfig.Channels[1], expectedMessage );
                uut.HandleEvent( this.ConstructArgs( ircString ) );

                Assert.IsNotNull( this.responseReceived );
                Assert.AreEqual( this.ircConfig.Channels[1], this.responseReceived.Channel ); // <- Should fire.
                Assert.AreEqual( remoteUser, this.responseReceived.User );
                Assert.AreEqual( expectedMessage, this.responseReceived.Message );
                this.responseReceived = null;
            }

            // Fire again on channel 1.  Should not trigger.
            {
                string ircString = this.GenerateMessage( remoteUser, this.ircConfig.Channels[0], expectedMessage );
                uut.HandleEvent( this.ConstructArgs( ircString ) );

                Assert.IsNull( this.responseReceived ); // <- should not fire, cool down has not happened on this channel yet.
            }

            // Fire again on channel 2.  Should not trigger.
            {
                string ircString = this.GenerateMessage( remoteUser, this.ircConfig.Channels[1], expectedMessage );
                uut.HandleEvent( this.ConstructArgs( ircString ) );

                Assert.IsNull( this.responseReceived ); // <- should not fire, cool down has not happened on this channel yet.
            }
        }

        /// <summary>
        /// Ensures a message that does not fit the pattern does not go through.
        /// </summary>
        [Test]
        public void TestMisMatchedMessage()
        {
            MessageHandlerConfig config = new MessageHandlerConfig
            {
                LineRegex = @"!bot\s+help",
                LineAction = this.MessageFunction
            };

            MessageHandler uut = new MessageHandler(
                config
            );

            // Does not match pattern.  No response expected.
            const string expectedMessage = "hello world!";
            string ircString = this.GenerateMessage( remoteUser, this.ircConfig.Channels[0], expectedMessage );
            uut.HandleEvent( this.ConstructArgs( ircString ) );

            Assert.IsNull( this.responseReceived );
        }

        /// <summary>
        /// Ensures that if we get a message from a channel
        /// that is black-listed, nothing happens.
        /// </summary>
        [Test]
        public void BlacklistTest()
        {
            MessageHandlerConfig config = new MessageHandlerConfig
            {
                LineRegex = @"!bot\s+help",
                LineAction = this.MessageFunction
            };

            MessageHandler uut = new MessageHandler(
                config
            );

            const string channel = "#blacklist";
            const string expectedMessage = "!bot help";

            List<string> blackList = new List<string>() { channel };

            string ircString = this.GenerateMessage( remoteUser, channel, expectedMessage );

            HandlerArgs args = this.ConstructArgs( ircString );
            args.BlackListedChannels = blackList;

            uut.HandleEvent( args );

            Assert.IsNull( this.responseReceived );
        }

        /// <summary>
        /// Tests to make sure the bridge bots work.
        /// </summary>
        [Test]
        public void TestBridgeUser()
        {
            const string expectedMessage = "!bot help";

            MessageHandlerConfig config = new MessageHandlerConfig
            {
                LineRegex = @"!bot\s+help",
                LineAction = this.MessageFunction
            };

            MessageHandler uut = new MessageHandler(
                config
            );

            string ircString = this.GenerateMessage( TestHelpers.BridgeBotUser, this.ircConfig.Channels[0], remoteUser + ": " + expectedMessage );
            uut.HandleEvent( this.ConstructArgs( ircString ) );

            Assert.AreEqual( this.ircConfig.Channels[0], this.responseReceived.Channel );
            Assert.AreEqual( remoteUser, this.responseReceived.User );
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

            MessageHandlerConfig config = new MessageHandlerConfig
            {
                LineRegex = @"!bot\s+help",
                LineAction = this.MessageFunction
            };

            MessageHandler uut = new MessageHandler(
                config
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

                string ircString = this.GenerateMessage( bridgeBotNick, this.ircConfig.Channels[0], remoteUser + ": " + expectedMessage );
                uut.HandleEvent( this.ConstructArgs( ircString ) );

                Assert.AreEqual( this.ircConfig.Channels[0], this.responseReceived.Channel );
                Assert.AreEqual( remoteUser, this.responseReceived.User );
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

            MessageHandlerConfig config = new MessageHandlerConfig
            {
                LineRegex = @"!bot\s+help",
                LineAction = this.MessageFunction
            };

            MessageHandler uut = new MessageHandler(
                config
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
                    string ircString = this.GenerateMessage( bridgeBotNick, this.ircConfig.Channels[0], expectedReceivedMessage );
                    uut.HandleEvent( this.ConstructArgs( ircString ) );

                    Assert.AreEqual( this.ircConfig.Channels[0], this.responseReceived.Channel );
                    Assert.AreEqual( bridgeBotNick, this.responseReceived.User );
                    Assert.AreEqual( expectedReceivedMessage, this.responseReceived.Message );

                    this.responseReceived = null;
                }

                // Next, use the right regex.
                {
                    string ircString = this.GenerateMessage( bridgeBotNick, this.ircConfig.Channels[0], "<" + remoteUser + "> " + expectedMessage );
                    uut.HandleEvent( this.ConstructArgs( ircString ) );

                    Assert.AreEqual( this.ircConfig.Channels[0], this.responseReceived.Channel );
                    Assert.AreEqual( remoteUser, this.responseReceived.User );
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

            MessageHandlerConfig config = new MessageHandlerConfig
            {
                LineRegex = @".+",
                LineAction = this.MessageFunction
            };

            MessageHandler uut = new MessageHandler(
                config
            );

            string ircString = this.GenerateMessage( TestHelpers.BridgeBotUser, this.ircConfig.Channels[0], remoteUser + ": " + expectedMessage );
            uut.HandleEvent( this.ConstructArgs( ircString ) );

            Assert.AreEqual( this.ircConfig.Channels[0], this.responseReceived.Channel );
            Assert.AreEqual( remoteUser, this.responseReceived.User );
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

            MessageHandlerConfig config = new MessageHandlerConfig
            {
                LineRegex = @".+",
                LineAction = this.MessageFunction
            };

            MessageHandler uut = new MessageHandler(
                config
            );

            string ircString = this.GenerateMessage( TestHelpers.BridgeBotUser, this.ircConfig.Channels[0], remoteUser + ": " + expectedMessage );
            uut.HandleEvent( this.ConstructArgs( ircString ) );

            Assert.AreEqual( this.ircConfig.Channels[0], this.responseReceived.Channel );
            Assert.AreEqual( remoteUser, this.responseReceived.User );
            Assert.AreEqual( expectedMessage, this.responseReceived.Message );
        }

        /// <summary>
        /// Tests to make sure the bridge bots do not respond if the regex does not match..
        /// </summary>
        [Test]
        public void TestBridgeUserNoResponse()
        {
            const string expectedMessage = "hello world!";

            MessageHandlerConfig config = new MessageHandlerConfig
            {
                LineRegex = @"!bot\s+help",
                LineAction = this.MessageFunction
            };

            MessageHandler uut = new MessageHandler(
                config
            );

            string ircString = this.GenerateMessage( TestHelpers.BridgeBotUser, this.ircConfig.Channels[0], remoteUser + ": " + expectedMessage );
            uut.HandleEvent( this.ConstructArgs( ircString ) );

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
            MessageHandlerConfig config = new MessageHandlerConfig
            {
                LineRegex = @"!bot\s+help",
                LineAction = this.MessageFunction
            };

            MessageHandler uut = new MessageHandler(
               config
            );

            const string expectedMessage = "!bot help";

            string ircString = this.GenerateMessage( TestHelpers.BridgeBotUser, this.ircConfig.Channels[0], expectedMessage );
            uut.HandleEvent( this.ConstructArgs( ircString ) );

            Assert.AreEqual( this.ircConfig.Channels[0], this.responseReceived.Channel );
            Assert.AreEqual( TestHelpers.BridgeBotUser, this.responseReceived.User );
            Assert.AreEqual( expectedMessage, this.responseReceived.Message );
        }

        /// <summary>
        /// Ensures {%user%}, {%channel%}, and {%nick%} get replaced in the line
        /// regex properly.
        /// </summary>
        [Test]
        public void TestLiquification()
        {
            MessageHandlerConfig config = new MessageHandlerConfig
            {
                LineRegex = @"!{%nick%} {%channel%} {%user%}",
                LineAction = this.MessageFunction
            };

            MessageHandler uut = new MessageHandler(
                config
            );

            string expectedMessage = string.Format(
                "!{0} {1} {2}",
                this.ircConfig.Nick,
                this.ircConfig.Channels[0],
                remoteUser
            );

            string ircString = this.GenerateMessage( remoteUser, this.ircConfig.Channels[0], expectedMessage );
            uut.HandleEvent( this.ConstructArgs( ircString ) );

            Assert.IsNotNull( this.responseReceived );
            Assert.AreEqual( this.ircConfig.Channels[0], this.responseReceived.Channel );
            Assert.AreEqual( remoteUser, this.responseReceived.User );
            Assert.AreEqual( expectedMessage, this.responseReceived.Message );
        }

        /// <summary>
        /// Ensures {%user%}, {%channel%}, and {%nick%} get replaced in the line
        /// regex properly with bridge bots.
        /// </summary>
        [Test]
        public void TestLiquificationWithBridgeBots()
        {
            MessageHandlerConfig config = new MessageHandlerConfig
            {
                LineRegex = @"!{%nick%} {%channel%} {%user%}",
                LineAction = this.MessageFunction
            };

            MessageHandler uut = new MessageHandler(
                config
            );

            string expectedMessage = string.Format(
                "!{0} {1} {2}",
                this.ircConfig.Nick,
                this.ircConfig.Channels[0],
                remoteUser
            );

            string ircString = this.GenerateMessage( TestHelpers.BridgeBotUser, this.ircConfig.Channels[0], remoteUser + ": " + expectedMessage );
            uut.HandleEvent( this.ConstructArgs( ircString ) );

            Assert.AreEqual( this.ircConfig.Channels[0], this.responseReceived.Channel );
            Assert.AreEqual( remoteUser, this.responseReceived.User );
            Assert.AreEqual( expectedMessage, this.responseReceived.Message );
        }

        /// <summary>
        /// Ensures if we are set to "PMs Only" we'll get the
        /// PM correctly.
        /// </summary>
        [Test]
        public void TestPmOnlyMessage()
        {
            const string expectedMessage = "!bot help";

            MessageHandlerConfig config = new MessageHandlerConfig
            {
                LineRegex = @"!bot\s+help",
                LineAction = this.MessageFunction,
                ResponseOption = ResponseOptions.PmsOnly
            };

            MessageHandler uut = new MessageHandler(
                config
            );

            string ircString = this.GenerateMessage( remoteUser, this.ircConfig.Nick, expectedMessage );
            uut.HandleEvent( this.ConstructArgs( ircString ) );

            // Need to set the remote user as the channel so the message goes to the right channel.
            Assert.AreEqual( remoteUser, this.responseReceived.Channel );
            Assert.AreEqual( remoteUser, this.responseReceived.User );
            Assert.AreEqual( expectedMessage, this.responseReceived.Message );
        }

        /// <summary>
        /// Ensures that if we are set to channels only we do
        /// NOT respond to PMs.
        /// </summary>
        [Test]
        public void TestPmInChannelOnlyMessage()
        {
            const string expectedMessage = "!bot help";

            MessageHandlerConfig config = new MessageHandlerConfig
            {
                LineRegex = @"!bot\s+help",
                LineAction = this.MessageFunction,
                ResponseOption = ResponseOptions.ChannelOnly
            };

            MessageHandler uut = new MessageHandler(
                config
            );

            string ircString = this.GenerateMessage( remoteUser, this.ircConfig.Nick, expectedMessage );
            uut.HandleEvent( this.ConstructArgs( ircString ) );

            Assert.IsNull( this.responseReceived );
        }

        /// <summary>
        /// Ensures if we are set to "Channel Only" we'll get the
        /// PM correctly.
        /// </summary>
        [Test]
        public void TestChannelOnlyMessage()
        {
            const string expectedMessage = "!bot help";

            MessageHandlerConfig config = new MessageHandlerConfig
            {
                LineRegex = @"!bot\s+help",
                LineAction = this.MessageFunction,
                ResponseOption = ResponseOptions.ChannelOnly
            };

            MessageHandler uut = new MessageHandler(
                config
            );

            string ircString = this.GenerateMessage( remoteUser, this.ircConfig.Channels[0], expectedMessage );
            uut.HandleEvent( this.ConstructArgs( ircString ) );

            Assert.AreEqual( this.ircConfig.Channels[0], this.responseReceived.Channel );
            Assert.AreEqual( remoteUser, this.responseReceived.User );
            Assert.AreEqual( expectedMessage, this.responseReceived.Message );
        }

        /// <summary>
        /// Ensures that if we are set to channels only we do
        /// NOT respond to PMs.
        /// </summary>
        [Test]
        public void TestChannelInPmOnlyMessage()
        {
            const string expectedMessage = "!bot help";

            MessageHandlerConfig config = new MessageHandlerConfig
            {
                LineRegex = @"!bot\s+help",
                LineAction = this.MessageFunction,
                ResponseOption = ResponseOptions.PmsOnly
            };

            MessageHandler uut = new MessageHandler(
                config
            );

            string ircString = this.GenerateMessage( remoteUser, this.ircConfig.Channels[0], expectedMessage );
            uut.HandleEvent( this.ConstructArgs( ircString ) );

            Assert.IsNull( this.responseReceived );
        }

        /// <summary>
        /// Ensures that if RespondToSelf is set to true,
        /// we'll respond to ourself.
        /// </summary>
        [Test]
        public void RespondToSelfTest()
        {
            const string expectedMessage = "!bot help";

            MessageHandlerConfig config = new MessageHandlerConfig
            {
                LineRegex = @"!bot\s+help",
                LineAction = this.MessageFunction,
                RespondToSelf = true
            };

            MessageHandler uut = new MessageHandler(
                config
            );

            string ircString = this.GenerateMessage( this.ircConfig.Nick, this.ircConfig.Nick, expectedMessage );
            uut.HandleEvent( this.ConstructArgs( ircString ) );

            Assert.AreEqual( this.ircConfig.Nick, this.responseReceived.Channel );
            Assert.AreEqual( this.ircConfig.Nick, this.responseReceived.User );
            Assert.AreEqual( expectedMessage, this.responseReceived.Message );
        }

        /// <summary>
        /// Ensures that if RespondToSelf is set to false,
        /// we will NOT respond to ourself.
        /// </summary>
        [Test]
        public void DontRespondToSelfTest()
        {
            const string expectedMessage = "!bot help";

            MessageHandlerConfig config = new MessageHandlerConfig
            {
                LineRegex = @"!bot\s+help",
                LineAction = this.MessageFunction,
                RespondToSelf = false
            };

            MessageHandler uut = new MessageHandler(
                config
            );

            string ircString = this.GenerateMessage( this.ircConfig.Nick, this.ircConfig.Nick, expectedMessage );
            uut.HandleEvent( this.ConstructArgs( ircString ) );

            Assert.IsNull( this.responseReceived );
        }

        [Test]
        public void IgnoreActionTest()
        {
            const string expectedMessage = "\u0001ACTION !bot help\u0001";

            MessageHandlerConfig config = new MessageHandlerConfig
            {
                LineRegex = @".+", // Take any string so if we weren't filtering out ACTION, it would incorrectly trigger
                LineAction = this.MessageFunction,
                RespondToSelf = false
            };

            MessageHandler uut = new MessageHandler(
                config
            );

            string ircString = this.GenerateMessage( remoteUser, this.ircConfig.Channels[0], expectedMessage );
            uut.HandleEvent( this.ConstructArgs( ircString ) );

            Assert.IsNull( this.responseReceived );
        }

        [Test]
        public void IgnoreCtcpPingTest()
        {
            const string expectedMessage = "\u0001PING 1234567890\u0001";

            MessageHandlerConfig config = new MessageHandlerConfig
            {
                LineRegex = @".+", // Take any string so if we weren't filtering out PING, it would incorrectly trigger
                LineAction = this.MessageFunction,
                RespondToSelf = false
            };

            MessageHandler uut = new MessageHandler(
                config
            );

            string ircString = this.GenerateMessage( remoteUser, this.ircConfig.Channels[0], expectedMessage );
            uut.HandleEvent( this.ConstructArgs( ircString ) );

            Assert.IsNull( this.responseReceived );
        }

        // -------- Test Helpers --------

        private void DoGoodMessageTest( string user, string channel )
        {
            MessageHandlerConfig config = new MessageHandlerConfig
            {
                LineRegex = @"!bot\s+help",
                LineAction = this.MessageFunction
            };

            MessageHandler uut = new MessageHandler(
                config
            );

            const string expectedMessage = "!bot help";

            string ircString = this.GenerateMessage( user, channel, expectedMessage );
            uut.HandleEvent( this.ConstructArgs( ircString ) );

            Assert.AreEqual( channel, this.responseReceived.Channel );
            Assert.AreEqual( user, this.responseReceived.User );
            Assert.AreEqual( expectedMessage, this.responseReceived.Message );
        }

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
        /// <param name="args">The response from the server.</param>
        private void MessageFunction( MessageHandlerArgs args )
        {
            Assert.AreSame( this.ircWriter.Object, args.Writer );
            this.responseReceived = args;
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