//
//          Copyright Seth Hendrick 2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

namespace Chaskis.Core
{
    /// <summary>
    /// This class contains strings that map to chaskis core events.
    /// </summary>
    public static class ChaskisCoreEvents
    {
        /// <summary>
        /// Event that is fired after the bot joins a channel.
        /// The channel that was joined comes after this string after 1 space.
        /// </summary>
        public static readonly string JoinChannel = "JOIN";

        /// <summary>
        /// Event that happens after the bot is done joining channels after it connected.
        /// </summary>
        public static readonly string FinishedJoiningChannels = "FINISHED JOINING CHANNELS";

        /// <summary>
        /// Event that happens if the watchdog timer fails.
        /// </summary>
        public static readonly string WatchdogFailed = "WATCHDOG FAILED";
    }
}
