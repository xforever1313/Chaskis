//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Chaskis.RegressionTests.TestCore;
using NUnit.Framework;

namespace Chaskis.RegressionTests.Tests.CoreTests
{
    [TestFixture]
    public class EventHandlerExceptionTests
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

        [Test]
        public void DoEventHandlerExceptionTest()
        {
            const string message = "My Exception";

            Step.Run(
                "Throwing exception on string parsing queue",
                () =>
                {
                    string command = $"!{TestConstants.RegressionTestCommandPrefix} throw {message}";
                    this.testFrame.IrcServer.SendMessageToChannelAs(
                        command,
                        TestConstants.Channel1,
                        TestConstants.NormalUser
                    );

                    // We should get the plugin name, the command from the server, and the message
                    // the exception contains.
                    this.testFrame.ProcessRunner.WaitForStringFromChaskis(
                        @$"{TestConstants.RegressionTestCommandPrefix}.+threw.+{command}.+{message}"
                    ).FailIfFalse( "Did not get our exception message" );
                }
            );

            Step.Run(
                "Are we still there?",
                () => this.testFrame.CanaryTest()
            );
        }
    }
}
