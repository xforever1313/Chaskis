//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using SethCS.Exceptions;

namespace Chaskis.Plugins.MeetBot
{
    public class CommandDefinitionCollection
    {
        // ---------------- Fields ----------------

        private readonly List<CommandDefinition> defs;

        private readonly Dictionary<Regex, CommandDefinition> regexDefs;
        private readonly Dictionary<MeetingAction, CommandDefinition> actionDefs;
        private readonly Dictionary<MeetingAction, Regex> actionRegex;

        // ---------------- Constructor ----------------

        public CommandDefinitionCollection( IEnumerable<CommandDefinition> defs )
        {
            this.defs = new List<CommandDefinition>( defs );
            this.CommandDefinitions = this.defs.AsReadOnly();

            this.regexDefs = new Dictionary<Regex, CommandDefinition>();
            this.RegexToCommandDef = new ReadOnlyDictionary<Regex, CommandDefinition>( this.regexDefs );

            this.actionDefs = new Dictionary<MeetingAction, CommandDefinition>();
            this.MeetingActionToCommandDef = new ReadOnlyDictionary<MeetingAction, CommandDefinition>( this.actionDefs );

            this.actionRegex = new Dictionary<MeetingAction, Regex>();
            this.MeetingActionToRegex = new ReadOnlyDictionary<MeetingAction, Regex>( this.actionRegex );
        }

        // ---------------- Properties ----------------

        public IReadOnlyList<CommandDefinition> CommandDefinitions { get; private set; }

        public IReadOnlyDictionary<Regex, CommandDefinition> RegexToCommandDef { get; private set; }

        public IReadOnlyDictionary<MeetingAction, CommandDefinition> MeetingActionToCommandDef { get; private set; }

        public IReadOnlyDictionary<MeetingAction, Regex> MeetingActionToRegex { get; private set; }

        // ---------------- Functions ----------------

        /// <summary>
        /// Ensures all commands are definied correctly from the XML.
        /// </summary>
        public void InitStage1_ValidateDefinitions()
        {
            List<string> errors = new List<string>();

            // Check for well-formed commands.

            foreach( CommandDefinition def in this.defs )
            {
                if( def.MeetingAction == MeetingAction.Unknown )
                {
                    errors.Add(
                        $"Found a comamnd definition with an {MeetingAction.Unknown} {nameof( def.MeetingAction )}"
                    );
                }

                if( def.Restriction == CommandRestriction.Unknown )
                {
                    errors.Add(
                        $"Found a comamnd definition with an {CommandRestriction.Unknown} {nameof( def.Restriction )}"
                    );
                }

                if( def.Prefixes.Count() == 0 )
                {
                    errors.Add(
                        $"Found a command definition with no {nameof( def.Prefixes )} defined"
                    );
                }

                foreach( string prefix in def.Prefixes )
                {
                    if( string.IsNullOrWhiteSpace( prefix ) )
                    {
                        errors.Add(
                            $"Found a command definition with a null, empty, or whitespace string in {nameof( def.Prefixes )}"
                        );
                    }
                }

                if( string.IsNullOrWhiteSpace( def.HelpText ) )
                {
                    errors.Add(
                        $"Found a command definition with a null, empty, or whitespace {nameof( def.HelpText )}"
                    );
                }
            }

            // Ensure each command exists at least once.
            foreach( MeetingAction action in Enum.GetValues( typeof( MeetingAction ) ) )
            {
                if( action == MeetingAction.Unknown )
                {
                    continue;
                }

                {
                    IEnumerable<CommandDefinition> defaultDefs = this.defs.Where(
                        c => ( c.MeetingAction == action ) && c.IsDefault
                    );

                    if( defaultDefs.Count() != 1 )
                    {
                        errors.Add(
                            $"Default command definition does not have {action} definied once, but {defaultDefs.Count()}"
                        );
                    }
                }

                {
                    IEnumerable<CommandDefinition> userDefs = this.defs.Where(
                        c => ( c.MeetingAction == action ) && ( c.IsDefault == false )
                    );

                    if( userDefs.Count() > 1 )
                    {
                        errors.Add(
                            $"User-definied command definition for {action} exists more than once.  Got: {userDefs.Count()}"
                         );
                    }
                }
            }

            if( errors.Count != 0 )
            {
                throw new ListedValidationException(
                    "Errors when validating all command definitions",
                    errors
                );
            }
        }

        /// <summary>
        /// Filters out any commands the user overridden.
        /// </summary>
        public void InitStage2_FilterOutOverrides()
        {
            IEnumerable<MeetingAction> overriddenActions = this.defs
                .Where( c => ( c.IsDefault == false ) )
                .Select( c => c.MeetingAction );

            this.defs.RemoveAll(
                c => overriddenActions.Contains( c.MeetingAction ) && c.IsDefault
            );

            // One last sanity check, make sure everything exists at least once.
            List<string> errors = new List<string>();
            foreach( MeetingAction action in Enum.GetValues( typeof( MeetingAction ) ) )
            {
                if( this.defs.Exists( c => c.MeetingAction == action ) == false )
                {
                    errors.Add(
                        $"Could not find command definition for {action} after filtering."
                    );
                }
            }
        }

        public void InitStage3_BuildDictionaries()
        {
            foreach( CommandDefinition commandDef in this.CommandDefinitions )
            {
                this.regexDefs.Add( commandDef.GetPrefixRegex(), commandDef );
                this.actionDefs.Add( commandDef.MeetingAction, commandDef );
            }

            foreach( KeyValuePair<Regex, CommandDefinition> pair in this.regexDefs )
            {
                this.actionRegex.Add( pair.Value.MeetingAction, pair.Key );
            }
        }

        /// <summary>
        /// Takes in a string to parse.  If the string
        /// matches a regex, the found <see cref="CommandDefinition"/> is returned.
        /// Otherwise, this returns null.
        /// </summary>
        public CommandDefinitionFindResult Find( string command )
        {
            foreach( KeyValuePair<Regex, CommandDefinition> def in this.regexDefs )
            {
                Match match = def.Key.Match( command );
                if( match.Success )
                {
                    CommandDefinitionFindResult result = new CommandDefinitionFindResult
                    {
                        CommandPrefix = match.Groups["command"].Value,
                        CommandArgs = match.Groups["args"].Value,
                        FoundDefinition = def.Value
                    };

                    return result;
                }
            }

            // Couldn't find anything, return null.
            return null;
        }
    }
}
