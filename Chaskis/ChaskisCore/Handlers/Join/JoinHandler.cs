//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Text.RegularExpressions;
using SethCS.Exceptions;

namespace Chaskis.Core
{
    public delegate void JoinHandlerAction( JoinHandlerArgs args );

    /// <summary>
    /// Handles the event where someone joins the watched channel.
    /// </summary>
    public sealed class JoinHandler : BaseIrcHandler
    {
        // -------- Fields --------

        /// <summary>
        /// The irc command that will appear from the server.
        /// </summary>
        public static readonly string IrcCommand = "JOIN";

        // :nickName!~nick@10.0.0.1 JOIN #testchan

        /// <summary>
        /// The pattern to search for when a line comes in.
        /// </summary>
        private static readonly Regex pattern =
            new Regex(
                Regexes.IrcMessagePrefix + @"\s+" + IrcCommand + @"\s+(?<channel>\S+)",
                RegexOptions.Compiled | RegexOptions.ExplicitCapture
            );

        private readonly JoinHandlerConfig config;

        // ---------------- Constructor ----------------

        /// <summary>
        /// Constructor
        /// </summary>
        public JoinHandler( JoinHandlerConfig config ) :
            base()
        {
            ArgumentChecker.IsNotNull( config, nameof( config ) );

            config.Validate();
            this.config = config.Clone();
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// The action that gets triggered when a user joins.
        /// </summary>
        public JoinHandlerAction JoinAction
        {
            get
            {
                return this.config.JoinAction;
            }
        }

        /// <summary>
        /// Does the bot respond to itself joining a channel?
        /// </summary>
        public bool RespondToSelf
        {
            get
            {
                return this.config.RespondToSelf;
            }
        }

        // -------- Functions --------

        /// <summary>
        /// Handles the event and sends the responses to the channel if desired.
        /// </summary>
        public override void HandleEvent( HandlerArgs args )
        {
            ArgumentChecker.IsNotNull( args, nameof( args ) );

            Match match = pattern.Match( args.Line);
            if( match.Success )
            {
                string remoteUser = match.Groups["nickOrServer"].Value;

                // Don't fire if we were the ones to trigger the event.
                if(
                    remoteUser.Equals( args.IrcConfig.Nick, StringComparison.InvariantCultureIgnoreCase ) &&
                    ( this.RespondToSelf == false )
                )
                {
                    return;
                }

                string channel = match.Groups["channel"].Value;
                if( args.IsChannelBlackListed( channel ) )
                {
                    // Blacklisted channel, return.
                    return;
                }

                JoinHandlerArgs joinArgs = new JoinHandlerArgs(
                    args.IrcWriter,
                    remoteUser,
                    channel,
                    pattern,
                    match
                );

                this.JoinAction( joinArgs );
            }
        }
    }
}