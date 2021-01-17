//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.IO;
using Chaskis.Core;
using NUnit.Framework;

namespace Chaskis.UnitTests.Common
{
    /// <summary>
    /// This class contains helper functions that will be useful
    /// for all unit tests.
    /// </summary>
    public static class TestHelpers
    {
        // ---------------- Fields ----------------

        /// <summary>
        /// Path to the base directory of the Tests directory.
        /// </summary>
        public static readonly string TestsBaseDir = Path.Combine(
            TestContext.CurrentContext.TestDirectory,
            "..",
            "..",
            ".."
        );

        public static readonly string ChaskisTestsDir = Path.Combine(
            TestsBaseDir,
            "ChaskisTests"
        );

        public static readonly string PluginTestsDir = Path.Combine(
            TestsBaseDir,
            "PluginTests"
        );

        public static readonly string CoreTestsDir = Path.Combine(
            TestsBaseDir,
            "CoreTests"
        );

        /// <summary>
        /// The path to the root of the project.
        /// </summary>
        public static readonly string ProjectRoot = Path.Combine(
            TestsBaseDir,
            ".."
        );

        /// <summary>
        /// The path to the plugins folder.
        /// </summary>
        public static readonly string PluginDir = Path.Combine(
            ProjectRoot,
            "Plugins"
        );

        /// <summary>
        /// A bridge bot that is in the channel.
        /// </summary>
        public const string BridgeBotUser = "telegrambot";

        /// <summary>
        /// Strange names to test.
        /// </summary>
        public static IReadOnlyList<string> StrangeNames = new List<string>()
        {
            "xforever1313|m",
            "xforever1313[m]",
            "|hello{m}",
            "Ihave-dashes-_-"
        }.AsReadOnly();

        /// <summary>
        /// Strange channel names to test.
        /// </summary>
        public static IReadOnlyList<string> StrangeChannels = new List<string>()
        {
            "#channel:1",
            "#channel-2",
            "#my-channel",
            "#my_channel"
        }.AsReadOnly();

        public static IReadOnlyList<string> FactoryPluginNames = new List<string>()
        {
            "plugin1",
            "plugin2",
            "plugin3"
        }.AsReadOnly();

        public static IReadOnlyList<string> PrefixTests = new List<string>()
        {
            ":anickname!ausername@blah.org", // Nick, User, Host
            ":anickname!~ausername@192.168.2.1", // Nick, User with ~, Host
            ":anickname", // Just a nickname.
            ":anickname@blah.org" // A nickname with a Host.
        }.AsReadOnly();

        private static InterPluginEventFactory factoryInstance;

        // ---------------- Functions ----------------

        public static IrcConfig GetTestIrcConfig()
        {
            IrcConfig ircConfig = new IrcConfig();
            ircConfig.Server = "AServer";
            ircConfig.Channels.Add( "#achannel" );
            ircConfig.Port = 1234;
            ircConfig.UseSsl = false;
            ircConfig.UserName = "SomeUserName";
            ircConfig.Nick = "SomeNick";
            ircConfig.RealName = "Some Real Name";
            ircConfig.ServerPassword = "ServerPassword";
            ircConfig.NickServPassword = "Password";
            ircConfig.NickServNick = "NickServ";
            ircConfig.NickServMessage = "IDENTIFY {%password%}";
            ircConfig.BridgeBots[BridgeBotUser] = @"(?<bridgeUser>\w+):\s+(?<bridgeMessage>.+)";
            ircConfig.Admins.Add( "person1" );
            ircConfig.Admins.Add( "person2" );
            ircConfig.RateLimit = TimeSpan.FromMilliseconds( 800 );

            return ircConfig;
        }

        /// <summary>
        /// Constructs an IRC string sent from a server..
        /// </summary>
        /// <param name="remoteUser">Nickname of the user who sent the message</param>
        /// <param name="type">The message type (e.g. JOIN, PART, PRIVMSG)</param>
        /// <param name="channel">The irc channel used</param>
        /// <param name="message">The message... if any</param>
        /// <returns>The constructed IRC message sent from the server.</returns>
        public static string ConstructIrcString(
            string remoteUser,
            string type,
            string channel,
            string message
        )
        {
            string msg = ":" + remoteUser + "!" + remoteUser + "@10.0.0.1 " + type + " " + channel;
            if( string.IsNullOrEmpty( message ) == false )
            {
                msg += " :" + message;
            }

            return msg;
        }

        public static string ConstructKickString(
            string remotedUser,
            string kickedUser,
            string channel,
            string reason = null
        )
        {
            string msg = string.Format(
                ":{0}!{0}@10.0.0.1 " + KickHandler.IrcCommand + " {1} {2}",
                remotedUser,
                channel,
                kickedUser
            );

            if( string.IsNullOrEmpty( reason ) == false )
            {
                return msg + " :" + reason;
            }
            else
            {
                return msg;
            }
        }

        /// <summary>
        /// Constructs an IRC message sent from a server.
        /// </summary>
        /// <param name="remoteUser">Nickname of the user who sent the message</param>
        /// <param name="channel">The irc channel used</param>
        /// <param name="message">The message</param>
        /// <returns>The constructed IRC message sent from the server.</returns>
        public static string ConstructMessageString(
            string remoteUser,
            string channel,
            string message
        )
        {
            return ConstructIrcString( remoteUser, "PRIVMSG", channel, message );
        }

        /// <summary>
        /// Constructs an ACTION IRC message sent from a server.
        /// </summary>
        /// <param name="remoteUser">Nickname of the user who sent the message</param>
        /// <param name="channel">The irc channel used</param>
        /// <param name="message">The message</param>
        /// <returns>The constructed ACTION IRC message sent from the server.</returns>
        public static string ConstructActionString(
            string remoteUser,
            string channel,
            string message
        )
        {
            return ConstructIrcString( remoteUser, "PRIVMSG", channel, "\u0001ACTION " + message + "\u0001" );
        }

        /// <summary>
        /// Constructs a CTCP Ping message sent from a server.
        /// </summary>
        /// <param name="remoteUser">Nickname of the user who sent the message</param>
        /// <param name="channel">The irc channel used</param>
        /// <param name="message">The message</param>
        /// <returns>The constructed CTCP Ping IRC message sent from the server.</returns>
        public static string ConstructCtcpPingString(
            string remoteUser,
            string channel,
            string message
        )
        {
            return ConstructIrcString( remoteUser, "PRIVMSG", channel, "\u0001PING " + message + "\u0001" );
        }

        /// <summary>
        /// Constructs a CTCP Version message sent from a server.
        /// </summary>
        /// <param name="remoteUser">Nickname of the user who sent the message</param>
        /// <param name="channel">The irc channel used</param>
        /// <param name="message">The message, if any.  Set to null for no message, just VERSION returns.</param>
        /// <returns>The constructed CTCP Version IRC message sent from the server.</returns>
        public static string ConstructCtcpVersionString(
            string remoteUser,
            string channel,
            string message = null
        )
        {
            message = ( " " + message ) ?? string.Empty;

            return ConstructIrcString( remoteUser, "PRIVMSG", channel, "\u0001VERSION" + message + "\u0001" );
        }

        /// <summary>
        /// Constructs a PONG String from the server.
        /// </summary>
        /// <param name="server">The server URL</param>
        /// <param name="msg">The message that we ponged back.</param>
        /// <returns>A PONG response from the server.</returns>
        public static string ConstringPongString(
            string server,
            string msg
        )
        {
            return ":" + server + " PONG " + server + " :" + msg;
        }

        /// <summary>
        /// Constructs an IRC PING sent from a server.
        /// </summary>
        /// <param name="response">What the bot must respond.</param>
        /// <returns>The ping string.</returns>
        public static string ConstructPingString( string response )
        {
            return "PING :" + response;
        }

        public static InterPluginEventFactory CreateEventFactory()
        {
            if( factoryInstance == null )
            {
                factoryInstance = InterPluginEventFactory.CreateInstance( new List<string>( FactoryPluginNames ) );
            }

            return factoryInstance;
        }

        public static void EqualsTest<T>( T obj1, T obj2 )
        {
            Assert.AreEqual( obj1, obj2 );
            Assert.AreEqual( obj2, obj1 );
            Assert.AreEqual( obj1.GetHashCode(), obj2.GetHashCode() );
        }

        public static void NotEqualsTest<T>( T obj1, T obj2 )
        {
            Assert.AreNotEqual( obj1, obj2 );
            Assert.AreNotEqual( obj2, obj1 );
            Assert.AreNotEqual( obj1.GetHashCode(), obj2.GetHashCode() );
        }

        public static void CloneTest<T>( T obj1, T obj2 )
        {
            EqualsTest( obj1, obj2 );
            Assert.AreNotSame( obj1, obj2 );
        }
    }
}