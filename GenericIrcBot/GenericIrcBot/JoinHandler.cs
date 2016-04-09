
//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Text.RegularExpressions;

namespace GenericIrcBot
{
    /// <summary>
    /// Handles the event where someone joins the watched channel.
    /// </summary>
    public class JoinHandler : IIrcHandler
    {
        // -------- Fields --------

        /// <summary>
        /// The irc command that will appear from the server.
        /// </summary>
        public const string IrcCommand = "JOIN";

        // :nickName!~nick@10.0.0.1 JOIN #testchan

        /// <summary>
        /// The pattern to search for when a line comes in.
        /// </summary>
        private static readonly Regex pattern = 
            new Regex(
                @"^:(?<nick>\w+)!~(?<user>.+)\s+" + IrcCommand + @"\s+(?<channel>#?\w+)",
                RegexOptions.Compiled
            );

        // -------- Constructor --------

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="response">The action to take when a user joins the channel</param>
        public JoinHandler( Action<IIrcWriter, IrcResponse> response )
        {
            if( response == null )
            {
                throw new ArgumentNullException( nameof( response ) );
            }

            this.JoinAction = response;
        }

        // -------- Properties --------

        /// <summary>
        /// The action that gets triggered when a user joins.
        /// </summary>
        public Action<IIrcWriter, IrcResponse> JoinAction { get; private set; }

        // -------- Functions --------

        /// <summary>
        /// Handles the event and sends the responses to the channel if desired.
        /// </summary>
        /// <param name="line">The RAW line from IRC to check.</param>
        /// <param name="ircConfig">The irc config to use when parsing this line.</param>
        /// <param name="ircWriter">The way to write to the irc channel.</param>
        public void HandleEvent( string line, IIrcConfig ircConfig, IIrcWriter ircWriter )
        {
            if( string.IsNullOrEmpty( line ) )
            {
                throw new ArgumentNullException( nameof( line ) );
            }
            else if( ircConfig == null )
            {
                throw new ArgumentNullException( nameof( ircConfig ) );
            }
            else if( ircWriter == null )
            {
                throw new ArgumentNullException( nameof( ircConfig ) );
            }

            Match match = pattern.Match( line );
            if( match.Success )
            {
                string remoteUser = match.Groups["nick"].Value;

                // Don't fire if we were the ones to trigger the event.
                if( remoteUser.ToUpper() == ircConfig.Nick.ToUpper() )
                {
                    return;
                }

                IrcResponse response = 
                    new IrcResponse(
                        remoteUser,
                        match.Groups["channel"].Value,
                        string.Empty
                    );

                this.JoinAction( ircWriter, response );
            }
        }
    }
}
