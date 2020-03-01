//
//          Copyright Seth Hendrick 2020.
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
    public class PluginListCmdTests
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
        public void PluginsCmdTest()
        {
            DoPluginListTest(
                $"!{TestConstants.BotName} plugins"
            );
        }

        [Test]
        public void PluginListTest()
        {
            DoPluginListTest(
                $"@{TestConstants.BotName}: pluginlist"
            );
        }

        [Test]
        public void PluginListWithSpaceTest()
        {
            DoPluginListTest(
                $"!{TestConstants.BotName}: plugin list"
            );
        }

        // ---------------- Test Helpers ----------------

        private void DoPluginListTest( string command )
        {
            // For this test, we'll only look for the regression test plugin and the chaskis (default) plugin.
            // We don't care about the others
            // (if we list those 2, we are good enough.
            // Plus, we don't want to modify this file whenever the test environment changes).
            // The default plugin is always added last, should it should show up last.

            this.testFrame.IrcServer.SendMessageToChannelAsWaitMsg(
                command,
                TestConstants.Channel1,
                TestConstants.NormalUser,
                @"List\s+of\s+plugins\s+I\s+am\s+running:.+" + ChaskisConstants.ExpectedRegressionTestPluginName + ".+" + ChaskisConstants.ExpectedDefaultPluginName
            ).FailIfFalse( "Did not get plugin list response" );
        }
    }
}
