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
    /// Configuration for setting up <see cref="SendNoticeEventHandler"/>.
    /// </summary>
    public sealed class SendNoticeEventConfig :
        BaseCoreEvent<SendNoticeEventConfig, SendNoticeEventHandlerAction, SendNoticeEventArgs>
    {
        // ---------------- Constructor ----------------

        public SendNoticeEventConfig()
        {
        }

        // ---------------- Functions ----------------

        public override SendNoticeEventConfig Clone()
        {
            return (SendNoticeEventConfig)this.MemberwiseClone();
        }

        protected override IEnumerable<string> ValidateChild()
        {
            // Nothing to validate.
            return null;
        }
    }
}
