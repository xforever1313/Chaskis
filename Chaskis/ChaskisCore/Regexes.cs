//
//          Copyright Seth Hendrick 2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

namespace ChaskisCore
{
    /// <summary>
    /// Useful Regexes that can be used.
    /// </summary>
    public static class Regexes
    {
        /// <summary>
        /// Pattern that can be used to capture Chaskis
        /// IRC connect events.
        /// Groups:
        ///     server - the Server we connected to
        ///     nick - The user name we connected as.
        /// </summary>
        public const string ChaskisIrcConnectEvent =
            @"CONNECT\s+TO\s+(?<server>\S+)\s+AS\s+(?<nick>\S+)";

        /// <summary>
        /// Pattern that can be used to capture Chaskis
        /// IRC disconnect events.
        /// Groups:
        ///     server - the Server we disconnected from
        ///     nick - The user name we disconnected as.
        /// </summary>
        public const string ChaskisIrcDisconnectEvent =
            @"DISCONNECT\s+FROM\s+(?<server>\S+)\s+AS\s+(?<nick>\S+)";
    }
}
