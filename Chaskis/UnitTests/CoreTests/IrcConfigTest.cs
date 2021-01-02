//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using Chaskis.UnitTests.Common;
using Chaskis.Core;
using NUnit.Framework;
using SethCS.Exceptions;

namespace Chaskis.UnitTests.CoreTests
{
    [TestFixture]
    public class IrcConfigTest
    {
        // -------- Fields --------

        /// <summary>
        /// Irc config under test.
        /// </summary>
        private IrcConfig ircConfig;

        // -------- Setup / Teardown --------

        [SetUp]
        public void TestSetup()
        {
            this.ircConfig = TestHelpers.GetTestIrcConfig();
        }

        // -------- Tests --------

        /// <summary>
        /// Ensures the equals method works correctly.
        /// </summary>
        [Test]
        public void EqualsTest()
        {
            IReadOnlyIrcConfig interfaceIrcConfig = TestHelpers.GetTestIrcConfig();

            // Ensure both the interface and the RO config are equal, but not the same reference.
            Assert.AreNotSame( interfaceIrcConfig, this.ircConfig );

            // Ensure everything is equal.
            Assert.AreEqual( interfaceIrcConfig, this.ircConfig );
            Assert.AreEqual( this.ircConfig, interfaceIrcConfig );

            // BridgeBots should not be same reference.
            Assert.AreNotSame( interfaceIrcConfig.BridgeBots, this.ircConfig.BridgeBots );

            // Admins should not be same reference
            Assert.AreNotSame( interfaceIrcConfig.Admins, this.ircConfig.Admins );

            // Next, start changing things.  Everything should become false.
            this.ircConfig.Server = "irc.somewhere.net";
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            this.ircConfig.Channels.Add( "#derp" );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            this.ircConfig.Port = 9876;
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            this.ircConfig.UseSsl = true;
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            this.ircConfig.UserName = "Nate";
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            this.ircConfig.Nick = "Nate";
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            this.ircConfig.RealName = "Nate A Saurus";
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            this.ircConfig.ServerPassword = "SERVER_PASSWORD";
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            this.ircConfig.NickServPassword = "ABadPassword";
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            this.ircConfig.QuitMessage = "A quit message";
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            // Add an additional bot.
            this.ircConfig.BridgeBots["slackBridge"] = @"(?<bridgeUser>\w+):\s+(?<bridgeMessage>.+)";
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            // Change an existing bot's value.
            this.ircConfig.BridgeBots["telegrambot"] = @"(?<bridgeUser>\w+)-\s+(?<bridgeMessage>.+)";
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            // Add an addtional Admin
            this.ircConfig.Admins.Add( "person3" );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            // Rate Limit
            this.ircConfig.RateLimit = this.ircConfig.RateLimit + 1;
            this.ircConfig = TestHelpers.GetTestIrcConfig();
        }

        [Test]
        public void ValidateTest()
        {
            // Ensure a valid config does not throw.
            Assert.DoesNotThrow( () => this.ircConfig.Validate() );

            // Empty bridge bots config should be valid.
            this.ircConfig.BridgeBots.Clear();
            Assert.DoesNotThrow( () => this.ircConfig.Validate() );

            // Empty password should validate
            this.ircConfig.ServerPassword = string.Empty;
            Assert.DoesNotThrow( () => this.ircConfig.Validate() );

            // Empty password should validate
            this.ircConfig.NickServPassword = string.Empty;
            Assert.DoesNotThrow( () => this.ircConfig.Validate() );

            // 160 characters on quit message is okay.
            this.ircConfig.QuitMessage = "1234567891123456789212345678931234567894123456789512345678961234567897123456789812345678991234567890123456789112345678921234567893123456789412345678951234567896";
            Assert.DoesNotThrow( () => this.ircConfig.Validate() );

            // Change back.
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            // Now start changing stuff.
            this.ircConfig.Server = string.Empty;
            CheckNotValid( this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            this.ircConfig.Channels.Add( string.Empty );
            CheckNotValid( this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            this.ircConfig.Channels.Add( null );
            CheckNotValid( this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            this.ircConfig.Channels.Clear();
            CheckNotValid( this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            this.ircConfig.Port = -1;
            CheckNotValid( this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            this.ircConfig.UserName = string.Empty;
            CheckNotValid( this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            this.ircConfig.Nick = string.Empty;
            CheckNotValid( this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            this.ircConfig.RealName = string.Empty;
            CheckNotValid( this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            // Quit message more than 160 characters:
            this.ircConfig.QuitMessage = "12345678911234567892123456789312345678941234567895123456789612345678971234567898123456789912345678901234567891123456789212345678931234567894123456789512345678961";
            CheckNotValid( this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            // Quit message can not be null.
            this.ircConfig.QuitMessage = null;
            CheckNotValid( this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            // No bridge message
            this.ircConfig.BridgeBots["telegrambot"] = @"(?<bridgeUser>\w+)-\s+(?<bridgeMsg>.+)";
            CheckNotValid( this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            // No bridge user
            this.ircConfig.BridgeBots["telegrambot"] = @"(?<bridgeUsr>\w+)-\s+(?<bridgeMessage>.+)";
            CheckNotValid( this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            // Empty string.
            this.ircConfig.BridgeBots["telegrambot"] = string.Empty;
            CheckNotValid( this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            // null string.
            this.ircConfig.BridgeBots["telegrambot"] = null;
            CheckNotValid( this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            // Empty key
            this.ircConfig.BridgeBots[string.Empty] = @"(?<bridgeUser>\w+)-\s+(?<bridgeMessage>.+)";
            CheckNotValid( this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            // Empty Admin
            this.ircConfig.Admins.Add( string.Empty );
            CheckNotValid( this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            // Null Admin
            this.ircConfig.Admins.Add( null );
            CheckNotValid( this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            // Negative rate limit.
            this.ircConfig.RateLimit = -1;
            CheckNotValid( this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();
        }

        // -------- Test Helpers --------

        /// <summary>
        /// Ensures the three different types of IRC Configs are not equal.
        /// </summary>
        /// <param name="ircConfig">The IRC config object to test.</param>
        /// <param name="interfaceConfig">The interfaced IRC object to test.</param>
        /// <param name="roIrcConfig">The read-only IRC object to test.</param>
        private void CheckNotEqual( IrcConfig ircConfig, IReadOnlyIrcConfig interfaceConfig )
        {
            // Ensure not equals works going from real -> interface
            Assert.AreNotEqual( ircConfig, interfaceConfig );
            Assert.AreNotEqual( interfaceConfig, ircConfig );

            // Ensure not equals works going from interface -> interface
            IReadOnlyIrcConfig iircConfig = ircConfig;
            Assert.AreNotEqual( iircConfig, interfaceConfig );
            Assert.AreNotEqual( interfaceConfig, iircConfig );
        }

        /// <summary>
        /// Ensures the given IRCConfig is not valid.
        /// </summary>
        /// <param name="config">The config to check.</param>
        private void CheckNotValid( IrcConfig config )
        {
            Assert.Throws<ValidationException>( () => config.Validate() );

            IReadOnlyIrcConfig iircConfig = config;
            Assert.Throws<ValidationException>( () => iircConfig.Validate() );
        }
    }
}