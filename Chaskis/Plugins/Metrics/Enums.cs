//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Text;

namespace Chaskis.Plugins.MetricsBot
{
    /// <summary>
    /// Messages we keep metrics of.
    /// These do not match up with the total number of message types
    /// <see cref="Chaskis"/> supports, rather the messages we keep metrics of.
    /// </summary>
    public enum MessageType
    {
        PrivMsg = 1,

        Notice = 2,

        Join = 3,

        Part = 4,

        Kick = 5,

        Quit = 6,

        Action = 7,

        CtcpPing = 8,

        CtcpPong = 9,

        CtcpVersionRequest = 10,

        CtcpVersionResponse = 11
    }

    /// <summary>
    /// Copy of <see cref="Core.ChaskisEventProtocol"/>,
    /// but independent so changes to <see cref="Core.ChaskisEventProtocol"/> don't
    /// impact this plugin.
    /// </summary>
    public enum Protocol
    {
        IRC = 1
    }
}
