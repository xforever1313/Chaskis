//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Text.RegularExpressions;
using Chaskis.RegressionTests.TestCore;
using NUnit.Framework;

namespace Chaskis.RegressionTests.Tests.PluginTests
{
    [TestFixture]
    public class CrossChannelTests
    {
        // ---------------- Fields ----------------

        private ChaskisTestFramework testFrame;

        // ---------------- Setup / Teardown ----------------

        [OneTimeSetUp]
        public void FixtureSetup()
        {
            this.testFrame = new ChaskisTestFramework();

            ChaskisFixtureConfig fixtureConfig = new ChaskisFixtureConfig
            {
                Environment = "CrossChannelEnvironment"
            };

            this.testFrame.PerformFixtureSetup( fixtureConfig );
        }

        [OneTimeTearDown]
        public void FixtureTeardown()
        {
            this.testFrame?.PerformFixtureTeardown();
        }

        [SetUp]
        public void TestSetup()
        {
            this.testFrame.PerformTestSetup();
        }

        [TearDown]
        public void TestTeardown()
        {
            this.testFrame.PerformTestTeardown();
        }

        // ---------------- Tests ----------------

        /// <summary>
        /// Ensures the plugin loads by itself without issue.
        /// </summary>
        [Test]
        public void DoPluginLoadTest()
        {
            CommonPluginTests.DoPluginLoadTest( this.testFrame, "crosschannel" );
        }

        /// <summary>
        /// Ensures the broadcast command works.
        /// </summary>
        [Test]
        public void DoBroadcastTest()
        {
            void DoTest( string expectedMessage, string channel )
            {
                const string user = TestConstants.NormalUser;

                this.testFrame.IrcServer.SendMessageToChannelAs(
                    $"!broadcast {expectedMessage}",
                    channel,
                    user
                );

                foreach( string chan in TestConstants.JoinedChannels )
                {
                    // If a PM, the channel to be printed out in the @ will be the username of the user
                    // who sent the broadcast, vs if done in a channel, which will include the channel name.
                    //
                    // PM to bot: <user@user>
                    // Channel: <user@channel>
                    string namedChannel = channel.Equals( TestConstants.BotName ) ? user : channel;
                    string expectedResponse = Regex.Escape( $"<{user}@{namedChannel}> {expectedMessage}" );
                    this.testFrame.IrcServer.WaitForMessageOnChannel(
                        expectedResponse,
                        chan
                    ).FailIfFalse( "Did not get response: " + expectedResponse );
                }
            }

            Step.Run(
                "Message with spaces",
                () =>
                {
                    DoTest( "Hello World!", TestConstants.Channel1 );
                }
            );

            Step.Run(
                "Message with no spaces",
                () =>
                {
                    DoTest( "Hello!", TestConstants.Channel1 );
                }
            );


            Step.Run(
                "Message with numbers",
                () =>
                {
                    DoTest( "1 + 1 = 2", TestConstants.Channel1 );
                }
            );

            Step.Run(
                "Broadcast from private message",
                () =>
                {
                    DoTest( "Hello World!", TestConstants.BotName );
                }
            );
        }

        /// <summary>
        /// Ensures the CC command works.
        /// </summary>
        [Test]
        public void DoCrossChannelTest()
        {
            void DoTest( string expectedMessage, string sourceChannel, string destinationChannel )
            {
                const string user = TestConstants.NormalUser;

                this.testFrame.IrcServer.SendMessageToChannelAs(
                    $"!cc <{destinationChannel}> {expectedMessage}",
                    sourceChannel,
                    user
                );

                // If a PM, the channel to be printed out in the @ will be the username of the user
                // who sent the broadcast, vs if done in a channel, which will include the channel name.
                //
                // PM to bot: <user@user>
                // Channel: <user@channel>
                string namedChannel = sourceChannel.Equals( TestConstants.BotName ) ? user : sourceChannel;
                string expectedResponse = Regex.Escape( $"<{user}@{namedChannel}> {expectedMessage}" );
                this.testFrame.IrcServer.WaitForMessageOnChannel(
                    expectedResponse,
                    destinationChannel
                ).FailIfFalse( "Did not get response: " + expectedResponse );
            }

            Step.Run(
                "Message with spaces",
                () =>
                {
                    DoTest( "Hello World!", TestConstants.Channel1, TestConstants.Channel2 );
                }
            );

            Step.Run(
                "Message with no spaces",
                () =>
                {
                    DoTest( "Hello!", TestConstants.Channel1, TestConstants.Channel2 );
                }
            );


            Step.Run(
                "Message with numbers",
                () =>
                {
                    DoTest( "1 + 1 = 2", TestConstants.Channel1, TestConstants.Channel2 );
                }
            );

            Step.Run(
                "From private message",
                () =>
                {
                    DoTest( "Hello World!", TestConstants.BotName, TestConstants.Channel2 );
                }
            );
        }
    }
}
