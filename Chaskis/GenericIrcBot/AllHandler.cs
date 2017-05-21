//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using SethCS.Exceptions;

namespace ChaskisCore
{
    /// <summary>
    /// This class will fire for ALL IRC messages and pass in the raw
    /// IRC message as the message string.
    /// </summary>
    public class AllHandler : IIrcHandler
    {
        // -------- Constructor --------

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="allAction">The action to take when ANY message appears from IRC (JOIN, PART, PRIVMSG, PING, etc).</param>
        public AllHandler( Action<IIrcWriter, IrcResponse> allAction )
        {
            ArgumentChecker.IsNotNull( allAction, nameof( allAction ) );

            this.AllAction = allAction;
            this.KeepHandling = true;
        }

        // -------- Properties --------

        /// <summary>
        /// The action to take when ANY message appears from IRC (JOIN, PART, PRIVMSG, PING, etc).
        /// As far as the passed in IrcResponse to the action goes, the channel and remote user
        /// will be String.Empty, since this class does no parsing of the IRC message.
        /// It just grabs the line from the IRC channel and passes it into the AllAction
        /// with no parsing.  It is up to the AllAction to parse the channel and user
        /// name if they so desire.
        /// </summary>
        public Action<IIrcWriter, IrcResponse> AllAction { get; private set; }

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

        // -------- Functions --------

        /// <summary>
        /// Handles the event.
        /// </summary>
        /// <param name="line">The RAW line from IRC to check.</param>
        /// <param name="ircConfig">The irc config to use when parsing this line.</param>
        /// <param name="ircWriter">The way to write to the irc channel.</param>
        public void HandleEvent( string line, IIrcConfig ircConfig, IIrcWriter ircWriter )
        {
            ArgumentChecker.StringIsNotNullOrEmpty( line, nameof( line ) );
            ArgumentChecker.IsNotNull( ircConfig, nameof( ircConfig ) );
            ArgumentChecker.IsNotNull( ircWriter, nameof( ircWriter ) );

            IrcResponse response = new IrcResponse(
                string.Empty,
                string.Empty,
                line,
                null,
                null
            );

            this.AllAction( ircWriter, response );
        }
    }
}