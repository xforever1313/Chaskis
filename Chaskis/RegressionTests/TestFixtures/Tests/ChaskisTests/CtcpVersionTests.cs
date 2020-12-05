//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Text;
using Chaskis.RegressionTests.TestCore;
using NUnit.Framework;

namespace Chaskis.RegressionTests.Tests.ChaskisTests
{
    [TestFixture]
    public class CtcpVersionTests
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
        /// Sends a CTCP Version and ensures our bot behaves as expected.
        /// </summary>
        [Test]
        public void CtcpVersionNoArgsTest()
        {
            const string userMessage = "VERSION"; // <- There are hidden characters in this string.
            string expectedBotResponse = $"VERSION Chaskis IRC Bot v{ChaskisConstants.ExpectedChaskisExeVersion} :\\s+.+";

            // This is done where a user sends a PM to the bot directly,
            // and the bot will respond with a privmsg back to the user in the form of a PM.
            this.testFrame.IrcServer.SendMessageToChannelAs(
                userMessage,
                TestConstants.BotName,
                TestConstants.NormalUser
            );

            this.testFrame.IrcServer.WaitForNoticeOnChannel(
                expectedBotResponse,
                TestConstants.NormalUser
            ).FailIfFalse( "Did not get VERSION response" );
        }

        /// <summary>
        /// Sends a CTCP Version and ensures our bot behaves as expected.
        /// 
        /// This one sends args, but the behavior from our bot won't change.
        /// </summary>
        [Test]
        public void CtcpVersionWithArgsTest()
        {
            const string userMessage = "VERSION args"; // <- There are hidden characters in this string.
            string expectedBotResponse = $"VERSION Chaskis IRC Bot v{ChaskisConstants.ExpectedChaskisExeVersion} :\\s+.+";

            // This is done where a user sends a PM to the bot directly,
            // and the bot will respond with a privmsg back to the user in the form of a PM.
            this.testFrame.IrcServer.SendMessageToChannelAs(
                userMessage,
                TestConstants.BotName,
                TestConstants.NormalUser
            );

            this.testFrame.IrcServer.WaitForNoticeOnChannel(
                expectedBotResponse,
                TestConstants.NormalUser
            ).FailIfFalse( "Did not get VERSION response" );
        }
    }
}
