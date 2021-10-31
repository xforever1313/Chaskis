//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Threading;
using Chaskis.RegressionTests.TestCore;
using NUnit.Framework;
using SethCS.Extensions;

namespace Chaskis.RegressionTests.Tests.CoreTests
{
    [TestFixture]
    public sealed class EventHandlerExceptionTests
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
        public void DoEventHandlerExceptionTest()
        {
            const string message = "My Exception";

            Step.Run(
                "Throwing exception on string parsing queue",
                () =>
                {
                    string command = $"!{TestConstants.RegressionTestCommandPrefix} throw {message}";
                    this.testFrame.IrcServer.SendMessageToChannelAs(
                        command,
                        TestConstants.Channel1,
                        TestConstants.NormalUser
                    );

                    // We should get the plugin name, the command from the server, and the message
                    // the exception contains.
                    this.testFrame.ProcessRunner.WaitForStringFromChaskis(
                        @$"{TestConstants.RegressionTestCommandPrefix}.+threw.+{command}.+{message}"
                    ).FailIfFalse( "Did not get our exception message" );
                }
            );

            Step.Run(
                "Are we still there?",
                () => this.testFrame.CanaryTest()
            );
        }

        /// <summary>
        /// Ensures if we throw an exception, we do NOT get a ThreadInterruptedException
        /// anywhere in the string parsing queue.  This is a long test,
        /// and we remain idle for some time minutes while the watchdogs run.
        /// </summary>
        [Test]
        public void DoNoInterruptsFoundTest()
        {
            ThreadInterruptedException exception = new ThreadInterruptedException();

            string regex = $@"({exception.Message.Replace( " ", @"\s+" )})|(Thread\s+was\s+interrupted)";

            using( StringWatcher watcher = this.testFrame.ProcessRunner.CreateStringWatcher( regex ) )
            {
                const string message = "My Exception";

                Step.Run(
                    "Throwing exception on string parsing queue",
                    () =>
                    {
                        string command = $"!{TestConstants.RegressionTestCommandPrefix} throw {message}";
                        this.testFrame.IrcServer.SendMessageToChannelAs(
                            command,
                            TestConstants.Channel1,
                            TestConstants.NormalUser
                        );

                    // We should get the plugin name, the command from the server, and the message
                    // the exception contains.
                    this.testFrame.ProcessRunner.WaitForStringFromChaskis(
                            @$"{TestConstants.RegressionTestCommandPrefix}.+threw.+{command}.+{message}"
                        ).FailIfFalse( "Did not get our exception message" );
                    }
                );

                Step.Run(
                    $"Ensure we do not see {nameof( ThreadInterruptedException )}'s anywhere",
                    () =>
                    {
                        for( int i = 1; i <= 5; ++i )
                        {
                            this.testFrame.IrcServer.WaitForString( @"PING\s+watchdog", 90 * 1000 )
                                .FailIfFalse("Didn't get watchdog for somet reason...");
                        }
                    }
                );

                Step.Run(
                    "Are we still there?",
                    () => this.testFrame.CanaryTest()
                );

                Assert.IsFalse( watcher.SawString, $"Saw a {nameof( ThreadInterruptedException )} somewhere" );
            }
        }
    }
}
