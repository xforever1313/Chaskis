//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.IO;
using Chaskis.RegressionTests.TestCore;
using NUnit.Framework;

namespace Chaskis.RegressionTests.Tests.PluginTests
{
    internal interface IRssBotTestFixture
    {
        ChaskisTestFramework TestFrame { get; }

        string TestFilesLocation { get; }
    }

    public class RssBotTests
    {
        // ---------------- Fields ----------------

        private const ushort httpServerPort = 32101;

        private static readonly string url = $"http://127.0.0.1:{httpServerPort}";
        internal static string XmlFileName = "/feed.xml";
        internal static readonly string FeedUrl = $"{url}{XmlFileName}";

        // ---------------- Helper Classes ----------------

        [TestFixture]
        public class HappyPathRssBotTests : IRssBotTestFixture
        {
            // ---------------- Properties ----------------

            public ChaskisTestFramework TestFrame { get; private set; }

            public string TestFilesLocation { get; private set; }

            // ---------------- Setup / Teardown ----------------

            [OneTimeSetUp]
            public void FixtureSetup()
            {
                this.TestFrame = new ChaskisTestFramework();
                this.TestFrame.SetupLog();

                ChaskisFixtureConfig fixtureConfig = new ChaskisFixtureConfig
                {
                    Environment = "rssbotenvironment"
                };

                this.TestFilesLocation = Path.Combine(
                    this.TestFrame.EnvironmentManager.ChaskisProjectRoot,
                    "UnitTests",
                    "PluginTests",
                    "Plugins",
                    "RssBot",
                    "TestFiles"
                );

                this.TestFrame.HttpServer.MapFileToUrl(
                    Path.Combine( this.TestFilesLocation, "Reddit_Init.xml" ),
                    XmlFileName
                );
                this.TestFrame.HttpServer.StartHttpServer( httpServerPort );

                // Now, perform the common fixture setup.
                this.TestFrame.PerformFixtureSetup( fixtureConfig );

                this.SetVerbosity();
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
                this.TestFrame.HttpServer.SetHttpServerHangTimeTo( 0 );
            }

            [TearDown]
            public void TestTeardown()
            {
                this.TestFrame.PerformTestTeardown();
            }

            // ---------------- Tests ----------------

            /// <summary>
            /// Ensures the plugin loads by itself without issue.
            /// </summary>
            [Test]
            public void DoPluginLoadTest()
            {
                CommonPluginTests.DoPluginLoadTest( this.TestFrame, "rssbot" );
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

                // Now do an update.
                this.DoUpdateTest();

                Step.Run(
                    "Update feed with less items in it.  Should get no response",
                    () =>
                    {
                        this.TestFrame.HttpServer.MapFileToUrl(
                            Path.Join( this.TestFilesLocation, "Reddit_Init_Less_Items.xml" ),
                            XmlFileName
                        );

                        this.TriggerUpdate();
                        this.WaitForNoUpdatesMessage();
                    }
                );
            }
        }

        /// <summary>
        /// Ensures if our bot gets a 404, we don't crash.
        /// </summary>
        [TestFixture]
        public class RssBot404Tests : IRssBotTestFixture
        {
            // ---------------- Properties ----------------

            public ChaskisTestFramework TestFrame { get; private set; }

            public string TestFilesLocation { get; private set; }

            // ---------------- Setup / Teardown ----------------

            [OneTimeSetUp]
            public void FixtureSetup()
            {
                this.TestFrame = new ChaskisTestFramework();
                this.TestFrame.SetupLog();

                ChaskisFixtureConfig fixtureConfig = new ChaskisFixtureConfig
                {
                    Environment = "rssbotenvironment"
                };

                this.TestFilesLocation = Path.Combine(
                    this.TestFrame.EnvironmentManager.ChaskisProjectRoot,
                    "UnitTests",
                    "PluginTests",
                    "Plugins",
                    "RssBot",
                    "TestFiles"
                );

                // Defer to the test to start the HTTP server.  What we expect to
                // happen here is the process to NOT crash if it
                // gets an exception while trying to get the RSS feed.

                // Now, perform the common fixture setup.
                this.TestFrame.PerformFixtureSetup( fixtureConfig );

                this.SetVerbosity();
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
                this.TestFrame.HttpServer.SetHttpServerHangTimeTo( 0 );
            }

            [TearDown]
            public void TestTeardown()
            {
                this.TestFrame.PerformTestTeardown();
            }

            // ---------------- Tests ----------------

            [Test]
            public void DoUpdateAfterFailureTest()
            {
                this.TestFrame.HttpServer.MapFileToUrl(
                    Path.Combine( this.TestFilesLocation, "Reddit_Init.xml" ),
                    XmlFileName
                );
                this.TestFrame.HttpServer.StartHttpServer( httpServerPort );

                // We should trigger and ensure nothing goes out.
                // we don't want to spam anything by getting our first update.

                Step.Run(
                    "Ensure our first update results in no messages being sent; do not want to spam channels.",
                    () =>
                    {
                        this.TriggerUpdate();
                        this.WaitForNoUpdatesMessage();
                    }
                );

                // Now do an update.
                this.DoUpdateTest();
            }
        }
    }

    internal static class IRssBotTestFixtureExtensions
    {
        public static void SetVerbosity( this IRssBotTestFixture fixture )
        {
            fixture.TestFrame.IrcServer.SendMessageToChannelAs(
                $"!{TestConstants.BotName} debug verbosity rssbot 3",
                TestConstants.BotName,
                TestConstants.AdminUserName
            );

            fixture.TestFrame.IrcServer.WaitForMessageOnChannel(
                @"'rssbot'\s+log\s+verbosity\s+has\s+been\s+set\s+to\s+'3'",
                TestConstants.AdminUserName
            ).FailIfFalse( "Could not turn on RSS Debug" );
        }

        public static void TriggerUpdate( this IRssBotTestFixture fixture )
        {
            fixture.TestFrame.IrcServer.SendMessageToChannelAs(
                $"!debug rssbot updatefeed {RssBotTests.FeedUrl}",
                TestConstants.Channel1,
                TestConstants.AdminUserName
            );
        }

        public static void WaitForNoUpdatesMessage( this IRssBotTestFixture fixture )
        {
            fixture.TestFrame.ProcessRunner.WaitForStringFromChaskis(
                @"No\s+updates\s+for\s+RSS\s+feed",
                10 * 1000
            ).FailIfFalse( "Did not get debug message from process" );
        }

        public static void DoUpdateTest( this IRssBotTestFixture fixture )
        {
            Step.Run(
                "Update feed with more items in it.",
                () =>
                {
                    fixture.TestFrame.HttpServer.MapFileToUrl(
                        Path.Join( fixture.TestFilesLocation, "Reddit_Update.xml" ),
                        RssBotTests.XmlFileName
                    );

                    fixture.TriggerUpdate();

                    fixture.TestFrame.IrcServer.WaitForMessageOnChannel(
                        @".+Welcome\s+to\s+/r/csharp!\s+Read\s+this\s+post\s+before\s+submitting\..+",
                        TestConstants.Channel1
                    ).FailIfFalse( "Did not get feed updates" );

                    fixture.TestFrame.IrcServer.WaitForMessageOnChannel(
                        @".+PenguinUpload\s+-\s+file\s+hosting\s+service\s+built\s+with\s+ASP\.NET\s+Core,\s+NancyFx,\s+and\s+Vue\.js.+",
                        TestConstants.Channel1
                    ).FailIfFalse( "Did not get feed updates" );

                    fixture.TestFrame.IrcServer.WaitForMessageOnChannel(
                        @".+C#\s+7\s+Series,\s+Part\s+2:\s+Async\s+Main.+",
                        TestConstants.Channel1
                    ).FailIfFalse( "Did not get feed updates" );
                }
            );
        }
    }
}
