//
//          Copyright Seth Hendrick 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;
using Chaskis.RegressionTests.TestCore;
using NUnit.Framework;

namespace Chaskis.RegressionTests.Tests.PluginTests
{
    [TestFixture]
    public class CapsWatcherTests
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
                Environment = "CapsWatcherEnvironment"
            };

            this.testFrame.PerformFixtureSetup( fixtureConfig );

            // Turn on full verbosity for caps watcher.
            this.testFrame.IrcServer.SendMessageToChannelAs(
                "!chaskisbot debug verbosity capswatcher 3",
                TestConstants.BotName,
                TestConstants.AdminUserName
            );
            this.testFrame.IrcServer.WaitForMessageOnChannel(
                @"'capswatcher'\s+log\s+verbosity\s+has\s+been\s+set\s+to\s+'3'",
                TestConstants.AdminUserName
            ).FailIfFalse( "Could not set verbosity" );
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
        /// Ensures the plugin loads by itself without issue.
        /// </summary>
        [Test]
        public void DoPluginLoadTest()
        {
            CommonPluginTests.DoPluginLoadTest( this.testFrame, "capswatcher" );
        }

        /// <summary>
        /// Ensures the bot is correctly triggered when it gets
        /// a caps message.
        /// </summary>
        [Test]
        public void ExpectCapsTest()
        {
            List<string> goodMessages = new List<string>
            {
                "THIS IS ME SHOUTING", // With spaces
                "LOL", // No Spaces

                // Ignore inside of a message.
                // Should still go out though, ignores must match exactly.
                "USA IS AWESOME!",
                "BILL NYE",
                "NY USA HELLO USA NY!"
            };

            ExpectCapsWithMessages( goodMessages );
        }

        /// <summary>
        /// Ensures the bot is NOT triggered when it gets
        /// a message with lower case letters in it.
        /// </summary>
        [Test]
        public void IgnoreLowercaseTest()
        {
            List<string> msgs = new List<string>
            {
                "Lowercase message",
                "Bill NYE"
            };

            ExpectBotToIgnoreMessages( msgs );
        }

        /// <summary>
        /// Ensures the bot is NOT triggered when it gets
        /// a message that is in the ignored list.
        /// </summary>
        [Test]
        public void IgnoreIgnoredTest()
        {
            List<string> msgs = new List<string>
            {
                "USA! USA!",
                "NY"
            };

            ExpectBotToIgnoreMessages( msgs );
        }

        /// <summary>
        /// Ensures the bot ignores one-letter messages.
        /// </summary>
        [Test]
        public void IgnoreOneLetterMessageTest()
        {
            List<string> msgs = new List<string>
            {
                "I",
                "f"
            };

            ExpectBotToIgnoreMessages( msgs );
        }

        /// <summary>
        /// Ensures the bot ignores emojis.
        /// </summary>
        /// <remarks>
        /// Test for https://github.com/xforever1313/Chaskis/issues/18
        /// </remarks>
        [Test]
        public void IgnoreEmojisTest()
        {
            List<string> msgs = new List<string>
            {
                @"¯\\_(ツ)_/¯",
                ":D :D :D",
                ":P :D C: D:"
            };

            ExpectBotToIgnoreMessages( msgs );
        }

        private void ExpectCapsWithMessages( IEnumerable<string> messages )
        {
            foreach( string msg in messages )
            {
                ExpectCapsWithMessage( msg );
            }
        }

        private void ExpectCapsWithMessage( string message )
        {
            this.testFrame.IrcServer.SendMessageToChannelAsWaitMsg(
                message,
                TestConstants.Channel1,
                TestConstants.NormalUser,
                "LOUD NOISES!"
            ).FailIfFalse( "Did not get Caps watcher response" );
        }

        private void ExpectBotToIgnoreMessages( IEnumerable<string> messages )
        {
            foreach( string msg in messages )
            {
                ExpectBotToIgnoreMessage( msg );
            }
        }

        private void ExpectBotToIgnoreMessage( string message )
        {
            this.testFrame.IrcServer.SendMessageToChannelAs(
                message,
                TestConstants.Channel1,
                TestConstants.NormalUser
            );

            this.testFrame.ProcessRunner.WaitForStringFromChaskis(
                @"Caps\s+check\s+failed\s+for\s+message"
            ).FailIfFalse( "Did not get ignore message from process with message: " + message );
        }
    }
}
