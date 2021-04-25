//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Text.RegularExpressions;

namespace Chaskis.Core
{
    /// <summary>
    /// Arguments that are passed in when <see cref="MessageHandler"/> is triggered.
    /// </summary>
    public class MessageHandlerArgs : BasePrivateMessageHandlerArgs
    {
        // ---------------- Constructor ----------------

        public MessageHandlerArgs(
            IIrcWriter writer,
            string user,
            string channel,
            string message,
            Regex regex,
            Match match
        ) : base( writer, user, channel, message, regex, match )
        {
        }
    }
}