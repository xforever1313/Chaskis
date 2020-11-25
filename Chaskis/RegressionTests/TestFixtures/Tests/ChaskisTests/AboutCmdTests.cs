//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Text.RegularExpressions;
using Chaskis.RegressionTests.TestCore;
using NUnit.Framework;

namespace Chaskis.RegressionTests.Tests.ChaskisTests
{
    [TestFixture]
    public class AboutCmdTests
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
        /// Ensure if we query without a plugin specified, we get the main Chaskis source.
        /// </summary>
        [Test]
        public void MainAboutTest()
        {
            this.testFrame.IrcServer.SendMessageToChannelAsWaitMsg(
                $"!{TestConstants.BotName} about",
                TestConstants.Channel1,
                TestConstants.NormalUser,
                // We don't care what the about message is for the default plugin,
                // as long as it starts with "About", and has the license
                // information in it.
                @"About\s+'" + ChaskisConstants.ExpectedDefaultPluginName + @"':.+" + Regex.Escape( ChaskisConstants.ExpectedLicenseUrl )
            ).FailIfFalse( "Did not get about response" );
        }

        /// <summary>
        /// Ensure if we query the default plugin, we get the main Chaskis source.
        /// </summary>
        [Test]
        public void DefaultPluginAboutTest()
        {
            this.testFrame.IrcServer.SendMessageToChannelAsWaitMsg(
                $"@{TestConstants.BotName} about {ChaskisConstants.ExpectedDefaultPluginName}",
                TestConstants.Channel1,
                TestConstants.NormalUser,
                // We don't care what the about message is for the default plugin,
                // as long as it starts with "About", and has the license
                // information in it.
                @"About\s+'" + ChaskisConstants.ExpectedDefaultPluginName + @"':.+" + Regex.Escape( ChaskisConstants.ExpectedLicenseUrl )
            ).FailIfFalse( "Did not get about response" );
        }

        /// <summary>
        /// Ensure if we query a specific plugin, we get that plugin's source.
        /// </summary>
        [Test]
        public void QueryPluginAboutTest()
        {
            this.testFrame.IrcServer.SendMessageToChannelAsWaitMsg(
                $"!{TestConstants.BotName} about {ChaskisConstants.ExpectedRegressionTestPluginName}",
                TestConstants.Channel1,
                TestConstants.NormalUser,
                @"About\s+'" + ChaskisConstants.ExpectedRegressionTestPluginName + "':.+Regression Tests"
            ).FailIfFalse( "Did not get about response" );
        }

        /// <summary>
        /// Ensure if we query a non-existant plugin, we don't crash.
        /// </summary>
        [Test]
        public void DoNonExistantPluginTest()
        {
            this.testFrame.IrcServer.SendMessageToChannelAsWaitMsg(
                $"@{TestConstants.BotName}: about loldne",
                TestConstants.Channel1,
                TestConstants.NormalUser,
                @"'loldne'\s+is\s+not\s+a\s+plugin\s+I\s+have\s+loaded"
            ).FailIfFalse( "Did not get error response." );
        }
    }
}
