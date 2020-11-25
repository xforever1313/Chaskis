//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Text.RegularExpressions;

namespace Chaskis.Core
{
    /// <summary>
    /// Handler args that are consistent for all PRIVMSG types.
    /// </summary>
    public interface IPrivateMessageHandlerArgs
    {
        IIrcWriter Writer { get; }

        /// <summary>
        /// The user that sent the message.
        /// </summary>
        string User { get; }

        /// <summary>
        /// The channel that the message was received on.
        /// However, if the message was a PM, then this will become the user
        /// who sent the message's name (a PM would usually have the channel be this bot's name),
        /// so we can call the same function for a channel message
        /// and a private message. 
        /// </summary>
        string Channel { get; }

        /// <summary>
        /// The message that was sent via IRC.
        /// </summary>
        string Message { get; }

        /// <summary>
        /// The regex that was used to find this response.
        /// </summary>
        Regex Regex { get; }

        /// <summary>
        /// The regex match that was used to find this response.
        /// </summary>
        Match Match { get; }
    }
}
