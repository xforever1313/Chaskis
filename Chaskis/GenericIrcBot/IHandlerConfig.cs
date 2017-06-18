//
//          Copyright Seth Hendrick 2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;

namespace ChaskisCore
{
    /// <summary>
    /// Interface for a handler config.
    /// </summary>
    public interface IHandlerConfig
    {
        /// <summary>
        /// List of channels the handlers are ignored in. 
        /// </summary>
        IList<string> BlackListedChannels { get; }

        /// <summary>
        /// Gets the the handlers associated with this config.
        /// </summary>
        IList<IIrcHandler> Handlers { get; }
    }
}
