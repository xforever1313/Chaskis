﻿
//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Text.RegularExpressions;
using SethCS.Exceptions;

namespace GenericIrcBot
{
    /// <summary>
    /// Handles when a user parts.  That is, leaves the channel and logs off.
    /// </summary>
    public class PartHandler : IIrcHandler
    {
        // -------- Fields --------

        /// <summary>
        /// The irc command that will appear from the server.
        /// </summary>
        public const string IrcCommand = "PART";

        // :nickName!~nick@10.0.0.1 PART #testchan

        /// <summary>
        /// The pattern the search for when a line comes in.
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
        public PartHandler( Action<IIrcWriter, IrcResponse> response )
        {
            ArgumentChecker.IsNotNull( response, nameof( response ) );

            this.PartAction = response;
        }

        // -------- Properties --------

        /// <summary>
        /// The action that gets triggered when a user Parts from the channel.
        /// </summary>
        public Action<IIrcWriter, IrcResponse> PartAction { get; private set; }

        // -------- Functions --------

        /// <summary>
        /// Handles the event and sends the responses to the channel if desired.
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

                this.PartAction( ircWriter, response );
            }
        }
    }
}
