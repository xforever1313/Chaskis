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
    public class WelcomeBotTests
    {
        // ---------------- Fields ----------------

        const string user = TestConstants.NormalUser;
        const string adminUser = TestConstants.AdminUserName;
        const string channel = TestConstants.Channel1;

        private ChaskisTestFramework testFrame;

        // ---------------- Setup / Teardown ----------------

        [OneTimeSetUp]
        public void FixtureSetup()
        {
            this.testFrame = new ChaskisTestFramework();

            ChaskisFixtureConfig fixtureConfig = new ChaskisFixtureConfig
            {
                Environment = "WelcomeBotEnvironment"
            };

            this.testFrame.PerformFixtureSetup( fixtureConfig );
        }

        [OneTimeTearDown]
        public void FixtureTeardown()
        {
            this.testFrame?.PerformFixtureTeardown();
        }

        public void TestSetup()
        {
            this.testFrame.PerformTestSetup();
        }

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
            CommonPluginTests.DoPluginLoadTest( this.testFrame, "welcomebot" );
        }

        /// <summary>
        /// Ensures if a user joins, the bot welcomes them.
        /// </summary>
        [Test]
        public void JoinTest()
        {
            this.testFrame.IrcServer.SendJoined( user, channel );
            this.testFrame.IrcServer.WaitForMessageOnChannel(
                user + @"\s+has\s+joined\s+" + channel,
                channel
            ).FailIfFalse( "Did not get joined message." );
        }

        /// <summary>
        /// Ensures if a user leaves, the bot says they left.
        /// </summary>
        [Test]
        public void PartTest()
        {
            Step.Run(
                "Part with no reason",
                () =>
                {
                    this.testFrame.IrcServer.SendPartedFrom( user, channel );
                    this.testFrame.IrcServer.WaitForMessageOnChannel(
                        user + @"\s+has\s+left\s+" + channel,
                        channel
                    ).FailIfFalse( "Did not get parted message." );
                }
            );

            // Parting with reason has not been implemented yet.
        }

        /// <summary>
        /// Ensures if a user is kicked, the bot says they were kicked.
        /// </summary>
        [Test]
        public void KickTest()
        {
            Step.Run(
                "Kick with no reason",
                () =>
                {
                    this.testFrame.IrcServer.SendKickedFromBy( user, channel, adminUser );
                    this.testFrame.IrcServer.WaitForMessageOnChannel(
                        user + @"\s+has\s+been\s+kicked\s+from\s+" + channel + @"\s+by\s+" + adminUser,
                        channel
                    ).FailIfFalse( "Did not get kicked message." );
                }
            );

            Step.Run(
                "Kick with reason",
                () =>
                {
                    const string reason = "some reason";

                    this.testFrame.IrcServer.SendKickedFromByWithReason( user, channel, adminUser, reason );
                    this.testFrame.IrcServer.WaitForMessageOnChannel(
                        user + @"\s+has\s+been\s+kicked\s+from\s+" + channel + @"\s+by\s+" + adminUser + @"\s+for\s+reason\s+'" + reason + "'",
                        channel
                    ).FailIfFalse( "Did not get kicked message." );
                }
            );
        }
    }
}
