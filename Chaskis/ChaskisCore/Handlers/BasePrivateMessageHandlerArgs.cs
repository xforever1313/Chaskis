//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Text.RegularExpressions;

namespace Chaskis.Core
{
    public class BasePrivateMessageHandlerArgs: IPrivateMessageHandlerArgs
    {
        // ---------------- Constructor ----------------

        protected BasePrivateMessageHandlerArgs(
            IIrcWriter writer,
            string user,
            string channel,
            string message,
            Regex regex,
            Match match
        )
        {
            this.Writer = writer;
            this.User = user;
            this.Channel = channel;
            this.Message = message;
            this.Regex = regex;
            this.Match = match;
        }

        // ---------------- Properties ----------------

        public IIrcWriter Writer { get; private set; }

        /// <summary>
        /// The user that sent the message.
        /// </summary>
        public string User { get; private set; }

        /// <summary>
        /// The channel that the message was received on.
        /// However, if the message was a PM, then this will become the user
        /// who sent the message's name (a PM would usually have the channel be this bot's name),
        /// so we can call the same function for a channel message
        /// and a private message. 
        /// </summary>
        public string Channel { get; private set; }

        /// <summary>
        /// The message that was sent via IRC.
        /// </summary>
        public string Message { get; private set; }

        /// <summary>
        /// The regex that was used to find this response.
        /// </summary>
        public Regex Regex { get; private set; }

        /// <summary>
        /// The regex match that was used to find this response.
        /// </summary>
        public Match Match { get; private set; }
    }
}
