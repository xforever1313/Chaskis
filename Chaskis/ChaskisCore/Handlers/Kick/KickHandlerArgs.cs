//
//          Copyright Seth Hendrick 2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Text.RegularExpressions;

namespace Chaskis.Core
{
    public class KickHandlerArgs
    {
        // ---------------- Constructor ----------------

        public KickHandlerArgs(
            IIrcWriter writer,
            string userWhoSentKick,
            string channel,
            string userWhoWasKicked,
            string reason,
            Regex regex,
            Match match
        )
        {
            this.Writer = writer;
            this.UserWhoSentKick = userWhoSentKick;
            this.Channel = channel;
            this.UserWhoWasKicked = userWhoWasKicked;
            this.Reason = reason;
            this.Regex = regex;
            this.Match = match;
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// The Writer to use so we can respond to the Kick Event.
        /// </summary>
        public IIrcWriter Writer { get; private set; }

        /// <summary>
        /// The nick of the user who sent the kick message.
        /// </summary>
        public string UserWhoSentKick { get; private set; }

        /// <summary>
        /// Which channel the user was kicked from.
        /// </summary>
        public string Channel { get; private set; }

        /// <summary>
        /// The user who was kicked from the channel.
        /// </summary>
        public string UserWhoWasKicked { get; private set; }

        /// <summary>
        /// The reason why a user was kicked.
        /// Empty if no reason was specified.
        /// </summary>
        public string Reason { get; private set; }

        /// <summary>
        /// The regex that was used to find this response from the server.
        /// </summary>
        public Regex Regex { get; private set; }

        /// <summary>
        /// The match that was used to generate this class.
        /// </summary>
        public Match Match { get; private set; }
    }
}
