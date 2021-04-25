//
//          Copyright Seth Hendrick 2016-2021.
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
            this.CheckNotEqual( interfaceIrcConfig, this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            this.ircConfig.Channels.Add( "#derp" );
            this.CheckNotEqual( interfaceIrcConfig, this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            this.ircConfig.Port = 9876;
            this.CheckNotEqual( interfaceIrcConfig, this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            this.ircConfig.UseSsl = true;
            this.CheckNotEqual( interfaceIrcConfig, this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            this.ircConfig.UserName = "Nate";
            this.CheckNotEqual( interfaceIrcConfig, this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            this.ircConfig.Nick = "Nate";
            this.CheckNotEqual( interfaceIrcConfig, this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            this.ircConfig.RealName = "Nate A Saurus";
            this.CheckNotEqual( interfaceIrcConfig, this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            this.ircConfig.ServerPassword = "SERVER_PASSWORD";
            this.CheckNotEqual( interfaceIrcConfig, this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            this.ircConfig.NickServPassword = "ABadPassword";
            this.CheckNotEqual( interfaceIrcConfig, this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            this.ircConfig.NickServNick = "OtherNickServ";
            this.CheckNotEqual( interfaceIrcConfig, this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            this.ircConfig.NickServPassword = "IDENTIFY AS {%password%}";
            this.CheckNotEqual( interfaceIrcConfig, this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            this.ircConfig.QuitMessage = "A quit message";
            this.CheckNotEqual( interfaceIrcConfig, this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            // Add an additional bot.
            this.ircConfig.BridgeBots["slackBridge"] = @"(?<bridgeUser>\w+):\s+(?<bridgeMessage>.+)";
            this.CheckNotEqual( interfaceIrcConfig, this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            // Change an existing bot's value.
            this.ircConfig.BridgeBots["telegrambot"] = @"(?<bridgeUser>\w+)-\s+(?<bridgeMessage>.+)";
            this.CheckNotEqual( interfaceIrcConfig, this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            // Add an addtional Admin
            this.ircConfig.Admins.Add( "person3" );
            this.CheckNotEqual( interfaceIrcConfig, this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            // Rate Limit
            this.ircConfig.RateLimit = this.ircConfig.RateLimit + new TimeSpan( 1 );
            this.CheckNotEqual( interfaceIrcConfig, this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();
        }

        [Test]
        public void ValidateDefaultConfigTest()
        {
            // Ensure a valid config does not throw.
            Assert.DoesNotThrow( () => this.ircConfig.Validate() );
        }

        [Test]
        public void ValidateServerTest()
        {
            this.ircConfig.Server = string.Empty;
            this.CheckNotValid( this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();
        }

        [Test]
        public void ValidateChannelsTest()
        {
            this.ircConfig.Channels.Add( string.Empty );
            this.CheckNotValid( this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            this.ircConfig.Channels.Add( null );
            this.CheckNotValid( this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            this.ircConfig.Channels.Clear();
            this.CheckNotValid( this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();
        }

        [Test]
        public void ValidatePortTest()
        {
            this.ircConfig.Port = -1;
            this.CheckNotValid( this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();
        }

        [Test]
        public void ValidateUserNameTest()
        {
            this.ircConfig.UserName = string.Empty;
            this.CheckNotValid( this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();
        }

        [Test]
        public void ValidateNickTest()
        {
            this.ircConfig.Nick = string.Empty;
            this.CheckNotValid( this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();
        }

        [Test]
        public void ValidateRealNameTest()
        {
            this.ircConfig.RealName = string.Empty;
            this.CheckNotValid( this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();
        }

        [Test]
        public void ValidateServerPasswordTest()
        {
            // Empty password should validate
            this.ircConfig.ServerPassword = string.Empty;
            Assert.DoesNotThrow( () => this.ircConfig.Validate() );
        }

        [Test]
        public void ValidateNickServPasswordTest()
        {
            // Empty password should validate
            this.ircConfig.NickServPassword = string.Empty;
            Assert.DoesNotThrow( () => this.ircConfig.Validate() );
        }

        [Test]
        public void ValdiateNickServNickTest()
        {
            // Empty password and empty nick should not matter.
            this.ircConfig.NickServPassword = string.Empty;
            this.ircConfig.NickServNick = string.Empty;
            Assert.DoesNotThrow( () => this.ircConfig.Validate() );

            this.ircConfig.NickServPassword = string.Empty;
            this.ircConfig.NickServNick = null;
            Assert.DoesNotThrow( () => this.ircConfig.Validate() );

            // A nick with no password is also valid, we just ignore the nick.
            this.ircConfig.NickServPassword = string.Empty;
            this.ircConfig.NickServNick = "something";
            Assert.DoesNotThrow( () => this.ircConfig.Validate() );

            // Now, if we specify a password, the nick can't be empty.
            this.ircConfig.NickServPassword = "passwd";
            this.ircConfig.NickServNick = string.Empty;
            this.CheckNotValid( this.ircConfig );

            //... or null
            this.ircConfig.NickServPassword = "passwd";
            this.ircConfig.NickServNick = string.Empty;
            this.CheckNotValid( this.ircConfig );

            // ...nor whitespace
            this.ircConfig.NickServPassword = "passwd";
            this.ircConfig.NickServNick = "    ";
            this.CheckNotValid( this.ircConfig );

            // Both specified is good!
            this.ircConfig.NickServPassword = "passwd";
            this.ircConfig.NickServNick = "NickServ";
            Assert.DoesNotThrow( () => this.ircConfig.Validate() );
        }

        [Test]
        public void ValidateNickServMessageTest()
        {
            // Empty password and empty message should not matter.
            this.ircConfig.NickServPassword = string.Empty;
            this.ircConfig.NickServMessage = string.Empty;
            Assert.DoesNotThrow( () => this.ircConfig.Validate() );

            this.ircConfig.NickServPassword = string.Empty;
            this.ircConfig.NickServMessage = null;
            Assert.DoesNotThrow( () => this.ircConfig.Validate() );

            // A message with no password is also valid, we just ignore the message.
            this.ircConfig.NickServPassword = string.Empty;
            this.ircConfig.NickServMessage = "something {%password%}";
            Assert.DoesNotThrow( () => this.ircConfig.Validate() );

            // Now, if we specify a password, the nick can't be empty.
            this.ircConfig.NickServPassword = "passwd";
            this.ircConfig.NickServMessage = string.Empty;
            this.CheckNotValid( this.ircConfig );

            //... or null
            this.ircConfig.NickServPassword = "passwd";
            this.ircConfig.NickServMessage = string.Empty;
            this.CheckNotValid( this.ircConfig );

            // ...nor whitespace
            this.ircConfig.NickServPassword = "passwd";
            this.ircConfig.NickServMessage = "    ";
            this.CheckNotValid( this.ircConfig );

            // Must contain "{%password%}"
            this.ircConfig.NickServPassword = "passwd";
            this.ircConfig.NickServMessage = "IDENTIFY something";

            // Both specified is good!
            this.ircConfig.NickServPassword = "passwd";
            this.ircConfig.NickServMessage = "IDENTIFY {%password%}";
            Assert.DoesNotThrow( () => this.ircConfig.Validate() );
        }

        [Test]
        public void ValidateBridgeBotsTest()
        {
            // Empty bridge bots config should be valid.
            this.ircConfig.BridgeBots.Clear();
            Assert.DoesNotThrow( () => this.ircConfig.Validate() );

            // No bridge message
            this.ircConfig.BridgeBots["telegrambot"] = @"(?<bridgeUser>\w+)-\s+(?<bridgeMsg>.+)";
            this.CheckNotValid( this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            // No bridge user
            this.ircConfig.BridgeBots["telegrambot"] = @"(?<bridgeUsr>\w+)-\s+(?<bridgeMessage>.+)";
            this.CheckNotValid( this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            // Empty string.
            this.ircConfig.BridgeBots["telegrambot"] = string.Empty;
            this.CheckNotValid( this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            // null string.
            this.ircConfig.BridgeBots["telegrambot"] = null;
            this.CheckNotValid( this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            // Empty key
            this.ircConfig.BridgeBots[string.Empty] = @"(?<bridgeUser>\w+)-\s+(?<bridgeMessage>.+)";
            this.CheckNotValid( this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();
        }

        [Test]
        public void ValidateAdminTest()
        {
            // Empty Admin
            this.ircConfig.Admins.Add( string.Empty );
            this.CheckNotValid( this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            // Null Admin
            this.ircConfig.Admins.Add( null );
            this.CheckNotValid( this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();
        }

        [Test]
        public void ValidateQuitMessageTest()
        {
            // 160 characters on quit message is okay.
            this.ircConfig.QuitMessage = "1234567891123456789212345678931234567894123456789512345678961234567897123456789812345678991234567890123456789112345678921234567893123456789412345678951234567896";
            Assert.DoesNotThrow( () => this.ircConfig.Validate() );

            // Quit message more than 160 characters:
            this.ircConfig.QuitMessage = "12345678911234567892123456789312345678941234567895123456789612345678971234567898123456789912345678901234567891123456789212345678931234567894123456789512345678961";
            this.CheckNotValid( this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            // Quit message can not be null.
            this.ircConfig.QuitMessage = null;
            this.CheckNotValid( this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();
        }

        [Test]
        public void ValidateRateLimitTest()
        {
            // Negative rate limit.
            this.ircConfig.RateLimit = new TimeSpan( -1 );
            this.CheckNotValid( this.ircConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();
        }

        // -------- Test Helpers --------

        /// <summary>
        /// Ensures the three different types of IRC Configs are not equal.
        /// </summary>
        /// <param name="ircConfig">The IRC config object to test.</param>
        /// <param name="interfaceConfig">The interfaced IRC object to test.</param>
        /// <param name="roIrcConfig">The read-only IRC object to test.</param>
        private void CheckNotEqual( IReadOnlyIrcConfig interfaceConfig, IrcConfig ircConfig )
        {
            // Ensure not equals works going from real -> interface
            Assert.AreNotEqual( interfaceConfig, ircConfig );
            Assert.AreNotEqual( ircConfig, interfaceConfig );

            // Ensure not equals works going from interface -> interface
            IReadOnlyIrcConfig iircConfig = interfaceConfig;
            Assert.AreNotEqual( iircConfig, ircConfig );
            Assert.AreNotEqual( ircConfig, iircConfig );
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