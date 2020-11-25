//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

namespace Chaskis.Core
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
    /// The verbosity level for writing to the Chaskis Log.
    /// </summary>
    public enum LogVerbosityLevel
    {
        /// <summary>
        /// At this level, the verbosity is set to 0.
        /// This means a WriteLine with this will ALWAYS be sent.
        /// </summary>
        NoVerbosity = 0,

        /// <summary>
        /// Low verbosity.
        /// This means a WriteLine with this will be sent if our verbosity is set
        /// to 1.
        /// 
        /// Useful for the occasional messages.  For example, a notification
        /// that a non-fatal operation failed and was ignored.
        /// </summary>
        LowVerbosity = 1,

        /// <summary>
        /// Medium verbosity
        /// This means a WriteLine with this will be sent if our verbosity is set
        /// to 2.
        /// 
        /// Useful for common notifications.
        /// </summary>
        MediumVerbosity = 2,

        /// <summary>
        /// Highest level of verbosity.
        /// 
        /// Should only be used in debug or a test environment, as
        /// these messages will appear... a lot...
        /// </summary>
        HighVerbosity = 3
    }
}