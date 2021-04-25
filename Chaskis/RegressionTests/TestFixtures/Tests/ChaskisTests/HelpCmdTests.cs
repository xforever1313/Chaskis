//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;
using Chaskis.RegressionTests.TestCore;
using NUnit.Framework;

namespace Chaskis.RegressionTests.Tests.ChaskisTests
{
    [TestFixture]
    public class HelpCmdTests
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
        /// Ensures if someone sends help with no args, we get the top-level message.
        /// </summary>
        [Test]
        public void HelpWithNoArgsTest()
        {
            List<string> cmds = new List<string>
            {
                $"!{TestConstants.BotName} help",
                $"@{TestConstants.BotName} help {ChaskisConstants.ExpectedDefaultPluginName}"
            };

            // Best we can do here without making the tests fragile is to just look for "Default Commands:".
            DoHelpTest(
                cmds,
                @"Default\s+Commands:"
            );
        }

        /// <summary>
        /// Ensures if someone sends help plugins with no args, we get the correct message.
        /// </summary>
        [Test]
        public void PluginHelpTest()
        {
            List<string> cmds = new List<string>
            {
                $"!{TestConstants.BotName} help plugins",
                $"@{TestConstants.BotName} help {ChaskisConstants.ExpectedDefaultPluginName} plugins",
                $"!{TestConstants.BotName} help pluginlist",
                $"!{TestConstants.BotName}: help {ChaskisConstants.ExpectedDefaultPluginName} pluginlist",
            };

            DoHelpTest(
                cmds,
                @"Gets\s+the\s+list\s+of\s+plugins\s+running"
            );
        }

        /// <summary>
        /// Ensures we get the correct response if we query the source command.
        /// </summary>
        [Test]
        public void SourceHelpTest()
        {
            List<string> cmds = new List<string>
            {
                $"!{TestConstants.BotName} help source",
                $"@{TestConstants.BotName} help {ChaskisConstants.ExpectedDefaultPluginName} source"
            };

            DoHelpTest(
                cmds,
                @"Gets\s+the\s+source\s+code\s+URL\s+of\s+the\s+given\s+plugin"
            );
        }

        /// <summary>
        /// Ensures we get the correct response if we query the version command.
        /// </summary>
        [Test]
        public void VersionHelpTest()
        {
            List<string> cmds = new List<string>
            {
                $"!{TestConstants.BotName} help version",
                $"@{TestConstants.BotName} help {ChaskisConstants.ExpectedDefaultPluginName} version"
            };

            DoHelpTest(
                cmds,
                @"Gets\s+the\s+version\s+of\s+the\s+given\s+plugin"
            );
        }

        /// <summary>
        /// Ensures we get the correct response if we query the about command.
        /// </summary>
        [Test]
        public void AboutHelpTest()
        {
            List<string> cmds = new List<string>
            {
                $"!{TestConstants.BotName} help about",
                $"@{TestConstants.BotName} help {ChaskisConstants.ExpectedDefaultPluginName} about"
            };

            DoHelpTest(
                cmds,
                @"Gets\s+information\s+about\s+the\s+given\s+plugin"
            );
        }

        /// <summary>
        /// Ensures we get the correct response if we query the help command.
        /// </summary>
        [Test]
        public void HelpHelpTest()
        {
            List<string> cmds = new List<string>
            {
                $"!{TestConstants.BotName} help help",
                $"@{TestConstants.BotName} help {ChaskisConstants.ExpectedDefaultPluginName} help"
            };

            DoHelpTest(
                cmds,
                @"Gets\s+help\s+information\s+about\s+the\s+given\s+plugin"
            );
        }

        /// <summary>
        /// Ensures we get the correct response if we query the admins command.
        /// </summary>
        [Test]
        public void AdminsHelpTest()
        {
            List<string> cmds = new List<string>
            {
                $"!{TestConstants.BotName} help admins",
                $"@{TestConstants.BotName} help {ChaskisConstants.ExpectedDefaultPluginName} admins"
            };

            DoHelpTest(
                cmds,
                @"Shows\s+the\s+list\s+of\s+people\s+who\s+are\s+considered\s+admins"
            );
        }

        /// <summary>
        /// Ensures if we query a specific plugins help command, we get the correct response.
        /// </summary>
        [Test]
        public void SpecificPluginHelpTest()
        {
            {
                List<string> cmds = new List<string>
                {
                    $"!{TestConstants.BotName} help {ChaskisConstants.ExpectedRegressionTestPluginName}"
                };

                DoHelpTest(
                    cmds,
                    @"Got\s+help\s+request\s+with\s+no\s+arguments"
                );
            }

            {
                List<string> cmds = new List<string>
                {
                    $"!{TestConstants.BotName} help {ChaskisConstants.ExpectedRegressionTestPluginName} arg1 arg2 arg3"
                };

                DoHelpTest(
                    cmds,
                    @"Got\s+help\s+request\s+with\s+these\s+args:\s+arg1\s+arg2\s+arg3"
                );
            }
        }

        /// <summary>
        /// Ensures querying a non-existant plugin does not result in a crash.
        /// </summary>
        [Test]
        public void NonExistantPluginTest()
        {
            List<string> cmds = new List<string>
            {
                $"!{TestConstants.BotName} help nope"
            };

            DoHelpTest(
                cmds,
                @"Invalid\s+Command!"
            );
        }

        // ---------------- Test Helpers ----------------

        private void DoHelpTest( IEnumerable<string> cmds, string expectedResponse )
        {
            foreach( string cmd in cmds )
            {
                Step.Run(
                    "Trying command: " + cmd,
                    () =>
                    {
                        this.testFrame.IrcServer.SendMessageToChannelAsWaitMsg(
                            cmd,
                            TestConstants.Channel1,
                            TestConstants.NormalUser,
                            expectedResponse
                        ).FailIfFalse( "Did not get help response ");
                    }
                );
            }
        }
    }
}
