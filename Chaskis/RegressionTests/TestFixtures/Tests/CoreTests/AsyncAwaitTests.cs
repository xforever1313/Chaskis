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
    public class AsyncAwaitTests
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
        /// Ensures that if we do async/await, we get on a background thread.
        /// </summary>
        [Test]
        public void DoBackgroundThreadCheckTest()
        {
            Step.Run(
                "Start the test",
                () =>
                {
                    this.testFrame.IrcServer.SendMessageToChannelAs(
                        $"!{TestConstants.RegressionTestPluginName} asyncawait threadname",
                        TestConstants.Channel1,
                        TestConstants.NormalUser
                    );
                }
            );

            Step.Run(
                "Ensure before we call await, we are on the Parsing Queue Thread",
                () =>
                {
                    this.testFrame.IrcServer.WaitForMessageOnChannel(
                        @"Starting\s+from\s+StringParsingQueue\s+Thread",
                        TestConstants.Channel1
                    ).FailIfFalse( "Did not get parsing queue message." );
                }
            );

            Step.Run(
                "Ensure our background thread is NOT the String Parsing Queue Thread",
                () =>
                {
                    this.testFrame.IrcServer.WaitForMessageOnChannel(
                        // Called "Thread pool worker" on mono, empty if .NET Framework.
                        @"Background\s+Thread\s+Name:\s+(Thread\s+Pool\s+Worker)?END",
                        TestConstants.Channel1
                    ).FailIfFalse( "Did not get background thread name." );
                }
            );

            Step.Run(
                "Ensure when we return from the await, we are back on the String Parsing Queue Thread",
                () =>
                {
                    this.testFrame.IrcServer.WaitForMessageOnChannel(
                        @"Finishing\s+from\s+StringParsingQueue\s+Thread",
                        TestConstants.Channel1
                    ).FailIfFalse( "Not back on staring parsing thread." );
                }
            );
        }

        /// <summary>
        /// Ensures if we get an exception from a background thread,
        /// we do not crash.
        /// </summary>
        [Test]
        public void BackgroundThreadExceptionTest()
        {
            Step.Run(
                "Start the test",
                () =>
                {
                    this.testFrame.IrcServer.SendMessageToChannelAs(
                        $"!{TestConstants.RegressionTestPluginName} asyncawait exception",
                        TestConstants.Channel1,
                        TestConstants.NormalUser
                    );
                }
            );

            Step.Run(
                "Wait for error messages",
                () =>
                {
                    this.testFrame.IrcServer.WaitForMessageOnChannel(
                        @"About\s+to\s+throw\s+Exception",
                        TestConstants.Channel1
                    ).FailIfFalse( "Didn't get 'about to throw' message." );

                    this.testFrame.IrcServer.WaitForMessageOnChannel(
                        @"Caught\s+Exception\s+Throwing\s+Exception\s+From\s+Background\s+Thread",
                        TestConstants.Channel1
                    ).FailIfFalse( "Didn't catch exception." );
                }
            );

            Step.Run(
                "Are we still there?",
                () => this.testFrame.CanaryTest()
            );
        }
    }
}
