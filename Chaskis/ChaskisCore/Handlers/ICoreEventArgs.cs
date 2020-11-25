//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

namespace Chaskis.Core
{
    /// <summary>
    /// When the core creates an event, these are arguments
    /// all events shall contain.
    /// </summary>
    public interface ICoreEventArgs
    {
        // ---------------- Properties ----------------

        /// <summary>
        /// The server that issued the event.
        /// </summary>
        string Server { get; }

        ChaskisEventProtocol Protocol { get; }
    }
}
