//
//          Copyright Seth Hendrick 2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;

namespace Chaskis.Core
{
    public class MessageHandlerConfig : BasePrivateMessageConfig<MessageHandlerConfig, MessageHandlerAction>
    {
        // ---------------- Constructor ----------------

        public MessageHandlerConfig() :
            base()
        {
        }

        // ---------------- Properties ----------------

        // ---------------- Functions ----------------

        public override MessageHandlerConfig Clone()
        {
            return (MessageHandlerConfig)this.MemberwiseClone();
        }

        protected override IEnumerable<string> ValidateChild()
        {
            // Nothing to validate.
            return null;
        }
    }
}
