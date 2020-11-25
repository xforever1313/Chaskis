//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

namespace Chaskis.Plugins.MeetBot
{
    /// <summary>
    /// A message that happen during a meeting.
    /// </summary>
    public interface IMeetingMessage
    {
        string CommandPrefix { get; }

        /// <summary>
        /// Any arguments to the command.  Really, any text that comes after the <see cref="CommandPrefix"/>.
        /// <see cref="string.Empty"/> if no args were specified (only the prefix was).
        /// </summary>
        string CommandArgs { get; }

        CommandRestriction Restriction { get; }

        MeetingAction MeetingAction { get; }
    }
}
