//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Chaskis.RegressionTests.TestCore;
using NUnit.Framework;

namespace Chaskis.RegressionTests.Tests.ChaskisTests
{
    /// <summary>
    /// Tests the default plugin built into Chaskis.exe.
    /// </summary>
    [TestFixture]
    public class CtcpPingTests
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
        /// Sends a CTCP Ping and ensures our bot behaves as expected.
        /// </summary>
        [Test]
        public void CtcpPingTest()
        {
            const string pingMsg = "PING 123456789ABCD"; // <- There are hidden characters in this string.

            // This is done where a user sends a PM to the bot directly,
            // and the bot will respond with a notice back to the user in the form of a PM.
            this.testFrame.IrcServer.SendMessageToChannelAs(
                pingMsg, 
                TestConstants.BotName,
                TestConstants.NormalUser
            );

            this.testFrame.IrcServer.WaitForNoticeOnChannel(
                pingMsg,
                TestConstants.NormalUser
            ).FailIfFalse( "Did not get notice message" );
        }
    }
}
