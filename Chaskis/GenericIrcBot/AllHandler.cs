
//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using SethCS.Exceptions;

namespace GenericIrcBot
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
                line
            );

            this.AllAction( ircWriter, response );
        }
    }
}
