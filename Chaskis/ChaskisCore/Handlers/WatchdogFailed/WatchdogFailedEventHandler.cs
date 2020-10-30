//
//          Copyright Seth Hendrick 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Text.RegularExpressions;

namespace Chaskis.Core
{
    public delegate void WatchdogFailedHandlerAction( WatchdogFailedEventArgs args );

    /// <summary>
    /// Event that is fired when the bot does not get a PONG from the server,
    /// so our watchdog fails.  This means we are about to try to reconnect to the server.
    /// </summary>
    public sealed class WatchdogFailedEventHandler : BaseConnectionEventHandler<WatchdogFailedEventConfig>
    {
        // ---------------- Fields ----------------

        private static readonly Regex regex = new Regex(
            $@"^<{WatchdogFailedEventArgs.XmlRootName}>.+</{WatchdogFailedEventArgs.XmlRootName}>",
            RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.IgnoreCase
        );


        // ---------------- Constructor ----------------

        public WatchdogFailedEventHandler( WatchdogFailedEventConfig config ) :
            base( config, regex )
        {
        }

        // ---------------- Functions ----------------

        protected override void HandleEventInternal( HandlerArgs args, Match match )
        {
            WatchdogFailedEventArgs connectionArgs = WatchdogFailedEventArgsExtensions.FromXml( args.Line );
            this.config.LineAction( connectionArgs );
        }
    }
}
