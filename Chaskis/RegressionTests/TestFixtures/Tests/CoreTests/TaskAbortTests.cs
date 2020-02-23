//
//          Copyright Seth Hendrick 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Chaskis.RegressionTests.TestCore;
using NUnit.Framework;

namespace Chaskis.RegressionTests.Tests.CoreTests
{
    [TestFixture]
    public class TaskAbortTests
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
                Environment = null, // Use Default Environment.
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
        /// Ensures if a task is taking too long, we abort it.
        /// </summary>
        [Test]
        public void TaskAbortTest()
        {
            // By default, our task timeout is 15 seconds.  Make our timeout 20 seconds
            // so we can ensure there is no race conditions possible.

            const int timeout = 20 * 1000;

            Step.Run(
                "Sleep in parsing thread, ensure it gets aborted",
                () =>
                {
                    this.testFrame.IrcServer.SendMessageToChannelAs(
                        $"!{TestConstants.RegressionTestCommandPrefix} sleep {timeout}",
                        TestConstants.Channel1,
                        TestConstants.NormalUser
                    );

                    this.testFrame.IrcServer.WaitForMessageOnChannel(
                        @"Sleeping\s+for\s+\d+ms",
                        TestConstants.Channel1
                    ).FailIfFalse( "Did not get sleep message" );

                    this.testFrame.IrcServer.WaitForMessageOnChannel(
                        @"Caught\s+ThreadInterruptedException\s+during\s+sleep",
                        TestConstants.Channel1,
                        timeout
                    ).FailIfFalse( "Did not interrupted message" );
                }
            );

            Step.Run(
                "Are we still there?",
                () => this.testFrame.CanaryTest()
            );
        }
    }
}
