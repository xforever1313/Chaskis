//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;

namespace Chaskis.Core
{
    /// <remarks>
    /// By default, <see cref="BasePrivateMessageConfig{TChildType, TLineActionType, TLineActionArgs}.LineRegex"/>
    /// does not need to be set if we are just looking for VERSION.  VERSION does not usually have
    /// arguments after it.  However, if for some reason we expect arguments to come after VERSION,
    /// override <see cref="BasePrivateMessageConfig{TChildType, TLineActionType, TLineActionArgs}.LineRegex"/>.
    /// 
    /// If fact, by default, the regex is include anything after VERSION optionally.
    /// </remarks>
    public class CtcpVersionHandlerConfig : BasePrivateMessageConfig<CtcpVersionHandlerConfig, CtcpVersionHandlerAction, CtcpVersionHandlerArgs>
    {
        // ---------------- Constructor ----------------

        public CtcpVersionHandlerConfig() :
            base()
        {
            // By default, only respond to PM's.
            this.ResponseOption = ResponseOptions.PmsOnly;

            // By default, expect any message.  Including empty strings.
            this.LineRegex = ".*";
        }

        // ---------------- Functions ----------------

        public override CtcpVersionHandlerConfig Clone()
        {
            return (CtcpVersionHandlerConfig)this.MemberwiseClone();
        }

        protected override IEnumerable<string> ValidateChild()
        {
            // Nothing to validate.
            return null;
        }
    }
}
