//
//          Copyright Seth Hendrick 2016-2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using SethCS.Exceptions;

namespace Chaskis.Core
{
    public delegate void MessageHandlerAction( MessageHandlerArgs response );

    /// <summary>
    /// Configuration for responding to a message received from IRC.
    /// </summary>
    public class MessageHandler : IIrcHandler
    {
        // ---------------- Fields ----------------

        /// <summary>
        /// Last time this event was triggered in the channel.
        /// Key is the channel
        /// Value is the timestamp of the last event.
        /// </summary>
        private readonly Dictionary<string, DateTime> lastEvent;

        private readonly MessageHandlerConfig config;

        private readonly PrivateMessageHelper pmHelper;

        /// <summary>
        /// The irc command that will appear from the server.
        /// </summary>
        public static readonly string IrcCommand = "PRIVMSG";

        // :nickName!~nick@10.0.0.1 PRIVMSG #TestChan :!bot help
        /// <summary>
        /// The pattern to search for when a line comes in.
        /// </summary>
        private static readonly Regex pattern = new Regex(
            Regexes.IrcMessagePrefix + @"\s+" + IrcCommand + @"\s+(?<channel>\S+)\s+:(?<theIrcMessage>.+)",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture
        );

        private static readonly IEnumerable<Regex> otherPrivmsgRegexes;

        // ---------------- Constructor ----------------

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="lineRegex">
        /// The regex to search for to fire the action.
        /// For example, if you want !bot help to trigger the action, pass in "!bot\s+help"
        /// 
        /// This DOES get Liquified via <see cref="Parsing.LiquefyStringWithIrcConfig(string, string, string, string)'"/>
        /// </param>
        /// <param name="lineAction">The action to perform based on the line.</param>
        /// <param name="coolDown">How long to wait between firing the line action in seconds.  0 for no cooldown.</param>
        /// <param name="responseOption">Whether or not to respond to PMs, only channels, or both.</param>
        /// <param name="respondToSelf">Whether or not the bot should respond to lines sent out by itself. Defaulted to false.</param>
        public MessageHandler(
            MessageHandlerConfig config
        )
        {
            ArgumentChecker.IsNotNull( config, nameof( config ) );

            config.Validate();

            this.config = config.Clone();
            this.lastEvent = new Dictionary<string, DateTime>();
            this.KeepHandling = true;
            this.pmHelper = new PrivateMessageHelper( this.config, pattern );
        }

        static MessageHandler()
        {
            Assembly assembly = typeof( MessageHandler ).Assembly;
            IEnumerable<PrivateMessageAttribute> attrs = assembly.GetCustomAttributes<PrivateMessageAttribute>();

            List<Regex> otherMessageRegexesList = new List<Regex>();
            foreach ( PrivateMessageAttribute attr in attrs )
            {
                otherMessageRegexesList.Add( attr.MessageRegex );
            }

            otherPrivmsgRegexes = otherMessageRegexesList.AsReadOnly();
        }

        // ------------------------ Properties ------------------------

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
        public MessageHandlerAction LineAction
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

        // ------------------------ Function ------------------------

        /// <summary>
        /// Fires the action if the line regex matches.
        /// </summary>
        public void HandleEvent( HandlerArgs args )
        {
            ArgumentChecker.IsNotNull( args, nameof( args ) );

            PrivateMessageParseResult parseResult = this.pmHelper.ParseAndCheckHandlerArgs( args );
            if ( parseResult.Success == false )
            {
                return;
            }

            // If we are a bridge bot, we need to change
            // the nick and the channel
            foreach ( string bridgeBotRegex in args.IrcConfig.BridgeBots.Keys )
            {
                Match nameMatch = Regex.Match( parseResult.RemoteUser, bridgeBotRegex );
                if ( nameMatch.Success )
                {
                    Match bridgeBotMatch = Regex.Match( parseResult.Message, args.IrcConfig.BridgeBots[bridgeBotRegex] );

                    // If the regex matches, then we'll update the nick and message
                    // to be whatever came from the bridge.
                    if ( bridgeBotMatch.Success )
                    {
                        string newNick = bridgeBotMatch.Groups["bridgeUser"].Value;
                        string newMessage = bridgeBotMatch.Groups["bridgeMessage"].Value;

                        // Only change the nick anme and the message if the nick and the message aren't empty.
                        if ( ( string.IsNullOrEmpty( newNick ) == false ) && ( string.IsNullOrEmpty( newMessage ) == false ) )
                        {
                            parseResult.RemoteUser = newNick;
                            parseResult.Message = newMessage;
                        }

                        break;  // We have our message, break out of the loop.
                    }
                }
            }

            PrivateMessageResult pmResult = this.pmHelper.ShouldSend( args, parseResult );
            if ( pmResult.ShouldSend == false )
            {
                return;
            }

            MessageHandlerArgs response = new MessageHandlerArgs(
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