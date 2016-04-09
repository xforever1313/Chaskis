
//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

namespace GenericIrcBot
{
    /// <summary>
    /// Represents a response from IRC.
    /// </summary>
    public class IrcResponse
    {
        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="remoteUser">The user that sent the message.</param>
        /// <param name="channel">The message that channel was received on.</param>
        /// <param name="message">The message that was sent.</param>
        public IrcResponse( string remoteUser, string channel, string message )
        {
            this.RemoteUser = remoteUser;
            this.Channel = channel;
            this.Message = message;
        }

        /// <summary>
        /// The user that sent the message.
        /// </summary>
        public string RemoteUser { get; private set; }

        /// <summary>
        /// The channel that the message was received on.
        /// </summary>
        /// <value>The channel.</value>
        public string Channel { get; private set; }

        /// <summary>
        /// The message that was sent via IRC.  Empty if a Join/Part event.
        /// </summary>
        public string Message { get; private set; }
    }
}
