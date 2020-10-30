//
//          Copyright Seth Hendrick 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Text.RegularExpressions;

namespace Chaskis.Core
{
    public delegate void DisconnectingHandlerAction( DisconnectingEventArgs args );

    /// <summary>
    /// Event that is fired when the bot is about to disconnect
    /// from an IRC server, but has not yet.
    /// 
    /// When this event comes through, there is no guarentee anything
    /// written to the IRC server will make it there.  There is also
    /// no guarentee that we'll get anything else from the server.
    /// </summary>
    public sealed class DisconnectingEventHandler : BaseCoreEventHandler<DisconnectingEventConfig>
    {
        // ---------------- Fields ----------------

        private static readonly Regex regex = new Regex(
            $@"^<{DisconnectingEventArgs.XmlRootName}>.+</{DisconnectingEventArgs.XmlRootName}>",
            RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.IgnoreCase
        );


        // ---------------- Constructor ----------------

        public DisconnectingEventHandler( DisconnectingEventConfig config ) :
            base( config, regex )
        {
        }

        // ---------------- Functions ----------------

        protected override void HandleEventInternal( HandlerArgs args, Match match )
        {
            DisconnectingEventArgs connectionArgs = DisconnectingEventArgsExtensions.FromXml( args.Line );
            this.config.LineAction( connectionArgs );
        }
    }
}
