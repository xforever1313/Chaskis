//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

namespace Chaskis.Plugins.MeetBot
{
    /// <summary>
    /// Result of finding a <see cref="CommandDefinition"/>
    /// inside of a <see cref="CommandDefinitionCollection"/>
    /// </summary>
    public class CommandDefinitionFindResult : IMeetingMessage
    {
        // ---------------- Constructor ----------------

        public CommandDefinitionFindResult()
        {
        }

        // ---------------- Properties ----------------

        public string CommandPrefix { get; set; }

        /// <inheritdoc/>
        public string CommandArgs { get; set; }

        public CommandDefinition FoundDefinition { get; set; }

        /// <summary>
        /// Helper to get <see cref="CommandDefinition.Restriction"/>
        /// </summary>
        public CommandRestriction Restriction => FoundDefinition.Restriction;

        /// <summary>
        /// Helper to get <see cref="CommandDefinition.MeetingAction"/>
        /// </summary>
        public MeetingAction MeetingAction => FoundDefinition.MeetingAction;
    }
}
