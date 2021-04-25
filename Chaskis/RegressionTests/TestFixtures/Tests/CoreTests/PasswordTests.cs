//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using Chaskis.RegressionTests.TestCore;
using NUnit.Framework;

namespace Chaskis.RegressionTests.Tests.CoreTests
{
    // Different environments for passwords, so need two fixtures.

    public class PasswordTests
    {
        [TestFixture]
        public class NoPasswordTests
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
                    ConnectionWaitMode = ConnectionWaitMode.DoNotWait
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
            public void WaitForNoPasswordMessages()
            {
                this.testFrame.ProcessRunner.WaitForStringFromChaskis(
                    @"No\s+Server\s+Password\s+Specified"
                ).FailIfFalse( "Did not get no server password debug message" );

                this.testFrame.ProcessRunner.WaitForStringFromChaskis(
                    @"No\s+NickServ\s+Password\s+Specified"
                ).FailIfFalse( "Did not get no NickServ password debug message" );

                this.testFrame.ProcessRunner.WaitToFinishJoiningChannels();

                this.testFrame.CanaryTest();
            }
        }

        [TestFixture]
        public class InlinePasswordTests
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
                    Environment = "InlinePasswordEnvironment",
                    ConnectionWaitMode = ConnectionWaitMode.DoNotWait
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
            public void WaitForPasswordMessages()
            {
                this.testFrame.IrcServer.WaitForString(
                    @"PASS\s+ServerPassword123"
                ).FailIfFalse( "Did send server password" );

                // For this environment, we also change up the NickServ
                // nick and the message to send.
                this.testFrame.IrcServer.WaitForMessageOnChannel(
                    @"HELLO\s+NickServPassword123",
                    "IdentityServ"
                ).FailIfFalse( "Did not send NickServ password to the correct user" );

                this.testFrame.ProcessRunner.WaitToFinishJoiningChannels();

                this.testFrame.CanaryTest();
            }
        }

        [TestFixture]
        public class EnvVarPasswordTests
        {
            // ---------------- Fields ----------------

            private const string serverEnvVar = "CHASKIS_SERVER_PASS";
            private const string nickServEnvVar = "CHASKIS_NICK_SERV_PASS";

            private const string expectedServerPass = "ServerPassword123";
            private const string expectedNickServPass = "NickServPass123";

            private string oldServerEnvVar;
            private string oldNickServEnvVar;
            private ChaskisTestFramework testFrame;

            // ---------------- Setup / Teardown ----------------

            [OneTimeSetUp]
            public void FixtureSetup()
            {
                this.oldServerEnvVar = Environment.GetEnvironmentVariable( serverEnvVar );
                this.oldNickServEnvVar = Environment.GetEnvironmentVariable( nickServEnvVar );

                Environment.SetEnvironmentVariable( serverEnvVar, expectedServerPass );
                Environment.SetEnvironmentVariable( nickServEnvVar, expectedNickServPass );

                this.testFrame = new ChaskisTestFramework();

                ChaskisFixtureConfig fixtureConfig = new ChaskisFixtureConfig
                {
                    Environment = "EnvVarPasswordEnvironment",
                    ConnectionWaitMode = ConnectionWaitMode.DoNotWait
                };

                this.testFrame.PerformFixtureSetup( fixtureConfig );
            }

            [OneTimeTearDown]
            public void FixtureTeardown()
            {
                try
                {
                    this.testFrame?.PerformFixtureTeardown();
                }
                finally
                {
                    Environment.SetEnvironmentVariable( serverEnvVar, this.oldServerEnvVar );
                    Environment.SetEnvironmentVariable( nickServEnvVar, this.oldNickServEnvVar );
                }
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
            public void WaitForPasswordMessages()
            {
                this.testFrame.IrcServer.WaitForString(
                    $@"PASS\s+{expectedServerPass}"
                ).FailIfFalse( "Did send server password" );

                // For this environment, we also change up the NickServ
                // nick and the message to send.
                this.testFrame.IrcServer.WaitForMessageOnChannel(
                    $@"{expectedNickServPass}\s+LOG\s+ME\s+IN",
                    "CustomService"
                ).FailIfFalse( "Did not send NickServ password to the correct user" );

                this.testFrame.ProcessRunner.WaitToFinishJoiningChannels();

                this.testFrame.CanaryTest();
            }
        }

        /// <summary>
        /// Tests where the password is in a file.
        /// </summary>
        [TestFixture]
        public class FilePasswordTests
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
                    Environment = "PasswordEnvironment",
                    ConnectionWaitMode = ConnectionWaitMode.DoNotWait
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
            public void WaitForPasswordMessages()
            {
                this.testFrame.IrcServer.WaitForString(
                    @"PASS\s+ServerPassword123"
                ).FailIfFalse( "Did send server password" );

                this.testFrame.IrcServer.WaitForMessageOnChannel(
                    @"IDENTIFY\s+NickServPassword123",
                    "NickServ"
                ).FailIfFalse( "Did not send NickServ password" );

                this.testFrame.ProcessRunner.WaitToFinishJoiningChannels();

                this.testFrame.CanaryTest();
            }
        }
    }
}
