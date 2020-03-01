//
//          Copyright Seth Hendrick 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.IO;
using System.Text.RegularExpressions;
using Chaskis.RegressionTests.TestCore;
using NUnit.Framework;

namespace Chaskis.RegressionTests.Tests.PluginTests
{
    // Need multiple test fixtures since the environment is for each fixture.

    internal interface INewVersionNotifierTestFixture
    {
        ChaskisTestFramework TestFrame { get; }
    }

    public class NewVersionNotifier
    { 
        /// <summary>
        /// Tests ensures that if we have a different version number than the previous release,
        /// do we send the correct message?
        /// </summary>
        [TestFixture]
        public class DifferentVersionTests : INewVersionNotifierTestFixture
        {
            // ---------------- Fields ----------------

            // ---------------- Setup / Teardown ----------------

            [OneTimeSetUp]
            public void FixtureSetup()
            {
                this.TestFrame = new ChaskisTestFramework();

                ChaskisFixtureConfig fixtureConfig = new ChaskisFixtureConfig
                {
                    Environment = "NewVersionNotifierDifferentVersionEnvironment",
                    ConnectionWaitMode = ConnectionWaitMode.WaitForConnected
                };

                this.TestFrame.PerformFixtureSetup( fixtureConfig );
            }

            [OneTimeTearDown]
            public void FixtureTeardown()
            {
                this.TestFrame?.PerformFixtureTeardown();
            }

            [SetUp]
            public void TestSetup()
            {
                this.TestFrame.PerformTestSetup();
            }

            [TearDown]
            public void TestTeardown()
            {
                this.TestFrame.PerformTestTeardown();
            }

            // ---------------- Properties ----------------

            public ChaskisTestFramework TestFrame { get; private set; }

            // ---------------- Tests ----------------

            [Test]
            public void NewVersionMessageTest()
            {
                this.DoNewVersionTest();
            }
        }

        /// <summary>
        /// Tests ensures that if we no version file, we get a message.
        /// </summary>
        [TestFixture]
        public class NoVersionFileTests : INewVersionNotifierTestFixture
        {
            // ---------------- Fields ----------------

            // ---------------- Setup / Teardown ----------------

            [OneTimeSetUp]
            public void FixtureSetup()
            {
                this.TestFrame = new ChaskisTestFramework();

                ChaskisFixtureConfig fixtureConfig = new ChaskisFixtureConfig
                {
                    Environment = "NewVersionNotifierNoFileEnvironment",
                    ConnectionWaitMode = ConnectionWaitMode.WaitForConnected
                };

                this.TestFrame.PerformFixtureSetup( fixtureConfig );
            }

            [OneTimeTearDown]
            public void FixtureTeardown()
            {
                this.TestFrame?.PerformFixtureTeardown();
            }

            [SetUp]
            public void TestSetup()
            {
                this.TestFrame.PerformTestSetup();
            }

            [TearDown]
            public void TestTeardown()
            {
                this.TestFrame.PerformTestTeardown();
            }

            // ---------------- Properties ----------------

            public ChaskisTestFramework TestFrame { get; private set; }

            // ---------------- Tests ----------------

            [Test]
            public void NewVersionMessageTest()
            {
                this.DoNewVersionTest();
            }
        }

        /// <summary>
        /// Tests ensures that if the version does NOT change, no message goes out.
        /// </summary>
        [TestFixture]
        public class NoVersionChangeTests : INewVersionNotifierTestFixture
        {
            // ---------------- Fields ----------------

            // ---------------- Setup / Teardown ----------------

            [OneTimeSetUp]
            public void FixtureSetup()
            {
                this.TestFrame = new ChaskisTestFramework();

                ChaskisFixtureConfig fixtureConfig = new ChaskisFixtureConfig
                {
                    Environment = "NewVersionNotifierNoChangeEnvironment",
                    ConnectionWaitMode = ConnectionWaitMode.WaitForConnected
                };

                this.TestFrame.PerformFixtureSetup( fixtureConfig );
            }

            [OneTimeTearDown]
            public void FixtureTeardown()
            {
                this.TestFrame?.PerformFixtureTeardown();
            }

            [SetUp]
            public void TestSetup()
            {
                this.TestFrame.PerformTestSetup();
            }

            [TearDown]
            public void TestTeardown()
            {
                this.TestFrame.PerformTestTeardown();
            }

            // ---------------- Properties ----------------

            public ChaskisTestFramework TestFrame { get; private set; }

            // ---------------- Tests ----------------

            /// <summary>
            /// Ensures if there is no change, we do not send a message.
            /// </summary>
            [Test]
            public void DoNotSendMessageTest()
            {
                this.TestFrame.ProcessRunner.WaitForStringFromChaskis(
                    @"Bot\s+not\s+updated,\s+skipping\s+message",
                    20 * 1000
                ).FailIfFalse( "Did not get ignore message" );

                this.CheckFileVersion( ChaskisConstants.ExpectedChaskisExeVersion );
            }

            [Test]
            public void PluginLoadTest()
            {
                CommonPluginTests.DoPluginLoadTest( this.TestFrame, "new_version_notifier" );
            }
        }
    }

    internal static class NewVersionNotifierHelpers
    {
        public static void DoNewVersionTest( this INewVersionNotifierTestFixture fixture )
        {
            string versString = ChaskisConstants.ExpectedChaskisExeVersion;

            string expectedStr = @"I\s+have\s+been\s+upgraded\s+to\s+version\s+" + Regex.Escape( versString );

            Step.Run(
                "Wait for bot to relay message",
                () =>
                {
                    fixture.TestFrame.IrcServer.WaitForMessageOnChannel(
                        expectedStr,
                        TestConstants.Channel1,
                        15 * 1000 // Wait 15 seconds, since we need to finish joining and what-not.
                    ).FailIfFalse( "Did not get new version notification" );

                    fixture.TestFrame.IrcServer.WaitForMessageOnChannel(
                        expectedStr,
                        TestConstants.Channel2
                    ).FailIfFalse( "Did not get new version notification" );
                }
            );
            // Read the file.  Ensure the file is saved in there.

            Step.Run(
                "Ensure file got updated",
                () =>
                {
                        // Wait for the notification, and then read the file and ensure the version got saved correctly.
                        fixture.TestFrame.ProcessRunner.WaitForStringFromChaskis(
                                @"new_version_notifier's\s+\.lastversion\.txt\s+file\s+has\s+been\s+updated"
                            ).FailIfFalse( "Did not get notification that the file got updated" );

                    fixture.CheckFileVersion( versString );
                }
            );
        }

        public static void CheckFileVersion( this INewVersionNotifierTestFixture fixture, string expectedVersion )
        {
            string filePath = Path.Combine(
                fixture.TestFrame.EnvironmentManager.TestEnvironmentDir,
                "Plugins",
                "NewVersionNotifier",
                ".lastversion.txt"
            );

            CommonActions.FileContains(
                filePath,
                Regex.Escape( expectedVersion )
            );
        }
    }
}
