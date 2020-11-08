//
//          Copyright Seth Hendrick 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Text.RegularExpressions;

namespace Chaskis.Core
{
    public delegate void SendMessageEventHandlerAction( SendMessageEventArgs args );

    /// <summary>
    /// Event that is fired when the bot sends a private message to
    /// the IRC Server.
    /// </summary>
    public sealed class SendMessageEventHandler : BaseCoreEventHandler<SendMessageEventConfig>
    {
        // ---------------- Fields ----------------

        private static readonly Regex regex = new Regex(
            $@"^<{SendMessageEventArgs.XmlRootName}>.+</{SendMessageEventArgs.XmlRootName}>",
            RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        // ---------------- Constructor ----------------

        public SendMessageEventHandler( SendMessageEventConfig config ) :
            base( config, regex )
        {
        }

        // ---------------- Functions ----------------

        protected override void HandleEventInternal( HandlerArgs args, Match match )
        {
            SendMessageEventArgs eventArgs = SendMessageEventArgsExtensions.FromXml( args.Line, args.IrcWriter );
            this.config.LineAction( eventArgs );
        }
    }
}
