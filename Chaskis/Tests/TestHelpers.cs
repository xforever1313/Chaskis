//
//          Copyright Seth Hendrick 2016-2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.IO;
using ChaskisCore;

namespace Tests
{
    /// <summary>
    /// This class contains helper functions that will be useful
    /// for all unit tests.
    /// </summary>
    public static class TestHelpers
    {
        /// <summary>
        /// Path to the base directory of the Tests directory.
        /// </summary>
        public static readonly string TestsBaseDir = Path.Combine(
            "..",
            ".."
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
        /// Path to the TestFiles directory of the Tests directory.
        /// </summary>
        public static readonly string TestFilesDir = Path.Combine(
            TestsBaseDir, "TestFiles"
        );

        public static IrcConfig GetTestIrcConfig()
        {
            IrcConfig ircConfig = new IrcConfig();
            ircConfig.Server = "AServer";
            ircConfig.Channel = "#AChannel";
            ircConfig.Port = 1234;
            ircConfig.UserName = "SomeUserName";
            ircConfig.Nick = "SomeNick";
            ircConfig.RealName = "Some Real Name";
            ircConfig.Password = "Password";
            ircConfig.BridgeBots[BridgeBotUser] = @"(?<bridgeUser>\w+):\s+(?<bridgeMessage>.+)";
            ircConfig.Admins.Add( "person1" );
            ircConfig.Admins.Add( "person2" );

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
            string msg = ":" + remoteUser + "!~" + remoteUser + "@10.0.0.1 " + type + " " + channel;
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
    }
}