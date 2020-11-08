//
//          Copyright Seth Hendrick 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Text.RegularExpressions;

namespace Chaskis.Core
{
    public delegate void SendActionEventHandlerAction( SendActionEventArgs args );

    /// <summary>
    /// Event that is fired when the bot sends an action to
    /// the IRC Server.
    /// </summary>
    public sealed class SendActionEventHandler : BaseCoreEventHandler<SendActionEventConfig>
    {
        // ---------------- Fields ----------------

        private static readonly Regex regex = new Regex(
            $@"^<{SendActionEventArgs.XmlRootName}>.+</{SendActionEventArgs.XmlRootName}>",
            RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        // ---------------- Constructor ----------------

        public SendActionEventHandler( SendActionEventConfig config ) :
            base( config, regex )
        {
        }

        // ---------------- Functions ----------------

        protected override void HandleEventInternal( HandlerArgs args, Match match )
        {
            SendActionEventArgs eventArgs = SendActionEventArgsExtensions.FromXml( args.Line, args.IrcWriter );
            this.config.LineAction( eventArgs );
        }
    }
}
