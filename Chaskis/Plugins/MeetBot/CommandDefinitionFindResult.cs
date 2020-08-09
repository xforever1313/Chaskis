//
//          Copyright Seth Hendrick 2020.
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
    public class CommandDefinitionFindResult
    {
        // ---------------- Constructor ----------------

        public CommandDefinitionFindResult()
        {
        }

        // ---------------- Properties ----------------

        public string CommandPrefix { get; set; }

        /// <summary>
        /// Any arguments to the command.  Really, any text that comes after the <see cref="CommandPrefix"/>.
        /// <see cref="string.Empty"/> if no args were specified (only the prefix was).
        /// </summary>
        public string CommandArgs { get; set; }

        public CommandDefinition FoundDefinition { get; set; }
    }
}
