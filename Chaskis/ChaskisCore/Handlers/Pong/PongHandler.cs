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
    /// Used to handle receiving pongs from a connection.
    /// </summary>
    internal sealed class PongHandler : BaseIrcHandler
    {
        // ---------------- Fields ----------------

        /// <summary>
        /// Pattern to watch for after sending a ping.
        /// 
        /// nickOrServer: Indicates where our message came from, usually the server URL.
        /// server: Indicates where our response came from
        /// response: The response the server sent to us.
        /// </summary>
        private static readonly Regex pattern =
            new Regex(
                Regexes.IrcMessagePrefix + @"\s+PONG\s+(?<server>.+)\s*:(?<response>.+)",
                RegexOptions.Compiled | RegexOptions.ExplicitCapture
            );

        // ---------------- Constructor ----------------

        public PongHandler() :
            base()
        {
        }

        // ---------------- Functions ----------------

        /// <summary>
        /// Handles the event and sends the responses to the channel if desired.
        /// </summary>
        public override void HandleEvent( HandlerArgs args )
        {
            ArgumentChecker.IsNotNull( args, nameof( args ) );

            Match match = pattern.Match( args.Line );
            if( match.Success )
            {
                args.IrcWriter.ReceivedPong( match.Groups["response"].Value );
            }
        }
    }
}
