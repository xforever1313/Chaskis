//
//          Copyright Seth Hendrick 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;

namespace Chaskis.Plugins.MeetBot
{
    public interface IReadOnlyMeetingNote
    {
        /// <summary>
        /// The timestamp of the action.
        /// </summary>
        DateTime TimeStamp { get; }

        /// <summary>
        /// The meeting action.  Set to <see cref="MeetingAction.Unknown"/>
        /// if it just a standard message.
        /// </summary>
        MeetingAction MeetingAction { get; }

        /// <summary>
        /// The username who send the message.
        /// </summary>
        string UserName { get; }

        /// <summary>
        /// The message that was said.
        /// </summary>
        string Message { get; }
    }

    /// <summary>
    /// A single message that happened during the meeting.
    /// </summary>
    public class MeetingNote : IReadOnlyMeetingNote
    {
        // ---------------- Constructor ----------------

        public MeetingNote()
        {
        }

        // ---------------- Properties ----------------

        /// <inheritdoc/>
        public DateTime TimeStamp { get; set; }

        /// <inheritdoc/>
        public MeetingAction MeetingAction { get; set; }

        /// <inheritdoc/>
        public string UserName { get; set; }

        /// <inheritdoc/>
        public string Message { get; set; }
    }
}
