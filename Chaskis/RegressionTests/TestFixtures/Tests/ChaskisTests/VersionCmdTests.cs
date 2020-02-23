//
//          Copyright Seth Hendrick 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Reflection;
using System.Text.RegularExpressions;
using Chaskis.RegressionTests.TestCore;
using NUnit.Framework;

namespace Chaskis.RegressionTests.Tests.ChaskisTests
{
    [TestFixture]
    public class VersionCmdTests
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
        /// Ensures the version command works as expected when getting chaskis.exe's version.
        /// </summary>
        [Test]
        public void ChaskisVersionCmdTest()
        {
            Step.Run(
                "Ensure we can query the CLI version",
                () =>
                {
                    string expectedResponse = @"Version\s+of\s+'chaskis':\s+" + Regex.Escape( this.testFrame.ProcessRunner.ChaskisExeVersionStr );

                    this.testFrame.IrcServer.SendMessageToChannelAsWaitMsg(
                        $"!{TestConstants.BotName} version",
                        TestConstants.Channel1,
                        TestConstants.NormalUser,
                        expectedResponse
                    ).FailIfFalse( "Did not get version response" );

                    this.testFrame.IrcServer.SendMessageToChannelAsWaitMsg(
                        $"@{TestConstants.BotName} version chaskis",
                        TestConstants.Channel1,
                        TestConstants.NormalUser,
                        expectedResponse
                    ).FailIfFalse( "Did not get version response" );
                }
            );
        }

        /// <summary>
        /// Ensures we can get a plugin's version.  In this case,
        /// the regression test pluin's.
        /// </summary>
        [Test]
        public void PluginVersionCmdTest()
        {
            Step.Run(
                "Ensure we can query a specific plugin's version",
                () =>
                {
                    string vers = AssemblyName.GetAssemblyName( this.testFrame.EnvironmentManager.RegressionTestDllPath ).Version.ToString( 3 );

                    string expectedResponse = @"Version\s+of\s+'" + TestConstants.RegressionTestPluginName + @"':\s+" + Regex.Escape( vers );

                    this.testFrame.IrcServer.SendMessageToChannelAsWaitMsg(
                        $"!{TestConstants.BotName} version {TestConstants.RegressionTestPluginName}",
                        TestConstants.Channel1,
                        TestConstants.NormalUser,
                        expectedResponse
                    ).FailIfFalse( "Did not get version response" );
                }
            );
        }

        /// <summary>
        /// Ensures if a plugin we query does not exist, we don't crash.
        /// </summary>
        [Test]
        public void NonExistentPluginTest()
        {
            Step.Run(
                "Ensure if a plugin we query does not exist, we don't crash",
                () =>
                {
                    const string badPlugin = "herpderp";

                    string expectedResponse = $"'{badPlugin}'" + @"\s+is\s+not\s+a\s+plugin\s+I\s+have\s+loaded";

                    this.testFrame.IrcServer.SendMessageToChannelAsWaitMsg(
                        $"!{TestConstants.BotName} version {badPlugin}",
                        TestConstants.Channel1,
                        TestConstants.NormalUser,
                        expectedResponse
                    ).FailIfFalse( "Did not get error message" );
                }
            );
        }
    }
}
