//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Text.RegularExpressions;
using SethCS.Exceptions;

namespace Chaskis.Core
{
    public delegate void PartHandlerAction( PartHandlerArgs args );

    /// <summary>
    /// Handles when a user parts.  That is, leaves the channel and logs off.
    /// </summary>
    public sealed class PartHandler : BaseIrcHandler
    {
        // ---------------- Fields ----------------

        /// <summary>
        /// The irc command that will appear from the server.
        /// </summary>
        public static readonly string IrcCommand = "PART";

        // :nickName!~nick@10.0.0.1 PART #channel :optional reason

        /// <summary>
        /// The pattern the search for when a line comes in.
        /// </summary>
        private static readonly Regex pattern =
            new Regex(
                Regexes.IrcMessagePrefix + @"\s+" + IrcCommand + @"\s+(?<channel>\S+)\s*(:(?<reason>.+))?",
                RegexOptions.Compiled | RegexOptions.ExplicitCapture
            );

        private readonly PartHandlerConfig config;

        // ---------------- Constructor ----------------

        /// <summary>
        /// Constructor
        /// </summary>
        public PartHandler( PartHandlerConfig config )
        {
            ArgumentChecker.IsNotNull( config, nameof( config ) );

            config.Validate();

            this.config = config.Clone();
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// The action that gets triggered when a user Parts from the channel.
        /// </summary>
        public PartHandlerAction PartAction
        {
            get
            {
                return this.config.PartAction;
            }
        }

        // ---------------- Functions ----------------

        /// <summary>
        /// Handles the event and sends the responses to the channel if desired.
        /// </summary>
        public override void HandleEvent( HandlerArgs args )
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
                if( args.IsChannelBlackListed( channel ) )
                {
                    // Blacklisted channel, return.
                    return;
                }

                string reason = match.Groups["reason"].Value;

                PartHandlerArgs partArgs = new PartHandlerArgs(
                    args.IrcWriter,
                    remoteUser,
                    channel,
                    reason,
                    pattern,
                    match
                );

                this.PartAction( partArgs );
            }
        }
    }
}