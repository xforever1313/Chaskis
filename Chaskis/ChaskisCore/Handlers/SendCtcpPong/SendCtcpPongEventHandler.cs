//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Text.RegularExpressions;

namespace Chaskis.Core
{
    public delegate void SendCtcpPongEventHandlerAction( SendCtcpPongEventArgs args );

    /// <summary>
    /// Event that is fired when the bot sends a CTCP Pong response to
    /// the IRC Server.
    /// </summary>
    public sealed class SendCtcpPongEventHandler : BaseCoreEventHandler<SendCtcpPongEventConfig>
    {
        // ---------------- Fields ----------------

        private static readonly Regex regex = new Regex(
            $@"^<{SendCtcpPongEventArgs.XmlRootName}>.+</{SendCtcpPongEventArgs.XmlRootName}>",
            RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        // ---------------- Constructor ----------------

        public SendCtcpPongEventHandler( SendCtcpPongEventConfig config ) :
            base( config, regex )
        {
        }

        // ---------------- Functions ----------------

        protected override void HandleEventInternal( HandlerArgs args, Match match )
        {
            SendCtcpPongEventArgs eventArgs = SendCtcpPongEventArgsExtensions.FromXml( args.Line, args.IrcWriter );
            this.config.LineAction( eventArgs );
        }
    }
}
