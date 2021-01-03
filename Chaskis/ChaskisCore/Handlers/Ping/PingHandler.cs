//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Text.RegularExpressions;
using SethCS.Exceptions;

namespace Chaskis.Core
{
    /// <summary>
    /// Most servers will ping the bot using PING
    /// to make sure we are still there.
    /// We need to send a PONG back, otherwise
    /// the server will terminate our connection.
    /// </summary>
    internal sealed class PingHandler : BaseIrcHandler
    {
        // ---------------- Fields ----------------

        private static readonly Regex pattern =
            new Regex(
                @"^PING\s*:(?<response>.+)",
                RegexOptions.Compiled
            );

        // ---------------- Constructor ----------------

        public PingHandler() :
            base()
        {
        }


        // ---------------- Functions ----------------

        /// <summary>
        /// Handles the event and the pong back to the server.
        /// </summary>
        public override void HandleEvent( HandlerArgs args )
        {
            ArgumentChecker.IsNotNull( args, nameof( args ) );

            Match match = pattern.Match( args.Line );
            if( match.Success )
            {
                args.IrcWriter.SendPong( match.Groups["response"].Value );
            }
        }
    }
}