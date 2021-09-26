//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Chaskis.RegressionTests.TestCore;
using NUnit.Framework;

namespace Chaskis.RegressionTests.Tests.CoreTests
{
    [TestFixture]
    public sealed class BridgeBotTests
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
                Environment = "BridgeBotEnvironment"
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
        /// Tests a bug where if a STX character is in the bridge user,
        /// bridge bots don't work.
        /// 
        /// This was found in the #rit-foss channel.
        /// </summary>
        [Test]
        public void BridgeBotWithStxResponse()
        {
            Step.Run(
                "With stx character",
                () =>
                {
                    this.testFrame.IrcServer.SendMessageToChannelAsWaitMsg(
                        $"<xforever1313> Hello {TestConstants.BotName}",
                        TestConstants.Channel1,
                        "tg-ritfoss1",
                        "Hello xforever1313"
                    ).FailIfFalse( "Did not get a response." );
                }
            );

            Step.Run(
                "Without stx character",
                () =>
                {
                    this.testFrame.IrcServer.SendMessageToChannelAsWaitMsg(
                        $"<xforever1313> Hello {TestConstants.BotName}",
                        TestConstants.Channel1,
                        "tg-ritfoss1",
                        "Hello xforever1313"
                    ).FailIfFalse( "Did not get a response." );
                }
            );
        }
    }
}
