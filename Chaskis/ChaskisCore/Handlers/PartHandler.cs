//
//          Copyright Seth Hendrick 2016-2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Text.RegularExpressions;
using SethCS.Exceptions;

namespace Chaskis.Core
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
        public static readonly string IrcCommand = "PART";

        // :nickName!~nick@10.0.0.1 PART #testchan

        /// <summary>
        /// The pattern the search for when a line comes in.
        /// </summary>
        private static readonly Regex pattern =
            new Regex(
                Regexes.IrcMessagePrefix + @"\s+" + IrcCommand + @"\s+(?<channel>\S+)",
                RegexOptions.Compiled | RegexOptions.ExplicitCapture
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
            this.KeepHandling = true;
        }

        // -------- Properties --------

        /// <summary>
        /// The action that gets triggered when a user Parts from the channel.
        /// </summary>
        public Action<IIrcWriter, IrcResponse> PartAction { get; private set; }

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

            Match match = pattern.Match( args.Line );
            if( match.Success )
            {
                string remoteUser = match.Groups["nickOrServer"].Value;

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

                this.PartAction( args.IrcWriter, response );
            }
        }
    }
}