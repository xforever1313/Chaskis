//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Text.RegularExpressions;

namespace Chaskis.Core
{
    /// <remarks>
    /// <see cref="BasePrivateMessageHandlerArgs.Message"/> is usually empty with the VERSION
    /// command.  But, just in case one specifies something after "VERSION" in IRC, it will
    /// be put in there.  This is unlikely to happen though.
    /// </remarks>
    public class CtcpVersionHandlerArgs : BasePrivateMessageHandlerArgs
    {
        // ---------------- Constructor ----------------

        public CtcpVersionHandlerArgs(
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
