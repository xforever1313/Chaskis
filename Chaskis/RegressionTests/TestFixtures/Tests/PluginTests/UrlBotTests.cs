//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Chaskis.RegressionTests.TestCore;
using NUnit.Framework;

namespace Chaskis.RegressionTests.Tests.PluginTests
{
    [TestFixture]
    public class UrlBotTests
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
                Environment = "UrlBotEnvironment"
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
        /// Ensures the plugin loads by itself without issue.
        /// </summary>
        [Test]
        public void DoPluginLoadTest()
        {
            CommonPluginTests.DoPluginLoadTest( this.testFrame, "urlbot" );
        }

        /// <summary>
        /// Gets the title of a file with nothing odd in it.
        /// </summary>
        [Test]
        public void StandardTest()
        {
            DoGetTitleTest(
                "Check this out: https://files.shendrick.net/projects/chaskis/testfiles/urlbot/goodsize.html",
                @"Title:\s+My Title"
            );
        }

        /// <summary>
        /// Gets the title, but with strange characters in the message.
        /// </summary>
        [Test]
        public void StrangeCharactersTest()
        {
            DoGetTitleTest(
                "▄▄▄▄▄▄▄▄▄▄▄ Check this out ===> https://files.shendrick.net/projects/chaskis/testfiles/urlbot/goodsize.html ▄▄▄▄▄▄▄▄▄▄▄",
                @"Title:\s+My Title"
            );
        }

        /// <summary>
        /// Ensures we escape any characters correctly.
        /// This tests issue: https://github.com/xforever1313/Chaskis/issues/15
        /// </summary>
        [Test]
        public void EscapedCharactersTest()
        {
            DoGetTitleTest(
                "https://files.shendrick.net/projects/chaskis/testfiles/urlbot/escapedcharacters.html",
                @"Title:\s+<My ""Title"">"
            );
        }

        /// <summary>
        /// Ensures if a file is too big, we ignore it.
        /// </summary>
        [Test]
        public void FileTooBigTest()
        {
            this.testFrame.IrcServer.SendMessageToChannelAs(
                "https://files.shendrick.net/projects/chaskis/testfiles/urlbot/bigsize.html",
                TestConstants.Channel1,
                TestConstants.NormalUser
            );

            this.testFrame.ProcessRunner.WaitForStringFromChaskis(
                @"Ignoring\s+URL.+whose\s+file\s+size\s+is\s+\d+"
            ).FailIfFalse( "Did not get debug message from Chaskis Process" );
        }

        // ---------------- Test Helpers ----------------

        private void DoGetTitleTest( string message, string expectedTitle )
        {
            this.testFrame.IrcServer.SendMessageToChannelAsWaitMsg(
                message,
                TestConstants.Channel1,
                TestConstants.NormalUser,
                expectedTitle
            ).FailIfFalse( "Did not get title from URL" );
        }
    }
}
