﻿//
//          Copyright Seth Hendrick 2016-2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Text.RegularExpressions;
using SethCS.Exceptions;

namespace ChaskisCore
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
                @"^:(?<nick>\S+)!~(?<user>.+)\s+" + IrcCommand + @"\s+(?<channel>#?\w+)",
                RegexOptions.Compiled
            );

        // -------- Constructor --------

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="response">The action to take when a user joins the channel</param>
        public JoinHandler( Action<IIrcWriter, IrcResponse> response )
        {
            ArgumentChecker.IsNotNull( response, nameof( response ) );

            this.JoinAction = response;
            this.KeepHandling = true;
        }

        // -------- Properties --------

        /// <summary>
        /// The action that gets triggered when a user joins.
        /// </summary>
        public Action<IIrcWriter, IrcResponse> JoinAction { get; private set; }

        /// <summary>
        /// Whether or not the handler should keep handling or not.
        /// Set to true to keep handling the event when it appears in the chat.
        /// Set to false so when the current IRC message is finished processing being,
        /// it leaves the event queue and never
        /// happens again.   Useful for events that only need to happen once.
        ///
        /// This is a public get/set.  Either classes outside of the handler can
        /// tell the handler to cancel the event, or it can cancel itself.
        ///
        /// Note: when this is set to false, there must be one more IRC message that appears
        /// before it is removed from the queue.
        ///
        /// Defaulted to true.
        /// </summary>
        public bool KeepHandling { get; set; }

        // -------- Functions --------

        /// <summary>
        /// Handles the event and sends the responses to the channel if desired.
        /// </summary>
        public void HandleEvent( HandlerArgs args )
        {
            ArgumentChecker.IsNotNull( args, nameof( args ) );

            Match match = pattern.Match( args.Line);
            if( match.Success )
            {
                string remoteUser = match.Groups["nick"].Value;

                // Don't fire if we were the ones to trigger the event.
                if( remoteUser.ToUpper() == args.IrcConfig.Nick.ToUpper() )
                {
                    return;
                }

                string channel = match.Groups["channel"].Value;
                if( args.BlackListedChannels.Contains( channel.ToLower() ) )
                {
                    // Blacklisted channel, return.
                    return;
                }

                IrcResponse response = new IrcResponse(
                    remoteUser,
                    channel,
                    string.Empty,
                    pattern,
                    match
                );

                this.JoinAction( args.IrcWriter, response );
            }
        }
    }
}