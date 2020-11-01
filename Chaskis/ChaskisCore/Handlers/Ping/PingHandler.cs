//
//          Copyright Seth Hendrick 2016-2017.
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
    public sealed class PingHandler : IIrcHandler
    {
        // -------- Fields --------

        /// <summary>
        /// Pattern to watch for to send a pong.
        /// </summary>
        private static readonly Regex pattern =
            new Regex(
                @"^PING\s*:(?<response>.+)",
                RegexOptions.Compiled
            );

        // -------- Constructor --------

        /// <summary>
        /// Constructs a ping handler.
        /// </summary>
        public PingHandler()
        {
            this.KeepHandling = true;
        }

        // -------- Properties --------

        /// <summary>
        /// Whether or not the handler should keep handling or not.
        /// Set to true to keep handling the event when it appears in the chat.
        /// Set to false so when the current IRC message is finished processing being,
        /// it leaves the event queue and never
        /// happens again.   Useful for events that only need to happen once.
        ///
        /// This is a public get/set.  Either classes outside of the handler can
        /// tell the handler to cancel the event, or it can cancel itself.
        ///
        /// Note: when this is set to false, there must be one more IRC message that appears
        /// before it is removed from the queue.
        ///
        /// Defaulted to true.
        /// </summary>
        public bool KeepHandling { get; set; }

        // -------- Functions --------

        /// <summary>
        /// Handles the event and the pong back to the server.
        /// </summary>
        public void HandleEvent( HandlerArgs args )
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