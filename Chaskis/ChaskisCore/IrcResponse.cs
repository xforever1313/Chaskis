//
//          Copyright Seth Hendrick 2016-2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Text.RegularExpressions;

namespace Chaskis.Core
{
    /// <summary>
    /// Represents a response from IRC.
    /// </summary>
    public class IrcResponse
    {
        // -------- Constructor --------

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="remoteUser">The user that sent the message.</param>
        /// <param name="channel">The message that channel was received on.</param>
        /// <param name="message">The message that was sent.</param>
        /// <param name="match">The Regex match that was used to get this response, if any.</param>
        public IrcResponse( string remoteUser, string channel, string message, Regex regex, Match match )
        {
            this.RemoteUser = remoteUser;
            this.Channel = channel;
            this.Message = message;
            this.Regex = regex;
            this.Match = match;
        }

        // -------- Properties --------

        /// <summary>
        /// The user that sent the message.
        /// </summary>
        public string RemoteUser { get; private set; }

        /// <summary>
        /// The channel that the message was received on.
        /// However, if the message was a PM, then this will become the user
        /// who sent the message's name (a PM would usually have the channel be this bot's name),
        /// so we can call the same function for a channel message
        /// and a private message. 
        /// </summary>
        public string Channel { get; private set; }

        /// <summary>
        /// The message that was sent via IRC.  Empty if a Join/Part event.
        /// </summary>
        public string Message { get; private set; }
        
        /// <summary>
        /// The regex that was used to find this response.
        /// Null if no regex was used.
        /// </summary>
        public Regex Regex { get; private set; }

        /// <summary>
        /// The regex match that was used to find this response.
        /// Null if no Match was used.
        /// </summary>
        public Match Match { get; private set; }
    }
}