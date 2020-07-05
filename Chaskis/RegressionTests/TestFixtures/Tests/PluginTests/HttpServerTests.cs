//
//          Copyright Seth Hendrick 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;
using System.Net;
using Chaskis.RegressionTests.TestCore;
using NUnit.Framework;

namespace Chaskis.RegressionTests.Tests.PluginTests
{
    [TestFixture]
    public class HttpServerTests
    {
        // ---------------- Fields ----------------

        private const ushort httpPort = 10080;

        private static readonly string url = "http://localhost:" + httpPort;

        private ChaskisTestFramework testFrame;

        // ---------------- Setup / Teardown ----------------

        [OneTimeSetUp]
        public void FixtureSetup()
        {
            this.testFrame = new ChaskisTestFramework();

            ChaskisFixtureConfig fixtureConfig = new ChaskisFixtureConfig
            {
                Environment = "HttpServerEnvironment"
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
            CommonPluginTests.DoPluginLoadTest( this.testFrame, "httpserver" );
        }

        // -------- Action --------

        /// <summary>
        /// Ensures we send action messages via HTTP correctly.
        /// </summary>
        [Test]
        public void ActionMessagePostRequestTest()
        {
            Step.Run(
                "Message to channel 1",
                () =>
                {
                    this.testFrame.HttpClient.SendPostRequestToWithQueryExpect(
                        $"{url}/action",
                        $"message=somemessage&channel={TestConstants.Channel1}",
                        HttpStatusCode.OK
                    ).FailIfFalse( "Error when sending HTTP Request" );

                    this.testFrame.IrcServer.WaitForString(
                        @"PRIVMSG\s+" + TestConstants.Channel1 + @"\s+:ACTION somemessage"
                    ).FailIfFalse( "Did not get response from bot" );
                }
            );

            Step.Run(
                "Message to channel 2",
                () =>
                {
                    this.testFrame.HttpClient.SendPostRequestToWithQueryExpect(
                        $"{url}/action",
                        $"message=somemessage&channel={TestConstants.Channel2}",
                        HttpStatusCode.OK
                    ).FailIfFalse( "Error when sending HTTP Request" );

                    this.testFrame.IrcServer.WaitForString(
                        @"PRIVMSG\s+" + TestConstants.Channel2 + @"\s+:ACTION somemessage"
                    ).FailIfFalse( "Did not get response from bot" );
                }
            );
        }

        /// <summary>
        /// Ensures if we send a bad action query string, we get an error response.
        /// </summary>
        [Test]
        public void BadActionQueryStringTest()
        {
            Step.Run(
                "Missing message",
                () =>
                {
                    this.testFrame.HttpClient.SendPostRequestToWithQueryExpect(
                        $"{url}/action",
                        $"channel={TestConstants.Channel1}",
                        HttpStatusCode.BadRequest
                    ).FailIfFalse( "Did not get bad request." );
                }
            );

            Step.Run(
                "Missing channel",
                () =>
                {
                    this.testFrame.HttpClient.SendPostRequestToWithQueryExpect(
                        $"{url}/action",
                        $"message=somemessage",
                        HttpStatusCode.BadRequest
                    ).FailIfFalse( "Did not get bad request." );
                }
            );

            Step.Run(
                "Missing everything",
                () =>
                {
                    this.testFrame.HttpClient.SendPostRequestToWithQueryExpect(
                        $"{url}/action",
                        $"lol=somemessage",
                        HttpStatusCode.BadRequest
                    ).FailIfFalse( "Did not get bad request." );
                }
            );
        }

        // -------- Broadcast --------

        /// <summary>
        /// Ensures we send action messages via HTTP correctly.
        /// </summary>
        [Test]
        public void BCastMessagePostRequestTest()
        {
            Step.Run(
                "BCast with shortened URL",
                () =>
                {
                    this.testFrame.HttpClient.SendPostRequestToWithQueryExpect(
                        $"{url}/bcast",
                        $"message=somemessage",
                        HttpStatusCode.OK
                    ).FailIfFalse( "Error when sending HTTP Request" );

                    this.testFrame.IrcServer.WaitForString(
                        @"PRIVMSG\s" + TestConstants.Channel1 + @"\s+:somemessage"
                    ).FailIfFalse( "Did not get response from bot" );
                    
                    this.testFrame.IrcServer.WaitForString(
                        @"PRIVMSG\s+" + TestConstants.Channel2 + @"\s+:somemessage"
                    ).FailIfFalse( "Did not get response from bot" );
                }
            );

            Step.Run(
                "BCast with long URL",
                () =>
                {
                    this.testFrame.HttpClient.SendPostRequestToWithQueryExpect(
                        $"{url}/broadcast",
                        $"message=somemessage2",
                        HttpStatusCode.OK
                    ).FailIfFalse( "Error when sending HTTP Request" );

                    this.testFrame.IrcServer.WaitForString(
                        @"PRIVMSG\s+" + TestConstants.Channel1 + @"\s+:somemessage2"
                    ).FailIfFalse( "Did not get response from bot" );

                    this.testFrame.IrcServer.WaitForString(
                        @"PRIVMSG\s+" + TestConstants.Channel2 + @"\s+:somemessage2"
                    ).FailIfFalse( "Did not get response from bot" );
                }
            );
        }

        /// <summary>
        /// Ensures if we havea bad query string with the bcast, we get an error message.
        /// </summary>
        [Test]
        public void BadBCastQueryStringTest()
        {
            Step.Run(
                "BCast with shortened URL",
                () =>
                {
                    this.testFrame.HttpClient.SendPostRequestToWithQueryExpect(
                        $"{url}/bcast",
                        $"lol=somemessage2",
                        HttpStatusCode.BadRequest
                    ).FailIfFalse( "Did not get bad request." );
        }
            );

            Step.Run(
                "BCast with long URL",
                () =>
                {
                    this.testFrame.HttpClient.SendPostRequestToWithQueryExpect(
                        $"{url}/broadcast",
                        $"lol=somemessage3",
                        HttpStatusCode.BadRequest
                    ).FailIfFalse( "Did not get bad request." );
                }
            );
        }

        // -------- Kicks --------

        [Test]
        public void KickUserTest()
        {
            string urlPath = $"{url}/kick";

            Step.Run(
                "No reason specified",
                () =>
                {
                    this.testFrame.HttpClient.SendPostRequestToWithQueryExpect(
                        urlPath,
                        $"user={TestConstants.NormalUser}&channel={TestConstants.Channel1}",
                        HttpStatusCode.OK
                    ).FailIfFalse( "Error when sending HTTP Request" );

                    this.testFrame.IrcServer.WaitForString(
                        @"KICK\s+" + TestConstants.Channel1 + @"\s+" + TestConstants.NormalUser
                    ).FailIfFalse( "Did not get response from bot" );
                }
            );

            Step.Run(
                "Reason Specified",
                () =>
                {
                    this.testFrame.HttpClient.SendPostRequestToWithQueryExpect(
                        urlPath,
                        $"user={TestConstants.NormalUser}&channel={TestConstants.Channel1}&message=somemessage",
                        HttpStatusCode.OK
                    ).FailIfFalse( "Error when sending HTTP Request" );

                    this.testFrame.IrcServer.WaitForString(
                        @"KICK\s+" + TestConstants.Channel1 + @"\s+" + TestConstants.NormalUser + @"\s+:somemessage"
                    ).FailIfFalse( "Did not get response from bot" );
                }
            );
        }

        [Test]
        public void BadKickUserQueryStringTest()
        {
            string urlPath = $"{url}/kick";

            Step.Run(
                "Missing channel",
                () =>
                {
                    this.testFrame.HttpClient.SendPostRequestToWithQueryExpect(
                        urlPath,
                        $"user={TestConstants.NormalUser}&message=somemessage",
                        HttpStatusCode.BadRequest
                    ).FailIfFalse( "Did not get bad request." );
                }
            );

            Step.Run(
                "Missing User",
                () =>
                {
                    this.testFrame.HttpClient.SendPostRequestToWithQueryExpect(
                        urlPath,
                        $"channel={TestConstants.Channel2}&message=somemessage",
                        HttpStatusCode.BadRequest
                    ).FailIfFalse( "Did not get bad request." );
                }
            );

            Step.Run(
                "Missing everything",
                () =>
                {
                    this.testFrame.HttpClient.SendPostRequestToWithQueryExpect(
                        urlPath,
                        $"lol=somemessage",
                        HttpStatusCode.BadRequest
                    ).FailIfFalse( "Did not get bad request." );
                }
            );
        }

        // -------- PRIVMSG --------

        [Test]
        public void PrivmsgUserTest()
        {
            string urlPath = $"{url}/privmsg";

            Step.Run(
                "Send to channel 1",
                () =>
                {
                    this.testFrame.HttpClient.SendPostRequestToWithQueryExpect(
                        urlPath,
                        $"message=somemessage&channel={TestConstants.Channel1}",
                        HttpStatusCode.OK
                    ).FailIfFalse( "Error when sending HTTP Request" );

                    this.testFrame.IrcServer.WaitForString(
                        @"PRIVMSG\s+" + TestConstants.Channel1 + @"\s+:somemessage"
                    ).FailIfFalse( "Did not get response from bot" );
                }
            );

            Step.Run(
                "Send to channel 2",
                () =>
                {
                    this.testFrame.HttpClient.SendPostRequestToWithQueryExpect(
                        urlPath,
                        $"user={TestConstants.NormalUser}&channel={TestConstants.Channel2}&message=somemessage",
                        HttpStatusCode.OK
                    ).FailIfFalse( "Error when sending HTTP Request" );

                    this.testFrame.IrcServer.WaitForString(
                        @"PRIVMSG\s+" + TestConstants.Channel2 + @"\s+:somemessage"
                    ).FailIfFalse( "Did not get response from bot" );
                }
            );
        }

        [Test]
        public void BadPrivmsgUserQueryStringTest()
        {
            string urlPath = $"{url}/privmsg";

            Step.Run(
                "Missing message",
                () =>
                {
                    this.testFrame.HttpClient.SendPostRequestToWithQueryExpect(
                        urlPath,
                        $"channel={TestConstants.Channel2}",
                        HttpStatusCode.BadRequest
                    ).FailIfFalse( "Did not get bad request." );
                }
            );

            Step.Run(
                "Missing channel",
                () =>
                {
                    this.testFrame.HttpClient.SendPostRequestToWithQueryExpect(
                        urlPath,
                        $"message=somemessage",
                        HttpStatusCode.BadRequest
                    ).FailIfFalse( "Did not get bad request." );
                }
            );

            Step.Run(
                "Missing everything",
                () =>
                {
                    this.testFrame.HttpClient.SendPostRequestToWithQueryExpect(
                        urlPath,
                        $"lol=somemessage",
                        HttpStatusCode.BadRequest
                    ).FailIfFalse( "Did not get bad request." );
                }
            );
        }

        // -------- Parts --------

        [Test]
        public void PartUserTest()
        {
            string urlPath = $"{url}/part";

            Step.Run(
                "No reason specified",
                () =>
                {
                    this.testFrame.HttpClient.SendPostRequestToWithQueryExpect(
                        urlPath,
                        $"channel={TestConstants.Channel1}",
                        HttpStatusCode.OK
                    ).FailIfFalse( "Error when sending HTTP Request" );

                    this.testFrame.IrcServer.WaitForString(
                        @"PART\s+" + TestConstants.Channel1 + @"\s+:I'm going down!"
                    ).FailIfFalse( "Did not get response from bot" );
                }
            );

            Step.Run(
                "Reason Specified",
                () =>
                {
                    this.testFrame.HttpClient.SendPostRequestToWithQueryExpect(
                        urlPath,
                        $"channel={TestConstants.Channel1}&message=somemessage",
                        HttpStatusCode.OK
                    ).FailIfFalse( "Error when sending HTTP Request" );

                    this.testFrame.IrcServer.WaitForString(
                        @"PART\s+" + TestConstants.Channel1 + @"\s+:somemessage"
                    ).FailIfFalse( "Did not get response from bot" );
                }
            );
        }

        [Test]
        public void BadPartQueryString()
        {
            string urlPath = $"{url}/part";

            Step.Run(
                "Missing everything",
                () =>
                {
                    this.testFrame.HttpClient.SendPostRequestToWithQueryExpect(
                        urlPath,
                        $"lol=somemessage",
                        HttpStatusCode.BadRequest
                    ).FailIfFalse( "Did not get bad request." );
                }
            );
        }

        // -------- Bad Requests --------

        /// <summary>
        /// Ensures we get a bad request when we do a GET request.
        /// </summary>
        [Test]
        public void GetRequestTest()
        {
            List<string> urls = new List<string>
            {
                "action",
                "bcast",
                "broadcast",
                "kick",
                "part",
                "privmsg"
            };

            foreach( string path in urls )
            {
                this.testFrame.HttpClient.SendGetRequestToExpect(
                    $"{url}/{path}",
                    HttpStatusCode.BadRequest
                ).FailIfFalse( "Did not get bad request with url " + path );
            }
        }
        
        /// <summary>
        /// Ensure if we post to a URL that doesn't exist we get a NOT FOUND error.
        /// </summary>
        [Test]
        public void PostToInvalidUrlTest()
        {
            this.testFrame.HttpClient.SendPostRequestToWithQueryExpect(
                $"{url}/fakebroadcast",
                "message=somemessage2",
                HttpStatusCode.NotFound
            ).FailIfFalse( "Did not get a not found error" );
        }
    }
}
