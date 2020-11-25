//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;

namespace Chaskis.Core
{
    /// <summary>
    /// Object to configure <see cref="ReconnectingEventHandler"/>
    /// </summary>
    public class ReconnectingEventConfig :
        BaseCoreEvent<ReconnectingEventConfig, ReconnectingHandlerAction, ReconnectingEventArgs>
    {
        // ---------------- Constructor ----------------

        public ReconnectingEventConfig()
        {
        }

        // ---------------- Functions ----------------

        public override ReconnectingEventConfig Clone()
        {
            return (ReconnectingEventConfig)this.MemberwiseClone();
        }

        protected override IEnumerable<string> ValidateChild()
        {
            // Nothing to validate.
            return null;
        }
    }
}
