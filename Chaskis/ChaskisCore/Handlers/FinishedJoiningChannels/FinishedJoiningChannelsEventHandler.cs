//
//          Copyright Seth Hendrick 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Text.RegularExpressions;

namespace Chaskis.Core
{
    public delegate void FinishedJoiningChannelsHandlerAction( FinishedJoiningChannelsEventArgs args );

    /// <summary>
    /// Event that gets fired when the bot joins a server and finishes joining the channels
    /// it is configured to join.
    /// </summary>
    public sealed class FinishedJoiningChannelsEventHandler : BaseConnectionEventHandler<FinishedJoiningChannelsEventConfig>
    {
        // ---------------- Fields ----------------

        private static readonly Regex regex = new Regex(
            $@"^<{FinishedJoiningChannelsEventArgs.XmlRootName}>.+</{FinishedJoiningChannelsEventArgs.XmlRootName}>",
            RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        // ---------------- Constructor ----------------

        public FinishedJoiningChannelsEventHandler( FinishedJoiningChannelsEventConfig config ) :
            base( config, regex )
        {
        }

        // ---------------- Functions ----------------

        protected override void HandleEventInternal( HandlerArgs args, Match match )
        {
            FinishedJoiningChannelsEventArgs connectionArgs = FinishedJoiningChannelsEventArgsExtensions.FromXml( args.Line, args.IrcWriter );
            this.config.LineAction( connectionArgs );
        }
    }
}
