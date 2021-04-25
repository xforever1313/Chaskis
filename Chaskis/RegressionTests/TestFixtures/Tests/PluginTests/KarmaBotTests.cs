//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Chaskis.RegressionTests.TestCore;
using NUnit.Framework;

namespace Chaskis.RegressionTests.Tests.PluginTests
{
    [TestFixture]
    public class KarmaBotTests
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
                Environment = "KarmaBotEnvironment"
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
            CommonPluginTests.DoPluginLoadTest( this.testFrame, "karmabot" );
        }

        [Test]
        public void IncreaseDecreaseTest()
        {
            Step.Run(
                "Just make sure 'chaskis' and the number of karma shows up. Everything else in between we don't care about",
                () =>
                {
                    this.testFrame.IrcServer.SendMessageToChannelAsWaitMsg(
                        "chaskis++",
                        TestConstants.Channel1,
                        TestConstants.NormalUser,
                        @"chaskis\s+.*increased\s+.*1"
                    ).FailIfFalse( "Did not get expected response" );
                }
            );

            Step.Run(
                "Send to a different channel, the karma should be the same across all channels",
                () =>
                {
                    this.testFrame.IrcServer.SendMessageToChannelAsWaitMsg(
                        "++chaskis",
                        TestConstants.Channel2,
                        TestConstants.NormalUser,
                        @"chaskis\s+.*increased\s+.*2"
                    ).FailIfFalse( "Did not get expected response" );
                }
            );

            Step.Run(
                "Subtract Karma",
                () =>
                {
                    this.testFrame.IrcServer.SendMessageToChannelAsWaitMsg(
                        "chaskis--",
                        TestConstants.Channel2,
                        TestConstants.NormalUser,
                        @"chaskis\s+.*decreased\s+.*1"
                    ).FailIfFalse( "Did not get expected response" );

                    this.testFrame.IrcServer.SendMessageToChannelAsWaitMsg(
                        "--chaskis",
                        TestConstants.Channel1,
                        TestConstants.NormalUser,
                        @"chaskis\s+.*decreased\s+.*0"
                    ).FailIfFalse( "Did not get expected response" );
                }
            );

            Step.Run(
                "Decrease one more time to ensure we can go negative just fine",
                () =>
                {
                    this.testFrame.IrcServer.SendMessageToChannelAsWaitMsg(
                        "--chaskis",
                        TestConstants.Channel1,
                        TestConstants.NormalUser,
                        @"chaskis\s+.*decreased\s+.*-1"
                    ).FailIfFalse( "Did not get expected response" );
                }
            );

            Step.Run(
                "Decrease something new right out of the gate, should be -1",
                () =>
                {
                    this.testFrame.IrcServer.SendMessageToChannelAsWaitMsg(
                        "--java",
                        TestConstants.Channel2,
                        TestConstants.NormalUser,
                        @"java\s+.*decreased\s+.*-1"
                    ).FailIfFalse( "Did not get expected response" );
                }
            );

            Step.Run(
                "Increase with a reason",
                () =>
                {
                    this.testFrame.IrcServer.SendMessageToChannelAsWaitMsg(
                        "++chaskis for being cool",
                        TestConstants.Channel2,
                        TestConstants.NormalUser,
                        @"chaskis\s+.*increased\s+.*0"
                    ).FailIfFalse( "Did not get expected response" );
                }
            );

            Step.Run(
                "Decrease with a reason",
                () =>
                {
                    this.testFrame.IrcServer.SendMessageToChannelAsWaitMsg(
                        "java-- C# is a better language",
                        TestConstants.Channel1,
                        TestConstants.NormalUser,
                        @"java\s+.*decreased\s+.*-2"
                    ).FailIfFalse( "Did not get expected response" );
                }
            );

            Step.Run(
                "Query",
                () =>
                {
                    this.testFrame.IrcServer.SendMessageToChannelAsWaitMsg(
                        "!karma chaskis",
                        TestConstants.Channel1,
                        TestConstants.NormalUser,
                        @"chaskis\s+has\s+0\s+karma"
                    ).FailIfFalse( "Did not get expected response" );

                    this.testFrame.IrcServer.SendMessageToChannelAsWaitMsg(
                        "!karma java",
                        TestConstants.Channel1,
                        TestConstants.NormalUser,
                        @"java\s+has\s+-2\s+karma"
                    ).FailIfFalse( "Did not get expected response" );
                }
            );

            Step.Run(
                "Query something that doesn't exist, should report 0",
                () =>
                {
                    this.testFrame.IrcServer.SendMessageToChannelAsWaitMsg(
                        "!karma something_not_here",
                        TestConstants.Channel1,
                        TestConstants.NormalUser,
                        @"something_not_here\s+has\s+0\s+karma"
                    ).FailIfFalse( "Did not get expected response" );
                }
            );
        }
    }
}
