
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
        // -------- Constructor --------

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

        // -------- Properties --------

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

        // -------- Functions --------

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current.</param>
        /// <returns>true if the specified object is equal to the current object; otherwise, false.</returns>
        public override bool Equals( object obj )
        {
            IrcResponse other = obj as IrcResponse;
            if( other == null )
            {
                return false;
            }

            return (
                ( this.RemoteUser == other.RemoteUser ) &&
                ( this.Channel == other.Channel ) &&
                ( this.Message == other.Message )
            );
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        /// <summary>
        /// Clone this instance.
        /// </summary>
        public IrcResponse Clone()
        {
            return ( IrcResponse )this.MemberwiseClone();
        }
    }
}
