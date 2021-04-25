//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;

namespace Chaskis.Core
{
    /// <summary>
    /// Object to configure <see cref="WatchdogFailedEventHandler"/>
    /// </summary>
    public class WatchdogFailedEventConfig :
        BaseCoreEvent<WatchdogFailedEventConfig, WatchdogFailedHandlerAction, WatchdogFailedEventArgs>
    {
        // ---------------- Constructor ----------------

        public WatchdogFailedEventConfig()
        {
        }

        // ---------------- Functions ----------------

        public override WatchdogFailedEventConfig Clone()
        {
            return (WatchdogFailedEventConfig)this.MemberwiseClone();
        }

        protected override IEnumerable<string> ValidateChild()
        {
            // Nothing to validate.
            return null;
        }
    }
}
