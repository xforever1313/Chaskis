
//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

namespace GenericIrcBot
{
    /// <summary>
    /// Options for whom the bot will respond to.
    /// </summary>
    public enum ResponseOptions
    {
        /// <summary>
        /// The bot will only respond to Private messages.
        /// </summary>
        RespondOnlyToPMs,

        /// <summary>
        /// The bot will only respond to messages in the channel its listening on
        /// (ignores Private Messages).
        /// </summary>
        RespondOnlyToChannel,

        /// <summary>
        /// The bot will respond to both.
        /// </summary>
        RespondToBoth
    }
}
