
//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

namespace GenericIrcBot
{
    /// <summary>
    /// Handles irc messages from the watched channel
    /// </summary>
    public interface IIrcHandler
    {
        /// <summary>
        /// Handles the event and sends the responses to the channel if desired.
        /// </summary>
        /// <param name="line">The RAW line from IRC to check.</param>
        /// <param name="ircConfig">The irc config to use when parsing this line.</param>
        /// <param name="ircWriter">The way to write to the irc channel.</param>
        void HandleEvent( string line, IIrcConfig ircConfig, IIrcWriter ircWriter );
    }
}
