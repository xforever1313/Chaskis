//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Text;
using SethCS.Exceptions;

namespace Chaskis.Core
{
    /// <summary>
    /// Configuration used for <see cref="ReceiveHandler"/>
    /// </summary>
    public class ReceiveHandlerConfig
    {
        // ---------------- Constructor ----------------

        public ReceiveHandlerConfig()
        {
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// The action to take when ANY message appears from IRC (JOIN, PART, PRIVMSG, PING, etc).
        /// As far as the passed in IrcResponse to the action goes, the channel and remote user
        /// will be String.Empty, since this class does no parsing of the IRC message.
        /// It just grabs the line from the IRC channel and passes it into the <see cref="LineAction"/>
        /// with no parsing.  It is up to the <see cref="LineAction"/> to parse the channel and user
        /// name if they so desire.
        /// </summary>
        public ReceiveHandlerAction LineAction { get; set; }

        // ---------------- Functions ----------------

        public void Validate()
        {
            bool success = true;
            StringBuilder errorString = new StringBuilder();
            errorString.AppendLine( "Errors when validating " + nameof( ReceiveHandlerConfig ) );

            if( this.LineAction == null )
            {
                success = false;
                errorString.AppendLine( "\t- " + nameof( this.LineAction ) + " can not be null" );
            }

            if( success == false )
            {
                throw new ValidationException( errorString.ToString() );
            }
        }

        public ReceiveHandlerConfig Clone()
        {
            return (ReceiveHandlerConfig)this.MemberwiseClone();
        }
    }
}
