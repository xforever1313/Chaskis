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
    /// Configuration for setting up <see cref="SendCtcpVersionEventHandler"/>.
    /// </summary>
    public sealed class SendCtcpVersionEventConfig :
        BaseCoreEvent<SendCtcpVersionEventConfig, SendCtcpVersionEventHandlerAction, SendCtcpVersionEventArgs>
    {
        // ---------------- Constructor ----------------

        public SendCtcpVersionEventConfig()
        {
        }

        // ---------------- Functions ----------------

        public override SendCtcpVersionEventConfig Clone()
        {
            return (SendCtcpVersionEventConfig)this.MemberwiseClone();
        }

        protected override IEnumerable<string> ValidateChild()
        {
            // Nothing to validate.
            return null;
        }
    }
}
