//
//          Copyright Seth Hendrick 2016-2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using ChaskisCore;
using NUnit.Framework;

namespace Tests
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

        private static ChaskisEventFactory factoryInstance;

        // ---------------- Functions ----------------

        public static IrcConfig GetTestIrcConfig()
        {
            IrcConfig ircConfig = new IrcConfig();
            ircConfig.Server = "AServer";
            ircConfig.Channels.Add( "#achannel" );
            ircConfig.Port = 1234;
            ircConfig.UserName = "SomeUserName";
            ircConfig.Nick = "SomeNick";
            ircConfig.RealName = "Some Real Name";
            ircConfig.Password = "Password";
            ircConfig.BridgeBots[BridgeBotUser] = @"(?<bridgeUser>\w+):\s+(?<bridgeMessage>.+)";
            ircConfig.Admins.Add( "person1" );
            ircConfig.Admins.Add( "person2" );
            ircConfig.RateLimit = 800;

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

        /// <summary>
        /// Constructs an IRC message sent from a server..
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

        public static ChaskisEventFactory CreateEventFactory()
        {
            if( factoryInstance == null )
            {
                factoryInstance = ChaskisEventFactory.CreateInstance( new List<string>( FactoryPluginNames ) );
            }

            return factoryInstance;
        }
    }
}