
//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System.Text.RegularExpressions;
using SethCS.Exceptions;

namespace GenericIrcBot
{
    /// <summary>
    /// Most servers will ping the bot using PING
    /// to make sure we are still there.
    /// We need to send a PONG back, otherwise
    /// the server will terminate our connection.
    /// </summary>
    public class PingHandler : IIrcHandler
    {
        // -------- Fields --------

        /// <summary>
        /// Pattern to watch for to send a pong.
        /// </summary>
        private static readonly Regex pattern = 
            new Regex(
                @"^PING\s*:(?<response>.+)",
                RegexOptions.Compiled
            );

        // -------- Constructor --------

        /// <summary>
        /// Constructs a ping handler.
        /// </summary>
        public PingHandler()
        {
        }

        // -------- Functions --------

        /// <summary>
        /// Handles the event and the pong back to the server.
        /// </summary>
        /// <param name="line">The RAW line from IRC to check.</param>
        /// <param name="ircConfig">The irc config to use when parsing this line.</param>
        /// <param name="ircWriter">The way to write to the irc channel.</param>
        public void HandleEvent( string line, IIrcConfig ircConfig, IIrcWriter ircWriter )
        {
            ArgumentChecker.StringIsNotNullOrEmpty( line, nameof( line ) );
            ArgumentChecker.IsNotNull( ircConfig, nameof( ircConfig ) );
            ArgumentChecker.IsNotNull( ircWriter, nameof( ircWriter ) );

            Match match = pattern.Match( line );
            if( match.Success )
            {
                ircWriter.SendPong( match.Groups["response"].Value );
            }
        }
    }
}

