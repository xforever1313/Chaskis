//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Text.RegularExpressions;

namespace Chaskis.Core
{
    public delegate void SendCtcpVersionEventHandlerAction( SendCtcpVersionEventArgs args );

    /// <summary>
    /// Event that is fired when the bot sends a CTCP Version response to
    /// the IRC Server.
    /// </summary>
    public sealed class SendCtcpVersionEventHandler : BaseCoreEventHandler<SendCtcpVersionEventConfig>
    {
        // ---------------- Fields ----------------

        private static readonly Regex regex = new Regex(
            $@"^<{SendCtcpVersionEventArgs.XmlRootName}>.+</{SendCtcpVersionEventArgs.XmlRootName}>",
            RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        // ---------------- Constructor ----------------

        public SendCtcpVersionEventHandler( SendCtcpVersionEventConfig config ) :
            base( config, regex )
        {
        }

        // ---------------- Functions ----------------

        protected override void HandleEventInternal( HandlerArgs args, Match match )
        {
            SendCtcpVersionEventArgs eventArgs = SendCtcpVersionEventArgsExtensions.FromXml( args.Line, args.IrcWriter );
            this.config.LineAction( eventArgs );
        }
    }
}
