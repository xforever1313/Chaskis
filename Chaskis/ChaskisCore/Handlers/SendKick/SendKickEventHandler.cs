//
//          Copyright Seth Hendrick 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Text.RegularExpressions;

namespace Chaskis.Core
{
    public delegate void SendKickHandlerAction( SendKickEventArgs args );

    /// <summary>
    /// Event that gets fired when the bot to kick a user from a channel.
    /// 
    /// This does not mean the user has been kicked, rather the bot simply sent
    /// the command that will cause the server to remove the user from the channel if the
    /// bot has the correct permissions.
    /// </summary>
    public sealed class SendKickEventHandler : BaseCoreEventHandler<SendKickEventConfig>
    {
        // ---------------- Fields ----------------

        private static readonly Regex regex = new Regex(
            $@"^<{SendKickEventArgs.XmlRootName}>.+</{SendKickEventArgs.XmlRootName}>",
            RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        // ---------------- Constructor ----------------

        public SendKickEventHandler( SendKickEventConfig config ) :
            base( config, regex )
        {
        }

        // ---------------- Functions ----------------

        protected override void HandleEventInternal( HandlerArgs args, Match match )
        {
            SendKickEventArgs connectionArgs = SendKickEventArgsExtensions.FromXml( args.Line, args.IrcWriter );
            this.config.LineAction( connectionArgs );
        }
    }
}
