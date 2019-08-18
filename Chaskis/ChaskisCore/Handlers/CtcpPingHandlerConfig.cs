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
    public class CtcpPingHandlerConfig : BasePrivateMessageConfig<CtcpPingHandlerConfig, CtcpPingHandlerAction, CtcpPingHandlerArgs>
    {
        // ---------------- Constructor ----------------

        public CtcpPingHandlerConfig() :
            base()
        {
        }

        // ---------------- Properties ----------------

        // ---------------- Functions ----------------

        public override CtcpPingHandlerConfig Clone()
        {
            return (CtcpPingHandlerConfig)this.MemberwiseClone();
        }

        protected override IEnumerable<string> ValidateChild()
        {
            // Nothing to validate.
            return null;
        }
    }
}
