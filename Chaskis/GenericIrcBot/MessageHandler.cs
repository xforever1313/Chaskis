//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Text.RegularExpressions;
using SethCS.Exceptions;

namespace GenericIrcBot
{
    /// <summary>
    /// Configuration for responding to a message received from IRC.
    /// </summary>
    public class MessageHandler : IIrcHandler
    {
        // -------- Fields --------

        /// <summary>
        /// The irc command that will appear from the server.
        /// </summary>
        public const string IrcCommand = "PRIVMSG";

        // :nickName!~nick@10.0.0.1 PRIVMSG #TestChan :!bot help
        /// <summary>
        /// The pattern to search for when a line comes in.
        /// </summary>
        private static readonly Regex pattern = new Regex(
            @"^:(?<nick>\w+)!~(?<user>.+)\s+" + IrcCommand + @"\s+(?<channel>#?\w+)\s+:(?<theIrcMessage>.+)",
            RegexOptions.Compiled
        );

        // -------- Constructor --------

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="lineRegex">
        /// The regex to search for to fire the action.
        /// For example, if you want !bot help to trigger the action, pass in "!bot\s+help"
        /// </param>
        /// <param name="lineAction">The action to perform based on the line.</param>
        /// <param name="coolDown">How long to wait between firing the line action in seconds.  0 for no cooldown.</param>
        /// <param name="responseOption">Whether or not to respond to PMs, only channels, or both.</param>
        /// <param name="respondToSelf">Whether or not the bot should respond to lines sent out by itself. Defaulted to false.</param>
        public MessageHandler(
            string lineRegex,
            Action<IIrcWriter, IrcResponse> lineAction,
            int coolDown = 0,
            ResponseOptions responseOption = ResponseOptions.RespondToBoth,
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
            this.LastEvent = DateTime.MinValue;
            this.KeepHandling = true;
        }

        // -------- Properties --------

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
        /// The last time this event was triggered.
        /// </summary>
        public DateTime LastEvent { get; private set; }

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

        // -------- Function --------

        /// <summary>
        /// Fires the action if the line regex matches.
        /// </summary>
        /// <param name="line">The RAW line from IRC to check.</param>
        /// <param name="ircConfig">The irc config to use when parsing this line.</param>
        /// <param name="ircWriter">The way to write to the irc channel.</param>
        public void HandleEvent( string line, IIrcConfig ircConfig, IIrcWriter ircWriter )
        {
            ArgumentChecker.StringIsNotNullOrEmpty( line, nameof( line ) );
            ArgumentChecker.IsNotNull( ircConfig, nameof( ircConfig ) );
            ArgumentChecker.IsNotNull( ircWriter, nameof( ircWriter ) );

            Match match = pattern.Match( line );
            if( match.Success )
            {
                string nick = match.Groups["nick"].Value;
                string channel = match.Groups["channel"].Value;
                string message = match.Groups["theIrcMessage"].Value;

                // If we are a bridge bot, we need to change
                // the nick and the channel
                if( ircConfig.BridgeBots.ContainsKey( nick ) )
                {
                    Match bridgeBotMatch = Regex.Match( message, ircConfig.BridgeBots[nick] );

                    // If the regex matches, then we'll update the nick and message
                    // to be whatever came from the bridge.
                    if( bridgeBotMatch.Success )
                    {
                        string newNick = bridgeBotMatch.Groups["bridgeUser"].Value;
                        string newMessage = bridgeBotMatch.Groups["bridgeMessage"].Value;

                        // Only change the nick anme and the message if the nick and the message aren't empty.
                        if( ( string.IsNullOrEmpty( newNick ) == false ) && ( string.IsNullOrEmpty( newMessage ) == false ) )
                        {
                            nick = newNick;
                            message = newMessage;
                        }
                    }
                }

                // Take the message from the PRIVMSG and see if it matches the regex this class is watching.
                // If not, return and do nothing.
                Match messageMatch = Regex.Match( message, this.LineRegex );
                if( messageMatch.Success == false )
                {
                    return;
                }

                IrcResponse response = new IrcResponse(
                    nick,
                    channel,
                    message
                );

                // Return right away if the nick name from the remote user is our own.
                if( ( this.RespondToSelf == false ) && ( response.RemoteUser.ToUpper() == ircConfig.Nick.ToUpper() ) )
                {
                    return;
                }
                // Return right away if we only wish to respond on the channel we are listening on (ignore PMs).
                else if( ( this.ResponseOption == ResponseOptions.RespondOnlyToChannel ) && ( response.Channel.ToUpper() != ircConfig.Channel.ToUpper() ) )
                {
                    return;
                }
                // Return right away if we only wish to respond to Private Messages (the channel will be our nick name).
                else if( ( this.ResponseOption == ResponseOptions.RespondOnlyToPMs ) && ( response.Channel.ToUpper() != ircConfig.Nick.ToUpper() ) )
                {
                    return;
                }
                else
                {
                    DateTime currentTime = DateTime.UtcNow;
                    TimeSpan timeSpan = currentTime - this.LastEvent;

                    // Only fire if our cooldown was long enough.
                    if( timeSpan.TotalSeconds > this.CoolDown )
                    {
                        this.LineAction( ircWriter, response );
                        this.LastEvent = currentTime;
                    }
                }
            }
        }
    }
}