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
    public class SourceCmdTests
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
        public void MainSourceTest()
        {
            this.testFrame.IrcServer.SendMessageToChannelAsWaitMsg(
                $"!{TestConstants.BotName} source",
                TestConstants.Channel1,
                TestConstants.NormalUser,
                @"Source\s+of\s+'" + ChaskisConstants.ExpectedDefaultPluginName + @"':\s+" + Regex.Escape( ChaskisConstants.ExpectedProjectUrl )
            ).FailIfFalse( "Did not get source response" );
        }

        /// <summary>
        /// Ensure if we query the default plugin, we get the main Chaskis source.
        /// </summary>
        [Test]
        public void DefaultPluginSourceTest()
        {
            this.testFrame.IrcServer.SendMessageToChannelAsWaitMsg(
                $"!{TestConstants.BotName} source {ChaskisConstants.ExpectedDefaultPluginName}",
                TestConstants.Channel1,
                TestConstants.NormalUser,
                @"Source\s+of\s+'" + ChaskisConstants.ExpectedDefaultPluginName + @"':\s+" + Regex.Escape( ChaskisConstants.ExpectedProjectUrl )
            ).FailIfFalse( "Did not get source response" );
        }

        /// <summary>
        /// Ensure if we query a specific plugin, we get that plugin's source.
        /// </summary>
        [Test]
        public void QueryPluginSourceTest()
        {
            this.testFrame.IrcServer.SendMessageToChannelAsWaitMsg(
                $"!{TestConstants.BotName} source {ChaskisConstants.ExpectedRegressionTestPluginName}",
                TestConstants.Channel1,
                TestConstants.NormalUser,
                @"Source\s+of\s+'" + ChaskisConstants.ExpectedRegressionTestPluginName + @"':\s+" + Regex.Escape( ChaskisConstants.ExpectedProjectUrl ) + "tree/master/Chaskis/RegressionTests/TestFixtures"
            ).FailIfFalse( "Did not get source response" );
        }

        /// <summary>
        /// Ensure if we query a non-existant plugin, we don't crash.
        /// </summary>
        [Test]
        public void DoNonExistantPluginTest()
        {
            this.testFrame.IrcServer.SendMessageToChannelAsWaitMsg(
                $"!{TestConstants.BotName} source herpderp",
                TestConstants.Channel1,
                TestConstants.NormalUser,
                @"'herpderp'\s+is\s+not\s+a\s+plugin\s+I\s+have\s+loaded"
            ).FailIfFalse( "Did not get error response." );
        }
    }
}
