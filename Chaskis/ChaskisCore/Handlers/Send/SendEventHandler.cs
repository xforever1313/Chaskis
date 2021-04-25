//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Text.RegularExpressions;

namespace Chaskis.Core
{
    public delegate void SendHandlerAction( SendEventArgs args );

    /// <summary>
    /// Event that gets fired when the bot sends ALMOST ANYTHING to the server.
    /// This means while you could use this to capture, say us sending a join command to the server,
    /// you're better off using <see cref="SendJoinEventHandler"/>.
    /// </summary>
    public sealed class SendEventHandler : BaseCoreEventHandler<SendEventConfig>
    {
        // ---------------- Fields ----------------

        public static readonly Regex Regex = new Regex(
            $@"^<{SendEventArgs.XmlRootName}>.+</{SendEventArgs.XmlRootName}>",
            RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        // ---------------- Constructor ----------------

        public SendEventHandler( SendEventConfig config ) :
            base( config, Regex )
        {
        }

        // ---------------- Functions ----------------

        protected override void HandleEventInternal( HandlerArgs args, Match match )
        {
            SendEventArgs connectionArgs = SendEventArgsExtensions.FromXml( args.Line, args.IrcWriter );
            this.config.LineAction( connectionArgs );
        }
    }
}
