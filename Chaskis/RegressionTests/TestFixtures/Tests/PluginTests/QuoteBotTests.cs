//
//          Copyright Seth Hendrick 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Chaskis.RegressionTests.TestCore;
using NUnit.Framework;

namespace Chaskis.RegressionTests.Tests.PluginTests
{
    [TestFixture]
    public class QuoteBotTests
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
                Environment = "QuoteBotEnvironment"
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
            CommonPluginTests.DoPluginLoadTest( this.testFrame, "quotebot" );
        }

        /// <summary>
        /// Ensure if we query a negative number, we get an error message.
        /// </summary>
        [Test]
        public void NegativeQuoteTest()
        {
            this.testFrame.IrcServer.SendMessageToChannelAsWaitMsg(
                "!quote get -1",
                TestConstants.Channel1,
                TestConstants.NormalUser,
                @"Error\s+when\s+getting\s+quote:\s+"
            ).FailIfFalse( "Did not get expected response" );
        }

        [Test]
        public void AddRemoveTest()
        {
            Step.Run(
                "To start, ensure if we query a non-existant quote, an error message occurs",
                () =>
                {
                    this.testFrame.IrcServer.SendMessageToChannelAsWaitMsg(
                        "!quote random",
                        TestConstants.Channel1,
                        TestConstants.NormalUser,
                        @"Can\s+not\s+get\s+random\s+quote\.\s+Do\s+we\s+even\s+have\s+any\?"
                    ).FailIfFalse( "Did not get expected response" );

                    this.testFrame.IrcServer.SendMessageToChannelAsWaitMsg(
                        "!quote get 0",
                        TestConstants.Channel1,
                        TestConstants.NormalUser,
                        @"Can\s+not\s+get\s+quote\s+with\s+id\s+0\.\s+Are\s+you\s+sure\s+it\s+exists\?"
                    ).FailIfFalse( "Did not get expected response" );
                }
            );

            Step.Run(
                "Add a quote, ensure we can't delete it if we are normal user",
                () =>
                {
                    this.testFrame.IrcServer.SendMessageToChannelAsWaitMsg(
                        "!quote add <user> Hello World!",
                        TestConstants.Channel1,
                        TestConstants.NormalUser,
                        @"Quote\s+said\s+by\s+user\s+added\s+by\s+nonadminuser\.\s+Its\s+ID\s+is\s+\d+"
                    ).FailIfFalse( "Did not get expected response" );

                    this.testFrame.IrcServer.SendMessageToChannelAsWaitMsg(
                        "!quote delete 1",
                        TestConstants.Channel1,
                        TestConstants.NormalUser,
                        @".+You\s+can\s+not\s+delete\s+quotes"
                    ).FailIfFalse( "Did not get expected response" );
                }
            );

            Step.Run(
                "Attempt to get the added quote, both directly and randomly",
                () =>
                {
                    this.testFrame.IrcServer.SendMessageToChannelAsWaitMsg(
                        "!quote get 1",
                        TestConstants.Channel1,
                        TestConstants.NormalUser,
                        @"'Hello World!'\s+-user\.\s+Added\s+by\s+nonadminuser\s+on.+"
                    ).FailIfFalse( "Did not get expected response" );

                    this.testFrame.IrcServer.SendMessageToChannelAsWaitMsg(
                        "!quote random",
                        TestConstants.Channel1,
                        TestConstants.NormalUser,
                        @"'Hello World!'\s+-user\.\s+Added\s+by\s+nonadminuser\s+on.+"
                    ).FailIfFalse( "Did not get expected response" );
                }
            );

            Step.Run(
                "Delete the quote as an admin user",
                () =>
                {
                    this.testFrame.IrcServer.SendMessageToChannelAsWaitMsg(
                        "!quote delete 1",
                        TestConstants.Channel1,
                        TestConstants.AdminUserName,
                        @"Quote\s+1\s+deleted\s+successfully"
                    ).FailIfFalse( "Did not get expected response" );

                    this.testFrame.IrcServer.SendMessageToChannelAsWaitMsg(
                        "!quote get 1",
                        TestConstants.Channel1,
                        TestConstants.NormalUser,
                        @"Can\s+not\s+get\s+quote\s+with\s+id\s+1\.\s+Are\s+you\s+sure\s+it\s+exists\?"
                    ).FailIfFalse( "Did not get expected response" );
                }
            );
        }
    }
}
