//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;

namespace Chaskis.Core
{
    public class ActionHandlerConfig : BasePrivateMessageConfig<ActionHandlerConfig, ActionHandlerAction, ActionHandlerArgs>
    {
        // ---------------- Constructor ----------------

        public ActionHandlerConfig() :
            base()
        {
        }

        // ---------------- Properties ----------------

        // ---------------- Functions ----------------

        public override ActionHandlerConfig Clone()
        {
            return (ActionHandlerConfig)this.MemberwiseClone();
        }

        protected override IEnumerable<string> ValidateChild()
        {
            // Nothing to validate.
            return null;
        }
    }
}
