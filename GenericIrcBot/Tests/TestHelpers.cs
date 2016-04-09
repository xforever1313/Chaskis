
//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using GenericIrcBot;

namespace Tests
{
    /// <summary>
    /// This class contains helper functions that will be useful
    /// for all unit tests.
    /// </summary>
    public static class TestHelpers
    {
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

            return ircConfig;
        }

        /// <summary>
        /// Constructs an IRC string sent from a server..
        /// </summary>
        /// <param name="nick">Nickname of the user who sent the message</param>
        /// <param name="type">The message type (e.g. JOIN, PART, PRIVMSG)</param>
        /// <param name="channel">The irc channel used</param>
        /// <param name="message">The message... if any</param>
        /// <returns>The constructed IRC message sent from the server.</returns>
        public static string ConstructIrcString(
            string nick,
            string type,
            string channel,
            string message
        )
        {
            string msg = ":" + nick + "!~" + nick + "@10.0.0.1 " + type + " " + channel;
            if( string.IsNullOrEmpty( message ) == false )
            {
                msg += " :" + message;
            }

            return msg;
        }
    }
}

