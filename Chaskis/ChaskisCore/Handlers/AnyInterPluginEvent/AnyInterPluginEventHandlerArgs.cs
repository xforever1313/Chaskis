//
//          Copyright Seth Hendrick 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

namespace Chaskis.Core
{
    /// <summary>
    /// Arguments that are passed in when <see cref="AnyInterPluginEventHandler"/> is triggered.
    /// </summary>
    public class AnyInterPluginEventHandlerArgs
    {
        // ---------------- Constructor ----------------

        public AnyInterPluginEventHandlerArgs( IIrcWriter writer, string line )
        {
            this.Writer = writer;
            this.Line = line;
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// The Writer to use so we can respond to the event.
        /// </summary>
        public IIrcWriter Writer { get; private set; }

        /// <summary>
        /// The raw line that was received.
        /// </summary>
        public string Line { get; private set; }
    }
}
