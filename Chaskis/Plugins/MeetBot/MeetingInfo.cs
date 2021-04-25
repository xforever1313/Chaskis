//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;

namespace Chaskis.Plugins.MeetBot
{
    public class MeetingInfo : IMeetingInfo
    {
        // ---------------- Constructor ----------------

        public MeetingInfo()
        {
        }

        // ---------------- Properties ----------------

        /// <inheritdoc/>
        public string MeetingTopic { get; set; }

        /// <inheritdoc/>
        public DateTime StartTime { get; set; }

        /// <inheritdoc/>
        public string Channel { get; set; }

        /// <inheritdoc/>
        public string Owner { get; set; }
    }
}
