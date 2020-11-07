//
//          Copyright Seth Hendrick 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;

namespace Chaskis.Core
{
    /// <summary>
    /// Configuration for setting up <see cref="SendCtcpPongEventHandler"/>.
    /// </summary>
    public sealed class SendCtcpPongEventConfig :
        BaseCoreEvent<SendCtcpPongEventConfig, SendCtcpPongEventHandlerAction, SendCtcpPongEventArgs>
    {
        // ---------------- Constructor ----------------

        public SendCtcpPongEventConfig()
        {
        }

        // ---------------- Functions ----------------

        public override SendCtcpPongEventConfig Clone()
        {
            return (SendCtcpPongEventConfig)this.MemberwiseClone();
        }

        protected override IEnumerable<string> ValidateChild()
        {
            // Nothing to validate.
            return null;
        }
    }
}
