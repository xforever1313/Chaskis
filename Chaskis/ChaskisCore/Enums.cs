//
//          Copyright Seth Hendrick 2016-2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

namespace ChaskisCore
{
    /// <summary>
    /// Options for whom the bot will respond to.
    /// </summary>
    public enum ResponseOptions
    {
        /// <summary>
        /// The bot will only respond to Private messages.
        /// </summary>
        PmsOnly,

        /// <summary>
        /// The bot will only respond to messages in the channel its listening on
        /// (ignores Private Messages).
        /// </summary>
        ChannelOnly,

        /// <summary>
        /// The bot will respond to both.
        /// </summary>
        ChannelAndPms
    }

    // ---------------- Chaskis Event Enums ----------------

    /// <summary>
    /// The protocol we are using during a chaskis event.
    /// </summary>
    public enum ChaskisEventProtocol
    {
        IRC
    }

    /// <summary>
    /// Where the event was fired.
    /// </summary>
    public enum ChaskisEventSource
    {
        /// <summary>
        /// The event was fired from ChaskisCore.
        /// </summary>
        CORE,

        /// <summary>
        /// The event was fired from a plugin.
        /// </summary>
        PLUGIN
    }
}