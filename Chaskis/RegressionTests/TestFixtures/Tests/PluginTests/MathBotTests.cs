//
//          Copyright Seth Hendrick 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Text.RegularExpressions;
using Chaskis.RegressionTests.TestCore;
using NUnit.Framework;

namespace Chaskis.RegressionTests.Tests.PluginTests
{
    [TestFixture]
    public class MathBotTests
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
                Environment = "MathBotEnvironment"
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
            CommonPluginTests.DoPluginLoadTest( this.testFrame, "mathbot" );
        }

        /// <summary>
        /// Does tests with standard math.
        /// </summary>
        [Test]
        public void StandardMathTest()
        {
            DoMathTest( "1+1", "2" );
            DoMathTest( "1 + 1", "2" );
            DoMathTest( "1*1", "1" );
            DoMathTest( "10  * 1", "10" );
            DoMathTest( "5 / 1", "5" );
        }

        /// <summary>
        /// Does tests with boolean logic.
        /// </summary>
        [Test]
        public void BooleanLogicTest()
        {
            DoMathTest( "true and true", "True" );
            DoMathTest( "true or false", "True" );
            DoMathTest( "TRUE AND FALSE", "False" );
        }

        // ---------------- Test Helpers ----------------

        private void DoMathTest( string equation, string expectedAnswer )
        {
            this.testFrame.IrcServer.SendMessageToChannelAsWaitMsg(
                $"!calc {equation}",
                TestConstants.Channel1,
                TestConstants.NormalUser,
                // Expect: '1 + 1' Equals '2' (We don't care about the Equals in this test in case we change the output).
                "'" + Regex.Escape( equation ) + @"'.+'" + Regex.Escape( expectedAnswer ) + "'"
            ).FailIfFalse( "Equation did not get the correct answer" );
        }
    }
}
