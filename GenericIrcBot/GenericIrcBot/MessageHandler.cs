
//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Text.RegularExpressions;

namespace GenericIrcBot
{
    /// <summary>
    /// Configuration for responding to a message received from IRC.
    /// </summary>
    public class MessageHandler : IIrcHandler
    {
        /// <summary>
        /// The pattern to search for when a line comes in.
        /// </summary>
        private readonly Regex pattern;

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
            if( string.IsNullOrEmpty( lineRegex ) )
            {
                throw new ArgumentNullException( nameof( lineRegex ) );
            }
            else if( lineAction == null )
            {
                throw new ArgumentNullException( nameof( lineAction ) );
            }
            else if( coolDown < 0 )
            {
                throw new ArgumentException( "cool down must be greater than zero", nameof( coolDown ) );
            }

            this.LineRegex = lineRegex;
            this.LineAction = lineAction;
            this.CoolDown = coolDown;
            this.RespondToSelf = respondToSelf;
            this.ResponseOption = responseOption;
            this.LastEvent = DateTime.MinValue;

            // :nickName!~nick@10.0.0.1 PRIVMSG #TestChan :!bot help
            this.pattern = new Regex(
                @"^:(?<nick>\w+)!~(?<user>.+)\s+PRIVMSG\s+(?<channel>#?\w+)\s+:(?<msg>" + lineRegex + ")",
                RegexOptions.Compiled
            );
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
        public int CoolDown{ get; private set; }

        /// <summary>
        /// Whether or not this bot will respond to private messages or not.
        /// </summary>
        public ResponseOptions ResponseOption { get; private set; }

        /// <summary>
        /// Whether or not the action will be triggered if the person
        /// who sent the message was this bot.
        /// </summary>
        public bool RespondToSelf{ get; private set; }

        /// <summary>
        /// The last time this event was triggered.
        /// </summary>
        public DateTime LastEvent { get; private set; }

        // -------- Function --------

        /// <summary>
        /// Fires the action if the line regex matches.
        /// </summary>
        /// <param name="line">The RAW line from IRC to check.</param>
        /// <param name="ircConfig">The irc config to use when parsing this line.</param>
        /// <param name="ircWriter">The way to write to the irc channel.</param>
        public void HandleEvent( string line, IIrcConfig ircConfig, IIrcWriter ircWriter )
        {
            if( string.IsNullOrEmpty( line ) )
            {
                throw new ArgumentNullException( nameof( line ) );
            }
            else if( ircConfig == null )
            {
                throw new ArgumentNullException( nameof( ircConfig ) );
            }
            else if( ircWriter == null )
            {
                throw new ArgumentNullException( nameof( ircConfig ) );
            }

            Match match = this.pattern.Match( line );
            if( match.Success )
            {
                IrcResponse response = new IrcResponse(
                                           match.Groups["nick"].Value,
                                           match.Groups["channel"].Value,
                                           match.Groups["msg"].Value
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
