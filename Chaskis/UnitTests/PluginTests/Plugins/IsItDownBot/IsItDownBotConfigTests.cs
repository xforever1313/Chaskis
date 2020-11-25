//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using NUnit.Framework;
using Chaskis.Plugins.IsItDownBot;
using SethCS.Exceptions;

namespace Chaskis.UnitTests.PluginTests.Plugins.IsItDownBot
{
    [TestFixture]
    public class IsItDownBotConfigTests
    {
        // ---------------- Fields ----------------

        // ---------------- Setup / Teardown ----------------

        [OneTimeSetUp]
        public void FixtureSetup()
        {
        }

        [OneTimeTearDown]
        public void FixtureTeardown()
        {
        }

        [SetUp]
        public void TestSetup()
        {
        }

        [TearDown]
        public void TestTeardown()
        {
        }

        // ---------------- Tests ----------------

        [Test]
        public void WebsiteValidationTest()
        {
            Website uut = new Website();

            // Should fail right out-of-the-gate.
            Assert.Throws<ValidationException>( () => uut.Validate() );

            // Make valid config.  No channels is valid (means we want to send to all of them)
            {
                uut.Url = "https://shendrick.net/";
                uut.CheckInterval = new TimeSpan( 1, 0, 0 );
                Assert.DoesNotThrow( () => uut.Validate() );
            }

            // 1 channel is valid.
            {
                uut.Channels.Add( "#hello" );
                Assert.DoesNotThrow( () => uut.Validate() );
            }

            // 2 channels is valid.
            {
                uut.Channels.Add( "#hello2" );
                Assert.DoesNotThrow( () => uut.Validate() );
            }

            // Null/empty URL is invalid
            {
                uut.Url = null;
                Assert.Throws<ValidationException>( () => uut.Validate() );

                uut.Url = string.Empty;
                Assert.Throws<ValidationException>( () => uut.Validate() );

                uut.Url = "        ";
                Assert.Throws<ValidationException>( () => uut.Validate() );

                // Back to valid config for next test.
                uut.Url = "https://shendrick.net/";
                Assert.DoesNotThrow( () => uut.Validate() );
            }

            // Negative or zero timespan is not okay.
            {
                uut.CheckInterval = TimeSpan.Zero;
                Assert.Throws<ValidationException>( () => uut.Validate() );

                uut.CheckInterval = new TimeSpan( 0, 0, -1 );
                Assert.Throws<ValidationException>( () => uut.Validate() );

                // Back to valid value:
                uut.CheckInterval = new TimeSpan( 0, 0, 1 );
                Assert.DoesNotThrow( () => uut.Validate() );
            }

            // Null/empty channels not okay.
            {
                uut.Channels[0] = string.Empty;
                Assert.Throws<ValidationException>( () => uut.Validate() );

                uut.Channels[0] = "       ";
                Assert.Throws<ValidationException>( () => uut.Validate() );

                uut.Channels[0] = null;
                Assert.Throws<ValidationException>( () => uut.Validate() );
            }
        }

        [Test]
        public void IsItDownBotConfigValidateTest()
        {
            IsItDownBotConfig uut = new IsItDownBotConfig();
            Assert.AreEqual( "!isitdown", uut.CommandPrefix ); // Ensure default value is there.

            // Should validate right-out-the-gate (we have a default value).
            Assert.DoesNotThrow( () => uut.Validate() );

            // Add website, should still validate.
            {
                Website site1 = new Website()
                {
                    CheckInterval = new TimeSpan( 1, 0, 0 ),
                    Url = "https://shendrick.net"
                };
                Assert.DoesNotThrow( () => site1.Validate() );

                uut.Websites.Add( site1 );
                Assert.DoesNotThrow( () => uut.Validate() );
            }

            // Command prefix should not be null/empty/whitespace
            {
                uut.CommandPrefix = string.Empty;
                Assert.Throws<ValidationException>( () => uut.Validate() );

                uut.CommandPrefix = null;
                Assert.Throws<ValidationException>( () => uut.Validate() );

                uut.CommandPrefix = "        ";
                Assert.Throws<ValidationException>( () => uut.Validate() );

                // Back to valid value.
                uut.CommandPrefix = "!isitdown";
                Assert.DoesNotThrow( () => uut.Validate() );
            }

            // Invalid website should throw validation exception.
            {
                Website site2 = new Website();
                Assert.Throws<ValidationException>( () => site2.Validate() );

                uut.Websites.Add( site2 );
                Assert.Throws<ValidationException>( () => uut.Validate() );

                // Null website should throw.
                uut.Websites[1] = null;
                Assert.Throws<ValidationException>( () => uut.Validate() );
            }
        }

        // ---------------- Test Helpers ----------------
    }
}
