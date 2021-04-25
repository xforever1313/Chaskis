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
    /// Arguments that are passed in when <see cref="PartHandler"/> is triggered.
    /// </summary>
    public class PartHandlerArgs
    {
        // ---------------- Constructor ----------------

        public PartHandlerArgs(
            IIrcWriter writer,
            string user,
            string channel,
            string reason,
            Regex regex,
            Match match
        )
        {
            this.Writer = writer;
            this.User = user;
            this.Channel = channel;
            this.Reason = reason;
            this.Regex = regex;
            this.Match = match;
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// The Writer to use so we can respond to the PART Event.
        /// </summary>
        public IIrcWriter Writer { get; private set; }

        /// <summary>
        /// The user that parted the channel
        /// </summary>
        public string User { get; private set; }

        /// <summary>
        /// The channel the user left.
        /// </summary>
        public string Channel { get; private set; }

        /// <summary>
        /// The reason the user left the channel.
        /// <see cref="string.Empty"/> for no reason.
        /// </summary>
        public string Reason { get; private set; }

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
