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
    public delegate void CtcpVersionHandlerAction( CtcpVersionHandlerArgs response );

    /// <summary>
    /// Handler for when another IRC uses sends out a CTCP Version Request to the bot.
    /// </summary>
    [PrivateMessage( MessageRegexPattern )]
    public class CtcpVersionHandler : IIrcHandler
    {
        // ---------------- Fields ----------------

        private readonly CtcpVersionHandlerConfig config;

        private readonly PrivateMessageHelper pmHelper;

        internal const string MessageRegexPattern = "\u0001VERSION\\s*(?<theIrcMessage>.*)\u0001";

        private static readonly Regex pattern = new Regex(
            Regexes.IrcMessagePrefix + @"\s+" + PrivateMessageHelper.IrcCommand + @"\s+(?<channel>\S+)\s+:" + MessageRegexPattern,
            RegexOptions.Compiled | RegexOptions.ExplicitCapture
        );

        // ---------------- Constructor ----------------

        public CtcpVersionHandler( CtcpVersionHandlerConfig config )
        {
            ArgumentChecker.IsNotNull( config, nameof( config ) );

            config.Validate();

            this.config = config.Clone();
            this.KeepHandling = true;
            this.pmHelper = new PrivateMessageHelper( this.config, pattern );
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// The regex to look in IRC Chat that triggers the line action.
        /// </summary>
        /// <remarks>
        /// By default, <see cref="BasePrivateMessageConfig{TChildType, TLineActionType, TLineActionArgs}.LineRegex"/>
        /// does not need to be set if we are just looking for VERSION.  VERSION does not usually have
        /// arguments after it.  However, if for some reason we expect arguments to come after VERSION,
        /// override <see cref="BasePrivateMessageConfig{TChildType, TLineActionType, TLineActionArgs}.LineRegex"/>.
        /// 
        /// If fact, by default, the regex is include anything after VERSION optionally.
        /// </remarks>
        public string LineRegex
        {
            get
            {
                return this.config.LineRegex;
            }
        }

        /// <summary>
        /// What regex options to use with <see cref="LineRegex"/>.
        /// </summary>
        public RegexOptions RegexOptions
        {
            get
            {
                return this.config.RegexOptions;
            }
        }

        /// <summary>
        /// The action that gets triggered when the line regex matches.
        /// </summary>
        public CtcpVersionHandlerAction LineAction
        {
            get
            {
                return this.config.LineAction;
            }
        }

        /// <summary>
        /// How long to wait in seconds between firing events. 0 for no cool down.
        /// This cool down is on a per-channel basis if the bot is in multiple channels.
        /// </summary>
        public int CoolDown
        {
            get
            {
                return this.config.CoolDown;
            }
        }

        /// <summary>
        /// Whether or not this bot will respond to private messages or not.
        /// </summary>
        public ResponseOptions ResponseOption
        {
            get
            {
                return this.config.ResponseOption;
            }
        }

        /// <summary>
        /// Whether or not the action will be triggered if the person
        /// who sent the message was this bot.
        /// </summary>
        public bool RespondToSelf
        {
            get
            {
                return this.config.RespondToSelf;
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

        // ---------------- Function ----------------

        public void HandleEvent( HandlerArgs args )
        {
            ArgumentChecker.IsNotNull( args, nameof( args ) );

            PrivateMessageResult pmResult = this.pmHelper.ShouldSend( args );
            if( pmResult.ShouldSend == false )
            {
                return;
            }

            CtcpVersionHandlerArgs response = new CtcpVersionHandlerArgs(
                args.IrcWriter,
                pmResult.ParseResult.RemoteUser,
                pmResult.ParseResult.Channel,
                pmResult.ParseResult.Message,
                pmResult.Regex,
                pmResult.Match
            );

            this.LineAction( response );
        }
    }
}
