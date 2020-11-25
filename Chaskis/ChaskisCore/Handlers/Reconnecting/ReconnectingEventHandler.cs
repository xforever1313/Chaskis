//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Text.RegularExpressions;

namespace Chaskis.Core
{
    public delegate void ReconnectingHandlerAction( ReconnectingEventArgs args );

    /// <summary>
    /// Event that is fired when the bot is about to try to reconnect
    /// to the server.  When this event is fired, there is no connection to the server yet.
    /// Use the <see cref="ConnectedEventHandler"/> to know when the server connects to the server.
    /// </summary>
    public sealed class ReconnectingEventHandler : BaseCoreEventHandler<ReconnectingEventConfig>
    {
        // ---------------- Fields ----------------

        private static readonly Regex regex = new Regex(
            $@"^<{ReconnectingEventArgs.XmlRootName}>.+</{ReconnectingEventArgs.XmlRootName}>",
            RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.IgnoreCase
        );


        // ---------------- Constructor ----------------

        public ReconnectingEventHandler( ReconnectingEventConfig config ) :
            base( config, regex )
        {
        }

        // ---------------- Functions ----------------

        protected override void HandleEventInternal( HandlerArgs args, Match match )
        {
            ReconnectingEventArgs connectionArgs = ReconnectingEventArgsExtensions.FromXml( args.Line );
            this.config.LineAction( connectionArgs );
        }
    }
}
