//
//          Copyright Seth Hendrick 2016-2019.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Text.RegularExpressions;

namespace Chaskis.Core
{
    public class ActionHandlerArgs : BasePrivateMessageHandlerArgs
    {
        // ---------------- Constructor ----------------

        public ActionHandlerArgs(
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
