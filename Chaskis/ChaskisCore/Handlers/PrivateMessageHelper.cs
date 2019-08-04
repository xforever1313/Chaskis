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
using System.Text.RegularExpressions;
using SethCS.Exceptions;

namespace Chaskis.Core
{
    /// <summary>
    /// This class provides common functionality for ALL PRIVMSG related handlers.
    /// </summary>
    public class PrivateMessageHelper
    {
        // ---------------- Fields ----------------

        /// <summary>
        /// Last time this event was triggered in the channel.
        /// Key is the channel
        /// Value is the timestamp of the last event.
        /// </summary>
        private readonly Dictionary<string, DateTime> lastEvent;

        private readonly IPrivateMessageConfig config;

        /// <summary>
        /// The irc command that will appear from the server.
        /// </summary>
        public static readonly string IrcCommand = "PRIVMSG";

        // :nickName!~nick@10.0.0.1 PRIVMSG #TestChan :!bot help
        /// <summary>
        /// The pattern to search for when a line comes in.
        /// </summary>
        private readonly Regex pattern;
        // --------------- Constructor ---------------

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="pattern">
        /// The PRIVMSG pattern to look for.
        /// Must contain the following groups:
        /// - channel
        /// - theIrcMessage
        /// - nickOrServer
        /// </param>
        public PrivateMessageHelper( IPrivateMessageConfig config, Regex pattern )
        {
            this.config = config;
            this.lastEvent = new Dictionary<string, DateTime>();
            this.pattern = pattern;
        }

        // --------------- Properties ---------------

        // --------------- Functions ---------------

        /// <summary>
        /// This will parse the handler args into a <see cref="PrivateMessageParseResult"/>.
        /// <see cref="PrivateMessageParseResult.Success"/> will return false if:
        /// - Unable to parse
        /// - The channel is in the blacklisted channel
        /// </summary>
        public PrivateMessageParseResult ParseAndCheckHandlerArgs( HandlerArgs args )
        {
            ArgumentChecker.IsNotNull( args, nameof( args ) );

            Match match = pattern.Match( args.Line );
            if ( match.Success )
            {
                string channel = match.Groups["channel"].Value;
                if ( args.BlackListedChannels.Contains( channel ) )
                {
                    return new PrivateMessageParseResult();
                }

                // Otherwise, success!
                return new PrivateMessageParseResult
                {
                    Success = true,
                    RemoteUser = match.Groups["nickOrServer"].Value,
                    Channel = channel,
                    Message = match.Groups["theIrcMessage"].Value
                };
            }
            else
            {
                return new PrivateMessageParseResult();
            }
        }

        /// <summary>
        /// Parses the args and determines if a message should go out.
        /// </summary>
        public PrivateMessageResult ShouldSend( HandlerArgs args )
        {
            PrivateMessageParseResult parseResult = this.ParseAndCheckHandlerArgs( args );
            return this.ShouldSend( args, parseResult );
        }

        /// <summary>
        /// Takes in an already parsed message and does more checks to see if it should go out.
        /// This will not go out if:
        /// - <see cref="ResponseOptions.PmsOnly"/> is set, but it wasn't a PM.
        /// - <see cref="ResponseOptions.ChannelOnly"/> is set, but it was a PM.
        /// - The cooldown has not occurred yet.
        /// - The bot would respond to itself, but <see cref="IPrivateMessageConfig.RespondToSelf"/> is set to false.
        /// </summary>
        /// <param name="parseResult">
        /// The parsed result.
        /// This will be returned (and possibly modified) within the returned object.
        /// </param>
        public PrivateMessageResult ShouldSend( HandlerArgs args, PrivateMessageParseResult parseResult )
        {
            if ( parseResult.Success == false )
            {
                return new PrivateMessageResult( false, parseResult );
            }

            Regex lineRegex = new Regex(
                Parsing.LiquefyStringWithIrcConfig(
                    this.config.LineRegex,
                    parseResult.RemoteUser,
                    args.IrcConfig.Nick,
                    parseResult.Channel
                ),
                this.config.RegexOptions
            );

            Match messageMatch = lineRegex.Match( parseResult.Message );
            if ( messageMatch.Success == false )
            {
                return new PrivateMessageResult( false, parseResult );
            }

            string channelLowered = parseResult.Channel.ToLower();

            // Return right away if the nick name from the remote user is our own.
            if (
                ( this.config.RespondToSelf == false ) &&
                string.Equals( parseResult.RemoteUser, args.IrcConfig.Nick, StringComparison.InvariantCultureIgnoreCase )
            )
            {
                return new PrivateMessageResult( false, parseResult );
            }
            // Return right away if we only wish to respond on the channel we are listening on (ignore PMs).
            else if (
                ( this.config.ResponseOption == ResponseOptions.ChannelOnly ) &&
                ( args.IrcConfig.Channels.Any( c => c.ToLower( CultureInfo.InvariantCulture ) == channelLowered ) == false )
            )
            {
                return new PrivateMessageResult( false, parseResult );
            }
            // Return right away if we only wish to respond to Private Messages (the channel will be our nick name).
            else if (
                ( this.config.ResponseOption == ResponseOptions.PmsOnly ) &&
                ( string.Equals( parseResult.Channel, args.IrcConfig.Nick, StringComparison.InvariantCultureIgnoreCase ) == false )
            )
            {
                return new PrivateMessageResult( false, parseResult );
            }

            // If we haven't done a cool-down check yet, stick the minimum value in the dictionary
            // to ensure we will fire if its the first time.
            if ( lastEvent.ContainsKey( channelLowered ) == false )
            {
                lastEvent[channelLowered] = DateTime.MinValue;
            }

            DateTime currentTime = DateTime.UtcNow;
            TimeSpan timeSpan = currentTime - this.lastEvent[channelLowered];

            // Only fire if our cooldown was long enough. Cooldown of zero means always fire.
            // Need to explictly say Cooldown == 0 since DateTime.UtcNow has a innacurracy of +/- 15ms.  Therefore,
            // if the action happens too quickly, it can incorrectly not be triggered.
            if ( ( this.config.CoolDown == 0 ) || ( timeSpan.TotalSeconds > this.config.CoolDown ) )
            {
                // Sucess! Return a successful parse
                this.lastEvent[channelLowered] = currentTime;

                // If our response is a PM (channel name matches our bot's)
                // we need to change our channel to the remote user's
                // channel so it gets sent out correctly when handle event is called.
                if ( string.Equals( parseResult.Channel, args.IrcConfig.Nick, StringComparison.InvariantCultureIgnoreCase ) )
                {
                    parseResult.Channel = parseResult.RemoteUser;
                }

                return new PrivateMessageResult( true, parseResult )
                {
                    Regex = lineRegex,
                    Match = messageMatch
                };
            }
            else
            {
                // Otherwise, return a failure parse.
                return new PrivateMessageResult( false, parseResult );
            }
        }
    }

    /// <summary>
    /// After processing the PRIVMSG, this is the result of whether
    /// the message should be sent, or if there were problems parsing.
    /// </summary>
    public class PrivateMessageParseResult
    {
        // ---------------- Constructor ----------------

        public PrivateMessageParseResult()
        {
            this.Success = false;
            this.Channel = null;
            this.Message = null;
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// Should the message be sent?  This is set to false
        /// if things were not able to be parsed or if 
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// The remote user we got the message from.
        /// </summary>
        public string RemoteUser { get; set; }

        /// <summary>
        /// The channel to send the message to.  Null if <see cref="Success"/>
        /// is false.
        /// </summary>
        public string Channel { get; set; }

        /// <summary>
        /// The message to send.  Null if <see cref="Success"/> is false.
        /// </summary>
        public string Message { get; set; }
    }

    public class PrivateMessageResult
    {
        // ---------------- Constructor ----------------

        /// <summary>
        /// 
        /// </summary>
        public PrivateMessageResult( bool success, PrivateMessageParseResult parseResult )
        {
            this.ParseResult = parseResult;
            this.ShouldSend = success && this.ParseResult.Success;
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// Should the message be sent?
        /// </summary>
        public bool ShouldSend { get; private set; }

        /// <summary>
        /// Result of parsing.
        /// </summary>
        public PrivateMessageParseResult ParseResult{ get; private set; }

        /// <summary>
        /// The regex that was used to determine if we should send a message or not.
        /// </summary>
        public Regex Regex { get; set; }

        /// <summary>
        /// The regex match that was used to determine if we should send a message or not.
        /// </summary>
        public Match Match { get; set; }
    }
}
