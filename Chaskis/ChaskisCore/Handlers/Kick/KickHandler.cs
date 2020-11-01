//
//          Copyright Seth Hendrick 2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Text.RegularExpressions;
using SethCS.Exceptions;

namespace Chaskis.Core
{
    public delegate void KickHandlerAction( KickHandlerArgs args );

    public sealed class KickHandler : IIrcHandler
    {
        // ---------------- Fields ----------------

        /// <summary>
        /// The irc command that will appear from the server.
        /// </summary>
        public static readonly string IrcCommand = "KICK";

        // :nickName!~nick@10.0.0.1 KICK #channel badUser :reason

        private static readonly Regex pattern = new Regex(
            Regexes.IrcMessagePrefix + @"\s+" + IrcCommand + @"\s+(?<channel>\S+)\s+(?<kickedUser>\S+)\s*(:(?<reason>.+))?",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture
        );

        private readonly KickHandlerConfig config;

        // ---------------- Constructor ----------------

        public KickHandler( KickHandlerConfig config )
        {
            ArgumentChecker.IsNotNull( config, nameof( config ) );

            config.Validate();

            this.config = config.Clone();

            this.KeepHandling = true;
        }

        // ---------------- Properties ----------------

        public KickHandlerAction KickAction
        {
            get
            {
                return this.config.KickAction;
            }
        }

        /// <summary>
        /// Does the bot respond if a message appears where it did the kicking?
        /// </summary>
        public bool RespondToSelfPerformingKick
        {
            get
            {
                return this.config.RespondToSelfPerformingKick;
            }
        }

        /// <summary>
        /// Does the bot respond to itself being kicked from a channel?
        /// Defaulted to false.
        /// </summary>
        public bool RespondToSelfBeingKicked
        {
            get
            {
                return this.config.RespondToSelfBeingKicked;
            }
        }

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

        // ---------------- Functions ----------------

        public void HandleEvent( HandlerArgs args )
        {
            ArgumentChecker.IsNotNull( args, nameof( args ) );

            Match match = pattern.Match( args.Line );

            if( match.Success )
            {
                string remoteUser = match.Groups["nickOrServer"].Value; // The user who sent the kick command.
                string channel = match.Groups["channel"].Value;
                string kickedUser = match.Groups["kickedUser"].Value;
                string reason = match.Groups["reason"].Value;

                if( args.BlackListedChannels.Contains( channel.ToLower() ) )
                {
                    // Blacklist channel, return.
                    return;
                }

                if(
                    ( this.RespondToSelfPerformingKick == false ) &&
                    remoteUser.Equals( args.IrcConfig.Nick, StringComparison.InvariantCultureIgnoreCase )
                )
                {
                    return;
                }

                // If the kicked user was us, do nothing... if respond to self is false.
                if(
                    ( this.RespondToSelfBeingKicked == false ) &&
                    kickedUser.Equals( args.IrcConfig.Nick, StringComparison.InvariantCultureIgnoreCase )
                )
                {
                    return;
                }

                KickHandlerArgs kickArgs = new KickHandlerArgs(
                    args.IrcWriter,
                    remoteUser,
                    channel,
                    kickedUser,
                    reason,
                    pattern,
                    match
                );

                this.KickAction( kickArgs );
            }
        }
    }
}
