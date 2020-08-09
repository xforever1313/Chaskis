//
//          Copyright Seth Hendrick 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;

namespace Chaskis.Plugins.MeetBot
{
    public interface IMeetingInfo
    {
        string MeetingTopic { get; }

        DateTime StartTime { get; }

        string Channel { get; }

        /// <summary>
        /// Owner of the meeting.  Username is
        /// set to lowercase no matter what.
        /// </summary>
        string Owner { get; }
    }
}
