//
//          Copyright Seth Hendrick 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Chaskis.RegressionTests.TestCore;
using NUnit.Framework;

namespace Chaskis.RegressionTests.Tests.CoreTests
{
    [TestFixture]
    public class ReconnectTests
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
            this.testFrame.IrcServer.SetPingResponse( true );
            this.testFrame.PerformTestSetup();
        }

        [TearDown]
        public void TestTeardown()
        {
            this.testFrame.IrcServer.SetPingResponse( true );
            this.testFrame.PerformTestTeardown();
        }

        // ---------------- Tests ----------------

        /// <summary>
        /// Ensures we can reconnect when our watchdog times out.
        /// </summary>
        [Test]
        public void ReconnectTest()
        {
            // Disable pings
            this.testFrame.IrcServer.SetPingResponse( false );

            // Wait for a watchdog message to come from the client.
            this.testFrame.IrcServer.WaitForString( @"PING\s+watchdog", 70 * 1000 );

            // Now, wait for us to disconnect.
            Step.Run(
                "Wait for client to disconnect",
                () =>
                {
                    this.testFrame.ProcessRunner.WaitForStringFromChaskis(
                        @"<chaskis_event source_type=""CORE""\s+source_plugin=""IRC""\s+dest_plugin=""""><args><event_id>WATCHDOG\s+FAILED</event_id><server>(?<server>\S+)</server><nick>chaskisbot</nick></args><passthrough_args /></chaskis_event>",
                        150 * 1000
                    ).FailIfFalse( "Did not get watchdog failed message" );

                    this.testFrame.ProcessRunner.WaitForStringFromChaskis(
                        @"<chaskis_disconnecting_event><server>(?<server>\S+)</server><protocol>IRC</protocol></chaskis_disconnecting_event>",
                        15 * 1000
                    ).FailIfFalse( "Did not get disconnecting message" );

                    this.testFrame.ProcessRunner.WaitForStringFromChaskis(
                        @"<chaskis_event source_type=""CORE""\s+source_plugin=""IRC""\s+dest_plugin=""""><args><event_id>DISCONNECTED</event_id><server>(?<server>\S+)</server><nick>chaskisbot</nick></args><passthrough_args /></chaskis_event>",
                        15 * 1000
                    ).FailIfFalse( "Did not get disconnect message" );
                }
            );

            // We disconnect, reset server settings.
            this.testFrame.IrcServer.SetPingResponse( true );
            this.testFrame.IrcServer.ResetServerConnection();

            Step.Run(
                "Wait for client to reconnect",
                () =>
                {
                    this.testFrame.ProcessRunner.WaitForStringFromChaskis(
                        @"<chaskis_event source_type=""CORE""\s+source_plugin=""IRC""\s+dest_plugin=""""><args><event_id>ATTEMPTING\s+RECONNECT</event_id><server>(?<server>\S+)</server><nick>chaskisbot</nick></args><passthrough_args /></chaskis_event>",
                        70 * 1000 // Takes a minute to reconnect.
                    ).FailIfFalse( "Did not get reconnecting message" );

                    this.testFrame.IrcServer.WaitForConnection().FailIfFalse( "Server did not get connection" );

                    this.testFrame.ProcessRunner.WaitForClientToConnect();
                    this.testFrame.ProcessRunner.WaitToFinishJoiningChannels();
                }
            );

            Step.Run(
                "Check Comms",
                () => this.testFrame.CanaryTest()
            );
        }
    }
}
