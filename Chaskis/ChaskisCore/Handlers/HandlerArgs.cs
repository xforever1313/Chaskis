//
//          Copyright Seth Hendrick 2016-2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;

namespace Chaskis.Core
{
    /// <summary>
    /// This class is arguments that are passed in to our
    /// handlers.
    /// 
    /// Used as a class so if we need to add one, we just need to add a property to this class
    /// and not update everything that implements <see cref="IIrcHandler"/>
    /// </summary>
    public class HandlerArgs
    {
        // ---------------- Constructor ----------------

        /// <summary>
        /// Constructor.
        /// </summary>
        public HandlerArgs()
        {
            this.BlackListedChannels = new List<string>();
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// Line from the server
        /// </summary>
        public string Line { get; set; }

        /// <summary>
        /// The associated IRC config.
        /// </summary>
        public IIrcConfig IrcConfig { get; set; }

        /// <summary>
        /// The writer we can use to write to the server.
        /// </summary>
        public IIrcWriter IrcWriter { get; set; }

        /// <summary>
        /// Channels that are black listed for the handler.
        /// </summary>
        public IList<string> BlackListedChannels { get; set; }
    }
}
