//
//          Copyright Seth Hendrick 2016-2019.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Text;

namespace Chaskis.Core
{
    public delegate void ActionHandlerAction( ActionHandlerArgs response );

    [PrivateMessage( MessageRegexPattern )]
    public class ActionHandler
    {
        // ---------------- Fields ----------------

        internal const string MessageRegexPattern = "\u0001ACTION (?<message.+)\u0001";
    }
}
