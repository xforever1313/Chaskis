//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

namespace Chaskis.Core
{
    /// <summary>
    /// Arguments that are passed in when <see cref="ReceiveHandler"/> is triggered.
    /// </summary>
    public class ReceiveHandlerArgs
    {
        // ---------------- Constructor ----------------

        public ReceiveHandlerArgs( IIrcWriter writer, string line )
        {
            this.Writer = writer;
            this.Line = line;
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// The Writer to use so we can respond to the All Event.
        /// </summary>
        public IIrcWriter Writer { get; private set; }

        /// <summary>
        /// The raw line that was read from the server.
        /// </summary>
        public string Line { get; private set; }
    }
}
