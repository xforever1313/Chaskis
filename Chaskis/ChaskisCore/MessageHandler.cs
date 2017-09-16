//
//          Copyright Seth Hendrick 2016-2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using SethCS.Exceptions;

namespace ChaskisCore
{
    /// <summary>
    /// Configuration for responding to a message received from IRC.
    /// </summary>
    public class MessageHandler : IIrcHandler
    {
        // ---------------- Fields ----------------

        /// <summary>
        /// The irc command that will appear from the server.
        /// </summary>
        public const string IrcCommand = "PRIVMSG";

        /// <summary>
        /// Last time this event was triggered in the channel.
        /// Key is the channel
        /// Value is the timestamp of the last event.
        /// </summary>
        private Dictionary<string, DateTime> lastEvent;

        // :nickName!~nick@10.0.0.1 PRIVMSG #TestChan :!bot help
        /// <summary>
        /// The pattern to search for when a line comes in.
        /// </summary>
        private static readonly Regex pattern = new Regex(
            Regexes.IrcMessagePrefix + @"\s+" + IrcCommand + @"\s+(?<channel>\S+)\s+:(?<theIrcMessage>.+)",
            RegexOptions.Compiled | RegexOptions.ExplicitCapture
        );

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
            string lineRegex,
            Action<IIrcWriter, IrcResponse> lineAction,
            int coolDown = 0,
            ResponseOptions responseOption = ResponseOptions.ChannelAndPms,
            bool respondToSelf = false
        )
        {
            ArgumentChecker.StringIsNotNullOrEmpty( lineRegex, nameof( lineRegex ) );
            ArgumentChecker.IsNotNull( lineAction, nameof( lineAction ) );
            if( coolDown < 0 )
            {
                throw new ArgumentException( "cool down must be greater than zero", nameof( coolDown ) );
            }

            this.LineRegex = lineRegex;
            this.LineAction = lineAction;
            this.CoolDown = coolDown;
            this.RespondToSelf = respondToSelf;
            this.ResponseOption = responseOption;
            this.lastEvent = new Dictionary<string, DateTime>();
            this.KeepHandling = true;
        }

        // ------------------------ Properties ------------------------

        /// <summary>
        /// The regex to look in IRC Chat that triggers the line action.
        /// </summary>
        public string LineRegex { get; private set; }

        /// <summary>
        /// The action that gets triggered when the line regex matches.
        /// </summary>
        public Action<IIrcWriter, IrcResponse> LineAction { get; private set; }

        /// <summary>
        /// How long to wait in seconds between firing events. 0 for no cool down.
        /// This cool down is on a per-channel basis if the bot is in multiple channels.
        /// </summary>
        public int CoolDown { get; private set; }

        /// <summary>
        /// Whether or not this bot will respond to private messages or not.
        /// </summary>
        public ResponseOptions ResponseOption { get; private set; }

        /// <summary>
        /// Whether or not the action will be triggered if the person
        /// who sent the message was this bot.
        /// </summary>
        public bool RespondToSelf { get; private set; }

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

            Match match = pattern.Match( args.Line );
            if( match.Success )
            {
                string remoteUser = match.Groups["nickOrServer"].Value;
                string channel = match.Groups["channel"].Value;
                string message = match.Groups["theIrcMessage"].Value;

                if( args.BlackListedChannels.Contains( channel.ToLower() ) )
                {
                    // Blacklist channel, return.
                    return;
                }

                if( this.lastEvent.ContainsKey( channel ) == false )
                {
                    this.lastEvent[channel] = DateTime.MinValue;
                }

                // If we are a bridge bot, we need to change
                // the nick and the channel
                foreach( string bridgeBotRegex in args.IrcConfig.BridgeBots.Keys )
                {
                    Match nameMatch = Regex.Match( remoteUser, bridgeBotRegex );
                    if( nameMatch.Success )
                    {
                        Match bridgeBotMatch = Regex.Match( message, args.IrcConfig.BridgeBots[bridgeBotRegex] );

                        // If the regex matches, then we'll update the nick and message
                        // to be whatever came from the bridge.
                        if( bridgeBotMatch.Success )
                        {
                            string newNick = bridgeBotMatch.Groups["bridgeUser"].Value;
                            string newMessage = bridgeBotMatch.Groups["bridgeMessage"].Value;

                            // Only change the nick anme and the message if the nick and the message aren't empty.
                            if( ( string.IsNullOrEmpty( newNick ) == false ) && ( string.IsNullOrEmpty( newMessage ) == false ) )
                            {
                                remoteUser = newNick;
                                message = newMessage;
                            }

                            break;  // We have our message, break out of the loop.
                        }
                    }
                }

                // Take the message from the PRIVMSG and see if it matches the regex this class is watching.
                // If not, return and do nothing.

                // But first, Liquefy things!
                Regex lineRegex = new Regex(
                    Parsing.LiquefyStringWithIrcConfig(
                        this.LineRegex,
                        remoteUser,
                        args.IrcConfig.Nick,
                        channel
                    )
                );

                Match messageMatch = lineRegex.Match( message );
                if( messageMatch.Success == false )
                {
                    return;
                }

                IrcResponse response = new IrcResponse(
                    remoteUser,
                    channel,
                    message,
                    lineRegex,
                    messageMatch
                );

                // Return right away if the nick name from the remote user is our own.
                if(
                    ( this.RespondToSelf == false ) &&
                    ( response.RemoteUser.ToUpper() == args.IrcConfig.Nick.ToUpper() )
                )
                {
                    return;
                }
                // Return right away if we only wish to respond on the channel we are listening on (ignore PMs).
                else if(
                    ( this.ResponseOption == ResponseOptions.ChannelOnly ) &&
                    ( args.IrcConfig.Channels.Any( c => c.ToUpper() == response.Channel.ToUpper() ) == false )
                )
                {
                    return;
                }
                // Return right away if we only wish to respond to Private Messages (the channel will be our nick name).
                else if(
                    ( this.ResponseOption == ResponseOptions.PmsOnly ) &&
                    ( response.Channel.ToUpper() != args.IrcConfig.Nick.ToUpper() )
                )
                {
                    return;
                }
                else
                {
                    if( response.Channel.ToUpper() == args.IrcConfig.Nick.ToUpper() )
                    {
                        // If our response is a PM (channel name matches our bot's)
                        // we need to change our channel to the remote user's
                        // channel so it gets sent out correctly when handle event is called.
                        response = new IrcResponse(
                            remoteUser,
                            remoteUser,
                            message,
                            lineRegex,
                            messageMatch
                        );
                    }

                    DateTime currentTime = DateTime.UtcNow;
                    TimeSpan timeSpan = currentTime - this.lastEvent[channel];

                    // Only fire if our cooldown was long enough. Cooldown of zero means always fire.
                    // Need to explictly say Cooldown == 0 since DateTime.UtcNow has a innacurracy of +/- 15ms.  Therefore,
                    // if the action happens too quickly, it can incorrectly not be triggered.
                    if( ( this.CoolDown == 0 ) || ( timeSpan.TotalSeconds > this.CoolDown ) )
                    {
                        this.LineAction( args.IrcWriter, response );
                        this.lastEvent[channel] = currentTime;
                    }
                }
            }
        }
    }
}