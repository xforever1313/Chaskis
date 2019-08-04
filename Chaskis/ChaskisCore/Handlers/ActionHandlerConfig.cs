﻿//
//          Copyright Seth Hendrick 2016-2019.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

namespace Chaskis.Core
{
    public class ActionHandlerConfig : BasePrivateMessageConfig<ActionHandlerConfig, ActionHandlerAction>
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
    }
}
