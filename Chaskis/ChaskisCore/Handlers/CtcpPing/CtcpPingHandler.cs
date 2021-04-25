//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Text.RegularExpressions;
using SethCS.Exceptions;

namespace Chaskis.Core
{
    public delegate void CtcpPingHandlerAction( CtcpPingHandlerArgs response );

    [PrivateMessage( MessageRegexPattern )]
    public sealed class CtcpPingHandler : BaseIrcHandler
    {
        // ---------------- Fields ----------------

        private readonly CtcpPingHandlerConfig config;

        private readonly PrivateMessageHelper pmHelper;

        internal const string MessageRegexPattern = "\u0001PING (?<theIrcMessage>.+)\u0001";

        private static readonly Regex pattern = new Regex(
            Regexes.IrcMessagePrefix + @"\s+" + PrivateMessageHelper.IrcCommand + @"\s+(?<channel>\S+)\s+:" + MessageRegexPattern,
            RegexOptions.Compiled | RegexOptions.ExplicitCapture
        );

        // ---------------- Constructor ----------------

        public CtcpPingHandler( CtcpPingHandlerConfig config ) :
            base()
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
        public CtcpPingHandlerAction LineAction
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

        // ---------------- Function ----------------

        public override void HandleEvent( HandlerArgs args )
        {
            ArgumentChecker.IsNotNull( args, nameof( args ) );

            PrivateMessageResult pmResult = this.pmHelper.ShouldSend( args );
            if ( pmResult.ShouldSend == false )
            {
                return;
            }

            CtcpPingHandlerArgs response = new CtcpPingHandlerArgs(
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
