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
    /// Event to configure <see cref="SendKickEventHandler"/>
    /// </summary>
    public sealed class SendKickEventConfig :
        BaseCoreEvent<SendKickEventConfig, SendKickHandlerAction, SendKickEventArgs>
    {
        // ---------------- Constructor ----------------

        public SendKickEventConfig()
        {
        }

        // ---------------- Functions ----------------

        public override SendKickEventConfig Clone()
        {
            return (SendKickEventConfig)this.MemberwiseClone();
        }

        protected override IEnumerable<string> ValidateChild()
        {
            // Nothing to validate.
            return null;
        }
    }
}
