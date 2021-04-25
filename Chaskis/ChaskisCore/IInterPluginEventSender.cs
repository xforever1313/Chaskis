//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

namespace Chaskis.Core
{
    /// <summary>
    /// Interface that allows one to send chaskis events
    /// to other plugins.
    /// </summary>
    public interface IInterPluginEventSender
    {
        /// <summary>
        /// Sends a chaskis event to plugins
        /// loaded into the bot.
        /// </summary>
        void SendInterPluginEvent( InterPluginEvent e );
    }
}
