//
//          Copyright Seth Hendrick 2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

namespace Chaskis.Plugins.MessageFixerBot
{
    /// <summary>
    /// Result from our message fixer.
    /// </summary>
    public class MessageFixerResult
    {
        // ---------------- Constructor ----------------

        public MessageFixerResult( bool success, string message )
        {
            this.Success = success;
            this.Message = message;
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// 
        /// </summary>
        public bool Success { get; private set; }

        /// <summary>
        /// The message to send.
        /// If <see cref="Success"/> is true, it means
        /// we are able to fix the message.  If false,
        /// the user made a mistake, and this message
        /// contains that mistake.
        /// 
        /// This is empty if there is nothing to report.
        /// </summary>
        public string Message { get; private set; }
    }
}
