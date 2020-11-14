//
//          Copyright Seth Hendrick 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

namespace Chaskis.Core
{
    /// <summary>
    /// Arguments that are passed in when <see cref="AnyChaskisEventHandler"/> is triggered.
    /// </summary>
    public class AnyChaskisEventHandlerArgs
    {
        // ---------------- Constructor ----------------

        public AnyChaskisEventHandlerArgs( IIrcWriter writer, string line )
        {
            this.Writer = writer;
            this.Line = line;
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// The Writer to use so we can respond to the chaskis event.
        /// </summary>
        public IIrcWriter Writer { get; private set; }

        /// <summary>
        /// The raw line that was read from the server.
        /// </summary>
        public string Line { get; private set; }
    }
}
