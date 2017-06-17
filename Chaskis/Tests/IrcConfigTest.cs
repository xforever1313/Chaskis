//
//          Copyright Seth Hendrick 2016-2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using ChaskisCore;
using NUnit.Framework;
using SethCS.Exceptions;

namespace Tests
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
            IIrcConfig interfaceIrcConfig = this.ircConfig.Clone();
            ReadOnlyIrcConfig roIrcConfig = new ReadOnlyIrcConfig( this.ircConfig.Clone() );

            // Ensure both the interface and the RO config are equal, but not the same reference.
            Assert.AreNotSame( interfaceIrcConfig, this.ircConfig );
            Assert.AreNotSame( roIrcConfig, this.ircConfig );
            Assert.AreNotSame( interfaceIrcConfig, roIrcConfig );

            // Ensure everything is equal.
            Assert.AreEqual( interfaceIrcConfig, this.ircConfig );
            Assert.AreEqual( this.ircConfig, interfaceIrcConfig );
            Assert.AreEqual( roIrcConfig, this.ircConfig );
            Assert.AreEqual( this.ircConfig, roIrcConfig );
            Assert.AreEqual( interfaceIrcConfig, roIrcConfig );
            Assert.AreEqual( roIrcConfig, interfaceIrcConfig );

            // BridgeBots should not be same reference.
            Assert.AreNotSame( interfaceIrcConfig.BridgeBots, this.ircConfig.BridgeBots );
            Assert.AreNotSame( roIrcConfig, this.ircConfig.BridgeBots );
            Assert.AreNotSame( interfaceIrcConfig.BridgeBots, roIrcConfig.BridgeBots );

            // Admins should not be same reference
            Assert.AreNotSame( interfaceIrcConfig.Admins, this.ircConfig.Admins );
            Assert.AreNotSame( roIrcConfig.Admins, this.ircConfig.Admins );
            Assert.AreNotSame( interfaceIrcConfig.Admins, roIrcConfig.Admins );

            // Next, start changing things.  Everything should become false.
            this.ircConfig.Server = "irc.somewhere.net";
            CheckNotEqual( this.ircConfig, interfaceIrcConfig, roIrcConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            this.ircConfig.Channels.Add( "#derp" );
            CheckNotEqual( this.ircConfig, interfaceIrcConfig, roIrcConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            this.ircConfig.Port = 9876;
            CheckNotEqual( this.ircConfig, interfaceIrcConfig, roIrcConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            this.ircConfig.UserName = "Nate";
            CheckNotEqual( this.ircConfig, interfaceIrcConfig, roIrcConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            this.ircConfig.Nick = "Nate";
            CheckNotEqual( this.ircConfig, interfaceIrcConfig, roIrcConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            this.ircConfig.RealName = "Nate A Saurus";
            CheckNotEqual( this.ircConfig, interfaceIrcConfig, roIrcConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            this.ircConfig.Password = "ABadPassword";
            CheckNotEqual( this.ircConfig, interfaceIrcConfig, roIrcConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            this.ircConfig.QuitMessage = "A quit message";
            CheckNotEqual( this.ircConfig, interfaceIrcConfig, roIrcConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            // Add an additional bot.
            this.ircConfig.BridgeBots["slackBridge"] = @"(?<bridgeUser>\w+):\s+(?<bridgeMessage>.+)";
            CheckNotEqual( this.ircConfig, interfaceIrcConfig, roIrcConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            // Change an existing bot's value.
            this.ircConfig.BridgeBots["telegrambot"] = @"(?<bridgeUser>\w+)-\s+(?<bridgeMessage>.+)";
            CheckNotEqual( this.ircConfig, interfaceIrcConfig, roIrcConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();

            // Add an addtional Admin
            this.ircConfig.Admins.Add( "person3" );
            CheckNotEqual( this.ircConfig, interfaceIrcConfig, roIrcConfig );
            this.ircConfig = TestHelpers.GetTestIrcConfig();
        }

        /// <summary>
        /// Ensures the Read-only irc config does its job.
        /// </summary>
        [Test]
        public void ReadOnlyTest()
        {
            IIrcConfig config = new ReadOnlyIrcConfig( ircConfig );

            // First, ensure properties are all equal
            Assert.AreEqual( this.ircConfig.Server, config.Server );
            Assert.AreEqual( this.ircConfig.Channels, config.Channels );
            Assert.AreEqual( this.ircConfig.Port, config.Port );
            Assert.AreEqual( this.ircConfig.UserName, config.UserName );
            Assert.AreEqual( this.ircConfig.Nick, config.Nick );
            Assert.AreEqual( this.ircConfig.RealName, config.RealName );
            Assert.AreEqual( this.ircConfig.Password, config.Password );
            Assert.AreEqual( this.ircConfig.QuitMessage, config.QuitMessage );
            Assert.AreNotSame( this.ircConfig.BridgeBots, config.BridgeBots ); // Should not be same reference.
            Assert.AreNotSame( this.ircConfig.Admins, config.Admins ); // Should not be same reference.

            // Next, ensure trying to convert to an IRCConfig results in a null (someone's going to do this).
            Assert.IsNull( config as IrcConfig );

            // Lastly, ensure setters throw exceptions.
            ReadOnlyIrcConfig roConfig = config as ReadOnlyIrcConfig;

            ReadOnlyException ex =
                Assert.Throws<ReadOnlyException>(
                    delegate ()
                    {
                        roConfig.Server = "NewServer";
                    }
                );
            Assert.IsTrue( ex.Message.Contains( "Server" ) );

            Assert.Throws<NotSupportedException>(
                delegate ()
                {
                    roConfig.Channels.Add( "#NewChannel" );
                }
            );

            ex = Assert.Throws<ReadOnlyException>(
                delegate ()
                {
                    roConfig.Port = 4;
                }
            );
            Assert.IsTrue( ex.Message.Contains( "Port" ) );

            ex = Assert.Throws<ReadOnlyException>(
                delegate ()
                {
                    roConfig.UserName = "NewName";
                }
            );
            Assert.IsTrue( ex.Message.Contains( "UserName" ) );

            ex = Assert.Throws<ReadOnlyException>(
                delegate ()
                {
                    roConfig.Nick = "NewNick";
                }
            );
            Assert.IsTrue( ex.Message.Contains( "Nick" ) );

            ex = Assert.Throws<ReadOnlyException>(
                delegate ()
                {
                    roConfig.RealName = "NewRName";
                }
            );
            Assert.IsTrue( ex.Message.Contains( "RealName" ) );

            ex = Assert.Throws<ReadOnlyException>(
                delegate ()
                {
                    roConfig.Password = "NewPass";
                }
            );
            Assert.IsTrue( ex.Message.Contains( "Password" ) );

            ex = Assert.Throws<ReadOnlyException>(
                delegate ()
                {
                    roConfig.QuitMessage = "I am quitting";
                }
            );
            Assert.IsTrue( ex.Message.Contains( "QuitMessage" ) );
        }

        [Test]
        public void ValidateTest()
        {
            // Ensure a valid config does not throw.
            Assert.DoesNotThrow( () => this.ircConfig.Validate() );
            Assert.DoesNotThrow( () => this.ircConfig.Clone().Validate() ); // Tests Interface
            Assert.DoesNotThrow( () => new ReadOnlyIrcConfig( this.ircConfig ).Validate() ); // Tests Read-only

            // Empty bridge bots config should be valid.
            this.ircConfig.BridgeBots.Clear();
            Assert.DoesNotThrow( () => this.ircConfig.Validate() );
            Assert.DoesNotThrow( () => this.ircConfig.Clone().Validate() ); // Tests Interface
            Assert.DoesNotThrow( () => new ReadOnlyIrcConfig( this.ircConfig ).Validate() ); // Tests Read-only

            // Empty password should validate
            this.ircConfig.Password = string.Empty;
            Assert.DoesNotThrow( () => this.ircConfig.Validate() );
            Assert.DoesNotThrow( () => this.ircConfig.Clone().Validate() ); // Tests Interface
            Assert.DoesNotThrow( () => new ReadOnlyIrcConfig( this.ircConfig ).Validate() ); // Tests Read-only

            // 160 characters on quit message is okay.
            this.ircConfig.QuitMessage = "1234567891123456789212345678931234567894123456789512345678961234567897123456789812345678991234567890123456789112345678921234567893123456789412345678951234567896";
            Assert.DoesNotThrow( () => this.ircConfig.Validate() );
            Assert.DoesNotThrow( () => this.ircConfig.Clone().Validate() ); // Tests Interface
            Assert.DoesNotThrow( () => new ReadOnlyIrcConfig( this.ircConfig ).Validate() ); // Tests Read-only

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
        }

        // -------- Test Helpers --------

        /// <summary>
        /// Ensures the three different types of IRC Configs are not equal.
        /// </summary>
        /// <param name="ircConfig">The IRC config object to test.</param>
        /// <param name="interfaceConfig">The interfaced IRC object to test.</param>
        /// <param name="roIrcConfig">The read-only IRC object to test.</param>
        public void CheckNotEqual( IrcConfig ircConfig, IIrcConfig interfaceConfig, ReadOnlyIrcConfig roIrcConfig )
        {
            // Ensure not equals works going from real -> interface
            Assert.AreNotEqual( ircConfig, interfaceConfig );
            Assert.AreNotEqual( interfaceConfig, ircConfig );

            // Ensure not equals works going from real -> readonly
            Assert.AreNotEqual( ircConfig, roIrcConfig );
            Assert.AreNotEqual( roIrcConfig, ircConfig );

            // Ensure not equals works going from interface -> interface
            IIrcConfig iircConfig = ircConfig;
            Assert.AreNotEqual( iircConfig, interfaceConfig );
            Assert.AreNotEqual( interfaceConfig, iircConfig );

            // Ensure not equals works going from readonly -> readonly
            ReadOnlyIrcConfig roConfig = new ReadOnlyIrcConfig( ircConfig );
            Assert.AreNotEqual( roConfig, roIrcConfig );
            Assert.AreNotEqual( roIrcConfig, roConfig );
        }

        /// <summary>
        /// Ensures the given IRCConfig is not valid.
        /// </summary>
        /// <param name="config">The config to check.</param>
        public void CheckNotValid( IrcConfig config )
        {
            Assert.Throws<ApplicationException>( () => config.Validate() );

            ReadOnlyIrcConfig roConfig = new ReadOnlyIrcConfig( config.Clone() );
            Assert.Throws<ApplicationException>( () => roConfig.Validate() );

            IIrcConfig iircConfig = config;
            Assert.Throws<ApplicationException>( () => iircConfig.Validate() );
        }
    }
}