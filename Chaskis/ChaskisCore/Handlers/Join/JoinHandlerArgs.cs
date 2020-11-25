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
    /// Arguments that are passed in when <see cref="JoinHandler"/> is triggered.
    /// </summary>
    public class JoinHandlerArgs
    {
        // ---------------- Constructor ----------------

        public JoinHandlerArgs( IIrcWriter writer, string user, string channel, Regex regex, Match match )
        {
            this.Writer = writer;
            this.User = user;
            this.Channel = channel;
            this.Regex = regex;
            this.Match = match;
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// The Writer to use so we can respond to the JOIN Event.
        /// </summary>
        public IIrcWriter Writer { get; private set; }

        /// <summary>
        /// The user that joined the channel
        /// </summary>
        public string User { get; private set; }

        /// <summary>
        /// The channel the user joined.
        /// </summary>
        public string Channel { get; private set; }

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
