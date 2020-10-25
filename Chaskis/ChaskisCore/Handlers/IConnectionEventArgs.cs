//
//          Copyright Seth Hendrick 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

namespace Chaskis.Core
{
    /// <summary>
    /// When the status of the bot being
    /// connected to a server changes,
    /// this class gets passed into
    /// event handlers as an argument.
    /// </summary>
    public interface IConnectionEventArgs
    {
        // ---------------- Properties ----------------

        /// <summary>
        /// The server that had its connection
        /// status changed.
        /// </summary>
        string Server { get; }

        ChaskisEventProtocol Protocol { get; }
    }
}
