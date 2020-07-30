//
//          Copyright Seth Hendrick 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using SethCS.Exceptions;

namespace Chaskis.Plugins.MeetBot
{
    public class CommandDefinitionCollection
    {
        // ---------------- Fields ----------------

        private readonly List<CommandDefinition> defs;

        // ---------------- Constructor ----------------

        public CommandDefinitionCollection( IEnumerable<CommandDefinition> defs )
        {
            this.defs = new List<CommandDefinition>( defs );
            this.CommandDefinitions = this.defs.AsReadOnly();
        }

        // ---------------- Properties ----------------

        public IReadOnlyList<CommandDefinition> CommandDefinitions { get; private set; }

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
    }
}
