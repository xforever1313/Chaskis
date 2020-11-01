//
//          Copyright Seth Hendrick 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Text.RegularExpressions;

namespace Chaskis.Core
{
    public delegate void SendJoinHandlerAction( SendJoinEventArgs args );

    /// <summary>
    /// Event that gets fired when the bot attempts to join a channel.
    /// 
    /// This does NOT mean the bot successfully joined the channel, it simply
    /// requested the server to join the channel.  To determine if a bot joined
    /// a channel successfully or not, subscribe to the <see cref="JoinHandler"/>,
    /// and look for <see cref="JoinHandlerArgs.User"/>
    /// to match the bot's name.
    /// </summary>
    public sealed class SendJoinEventHandler : BaseCoreEventHandler<SendJoinEventConfig>
    {
        // ---------------- Fields ----------------

        private static readonly Regex regex = new Regex(
            $@"^<{SendJoinEventArgs.XmlRootName}>.+</{SendJoinEventArgs.XmlRootName}>",
            RegexOptions.ExplicitCapture | RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        // ---------------- Constructor ----------------

        public SendJoinEventHandler( SendJoinEventConfig config ) :
            base( config, regex )
        {
        }

        // ---------------- Functions ----------------

        protected override void HandleEventInternal( HandlerArgs args, Match match )
        {
            SendJoinEventArgs connectionArgs = SendJoinEventArgsExtensions.FromXml( args.Line, args.IrcWriter );
            this.config.LineAction( connectionArgs );
        }
    }
}
