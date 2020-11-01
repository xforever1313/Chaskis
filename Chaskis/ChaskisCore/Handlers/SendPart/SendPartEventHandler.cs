//
//          Copyright Seth Hendrick 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Text.RegularExpressions;

namespace Chaskis.Core
{
    public delegate void SendPartHandlerAction( SendPartEventArgs args );

    /// <summary>
    /// Event that gets fired when the bot joins a server.
    /// </summary>
    public sealed class SendPartEventHandler : BaseCoreEventHandler<SendPartEventConfig>
    {
        // ---------------- Fields ----------------

        private static readonly Regex regex = new Regex(
            $@"^<{SendPartEventArgs.XmlRootName}>.+</{SendPartEventArgs.XmlRootName}>",
            RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        // ---------------- Constructor ----------------

        public SendPartEventHandler( SendPartEventConfig config ) :
            base( config, regex )
        {
        }

        // ---------------- Functions ----------------

        protected override void HandleEventInternal( HandlerArgs args, Match match )
        {
            SendPartEventArgs connectionArgs = SendPartEventArgsExtensions.FromXml( args.Line, args.IrcWriter );
            this.config.LineAction( connectionArgs );
        }
    }
}
