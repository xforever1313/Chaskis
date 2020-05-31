//
//          Copyright Seth Hendrick 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.IO;
using Chaskis.RegressionTests.TestCore;
using NUnit.Framework;

namespace Chaskis.RegressionTests.Tests.PluginTests
{
    [TestFixture]
    public class RssBotTests
    {
        // ---------------- Fields ----------------

        private ChaskisTestFramework testFrame;

        private string testFilesLocation;

        private const ushort httpServerPort = 32101;

        private static readonly string url = $"http://127.0.0.1:{httpServerPort}";
        private const string xmlFileName = "/feed.xml";
        private static readonly string feedUrl = $"{url}{xmlFileName}";

        // ---------------- Setup / Teardown ----------------

        [OneTimeSetUp]
        public void FixtureSetup()
        {
            this.testFrame = new ChaskisTestFramework();
            this.testFrame.SetupLog();

            ChaskisFixtureConfig fixtureConfig = new ChaskisFixtureConfig
            {
                Environment = "rssbotenvironment"
            };

            this.testFilesLocation = Path.Combine(
                this.testFrame.EnvironmentManager.ChaskisProjectRoot,
                "UnitTests",
                "PluginTests",
                "Plugins",
                "RssBot",
                "TestFiles"
            );

            this.testFrame.HttpServer.MapFileToUrl(
                Path.Combine( this.testFilesLocation, "Reddit_Init.xml" ),
                xmlFileName
            );
            this.testFrame.HttpServer.StartHttpServer( httpServerPort );

            // Now, perform the common fixture setup.
            this.testFrame.PerformFixtureSetup( fixtureConfig );

            this.testFrame.IrcServer.SendMessageToChannelAs(
                $"!{TestConstants.BotName} debug verbosity rssbot 3",
                TestConstants.BotName,
                TestConstants.AdminUserName
            );

            this.testFrame.IrcServer.WaitForMessageOnChannel(
                @"'rssbot'\s+log\s+verbosity\s+has\s+been\s+set\s+to\s+'3'",
                TestConstants.AdminUserName
            ).FailIfFalse( "Could not turn on RSS Debug" );
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
            this.testFrame.HttpServer.SetHttpServerHangTimeTo( 0 );
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
            CommonPluginTests.DoPluginLoadTest( this.testFrame, "rssbot" );
        }

        [Test]
        public void UpdateTest()
        {
            Step.Run(
                "Ensure if there are no updates, no messages get sent.",
                () =>
                {
                    this.TriggerUpdate();
                    this.WaitForNoUpdatesMessage();
                }
            );

            Step.Run(
                "Update feed with more items in it.",
                () =>
                {
                    this.testFrame.HttpServer.MapFileToUrl(
                        Path.Join( this.testFilesLocation, "Reddit_Update.xml" ),
                        xmlFileName
                    );

                    this.TriggerUpdate();

                    this.testFrame.IrcServer.WaitForMessageOnChannel(
                        @".+Welcome\s+to\s+/r/csharp!\s+Read\s+this\s+post\s+before\s+submitting\..+",
                        TestConstants.Channel1
                    ).FailIfFalse( "Did not get feed updates" );

                    this.testFrame.IrcServer.WaitForMessageOnChannel(
                        @".+PenguinUpload\s+-\s+file\s+hosting\s+service\s+built\s+with\s+ASP\.NET\s+Core,\s+NancyFx,\s+and\s+Vue\.js.+",
                        TestConstants.Channel1
                    ).FailIfFalse( "Did not get feed updates" );

                    this.testFrame.IrcServer.WaitForMessageOnChannel(
                        @".+C#\s+7\s+Series,\s+Part\s+2:\s+Async\s+Main.+",
                        TestConstants.Channel1
                    ).FailIfFalse( "Did not get feed updates" );
                }
            );

            Step.Run(
                "Update feed with less items in it.  Should get no response",
                () =>
                {
                    this.testFrame.HttpServer.MapFileToUrl(
                        Path.Join( this.testFilesLocation, "Reddit_Init_Less_Items.xml" ),
                        xmlFileName
                    );

                    this.TriggerUpdate();
                    this.WaitForNoUpdatesMessage();
                }
            );
        }

        /// <summary>
        /// Ensures if the server response hangs, we get an abort exception.
        /// </summary>
        [Test]
        public void AbortTest()
        {
            Step.Run(
                "Try an update.  We expect an exception to occur due to a timeout",
                () =>
                {
                    this.testFrame.HttpServer.SetHttpServerHangTimeTo( 15 * 1000 );

                    this.TriggerUpdate();

                    this.testFrame.ProcessRunner.WaitForStringFromChaskis(
                        @"An\s+Exception\s+was\s+caught\s+while\s+updating\s+feed",
                        20 * 1000
                    ).FailIfFalse( "Did not get exception message" );

                    this.testFrame.ProcessRunner.WaitForStringFromChaskis(
                        @"System\.Threading\.Tasks\.TaskCanceledException",
                        5 * 1000
                    ).FailIfFalse( "Did not get TaskCanceledException" );
                }
            );

            Step.Run(
                "Ensure we didn't break anything, do another update as a sanity check",
                () =>
                {
                    this.testFrame.HttpServer.SetHttpServerHangTimeTo( 0 );

                    this.TriggerUpdate();
                    this.WaitForNoUpdatesMessage();
                }
            );
        }

        // ---------------- Test Helpers ----------------

        private void TriggerUpdate()
        {
            this.testFrame.IrcServer.SendMessageToChannelAs(
                $"!debug rssbot updatefeed {feedUrl}",
                TestConstants.Channel1,
                TestConstants.AdminUserName
            );
        }

        private void WaitForNoUpdatesMessage()
        {
            this.testFrame.ProcessRunner.WaitForStringFromChaskis(
                @"No\s+updates\s+for\s+RSS\s+feed",
                10 * 1000
            ).FailIfFalse( "Did not get debug message from process" );
        }
    }
}
