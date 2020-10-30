//
//          Copyright Seth Hendrick 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Text.RegularExpressions;

namespace Chaskis.Core
{
    public delegate void DisconnectedHandlerAction( DisconnectedEventArgs args );

    /// <summary>
    /// Event that is fired when the bot is about to disconnect
    /// from an IRC server, but has not yet.
    /// 
    /// When this event comes through, there is no connection to the server,
    /// so expect nothing to be able to be written out,
    /// and nothing to be received.
    /// </summary>
    public sealed class DisconnectedEventHandler : BaseCoreEventHandler<DisconnectedEventConfig>
    {
        // ---------------- Fields ----------------

        private static readonly Regex regex = new Regex(
            $@"^<{DisconnectedEventArgs.XmlRootName}>.+</{DisconnectedEventArgs.XmlRootName}>",
            RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.IgnoreCase
        );


        // ---------------- Constructor ----------------

        public DisconnectedEventHandler( DisconnectedEventConfig config ) :
            base( config, regex )
        {
        }

        // ---------------- Functions ----------------

        protected override void HandleEventInternal( HandlerArgs args, Match match )
        {
            DisconnectedEventArgs connectionArgs = DisconnectedEventArgsExtensions.FromXml( args.Line );
            this.config.LineAction( connectionArgs );
        }
    }
}
