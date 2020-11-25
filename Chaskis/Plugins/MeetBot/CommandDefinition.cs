//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using SethCS.Basic;

namespace Chaskis.Plugins.MeetBot
{
    public class CommandDefinition
    {
        // ---------------- Fields ----------------

        private readonly List<string> prefixes;

        // ---------------- Constructor ----------------

        public CommandDefinition()
        {
            this.prefixes = new List<string>();
            this.Prefixes = this.prefixes.AsReadOnly();

            this.MeetingAction = MeetingAction.Unknown;
            this.Restriction = CommandRestriction.Anyone;
            this.IsDefault = true;
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// The action to perform with this command.
        /// </summary>
        public MeetingAction MeetingAction { get; set; }

        /// <summary>
        /// Who is able to run this command?
        /// </summary>
        public CommandRestriction Restriction { get; set; }

        /// <summary>
        /// The prefix the triggers this command.
        /// </summary>
        public IEnumerable<string> Prefixes { get; private set; }

        /// <summary>
        /// The help text for this command.
        /// </summary>
        public string HelpText { get; set; }

        /// <summary>
        /// Is this a default command passed in by the compiled embedded resource (true)
        /// or was this user-overridden (false).
        /// </summary>
        public bool IsDefault { get; set; }

        // ---------------- Functions ----------------

        public void AddPrefix( string prefix )
        {
            if( string.IsNullOrWhiteSpace( prefix ) )
            {
                throw new ArgumentNullException( nameof( prefix ) );
            }

            this.prefixes.Add( prefix );
        }

        public string GetFullHelpText( string prefix )
        {
            if( string.IsNullOrWhiteSpace( prefix ) )
            {
                throw new ArgumentNullException( nameof( prefix ) );
            }

            string restrictionString;
            if( this.Restriction == CommandRestriction.ChairsOnly )
            {
                restrictionString = "  Chairs Only.";
            }
            else if( this.Restriction == CommandRestriction.ChairsAndBotAdmins )
            {
                restrictionString = "  Chairs and Bot Admins Only.";
            }
            else
            {
                restrictionString = string.Empty;
            }

            return $"'{prefix}': {this.HelpText}.{restrictionString}";
        }
    }

    public static class CommandDefinitionExtensions
    {
        // ---------------- Fields ----------------

        internal const string CommandXmlElementName = "command";

        private const string actionXmlAttrName = "action";
        private const string prefixXmlElementName = "prefix";
        private const string helpTextXmlElementName = "helptext";
        private const string restrictionXmlElementName = "restriction";

        // ---------------- Functions ----------------

        public static void FromXml( this CommandDefinition cmd, XElement commandElement, GenericLogger logger = null )
        {
            if( CommandXmlElementName.Equals( commandElement.Name.LocalName ) == false )
            {
                throw new ArgumentException(
                    $"Invalid XML element name.  Expected: {CommandXmlElementName}, Got: {commandElement.Name.LocalName}"
                );
            }

            foreach( XAttribute attribute in commandElement.Attributes() )
            {
                if( actionXmlAttrName.Equals( attribute.Name.LocalName ) )
                {
                    if( Enum.TryParse( attribute.Value.Trim(), out MeetingAction action ) )
                    {
                        cmd.MeetingAction = action;
                    }
                    else
                    {
                        cmd.MeetingAction = MeetingAction.Unknown;
                    }
                }
                else
                {
                    logger?.WarningWriteLine( "Unexpected Command Attribute: " + attribute.Name );
                }
            }

            foreach( XElement child in commandElement.Elements() )
            {
                if( prefixXmlElementName.Equals( child.Name.LocalName ) )
                {
                    cmd.AddPrefix( child.Value.Trim() );
                }
                else if( helpTextXmlElementName.Equals( child.Name.LocalName ) )
                {
                    cmd.HelpText = child.Value.Trim();
                }
                else if( restrictionXmlElementName.Equals( child.Name.LocalName ) )
                {
                    if( Enum.TryParse( child.Value.Trim(), out CommandRestriction restriction ) )
                    {
                        cmd.Restriction = restriction;
                    }
                    else
                    {
                        cmd.Restriction = CommandRestriction.Unknown;
                    }
                }
                else
                {
                    logger?.WarningWriteLine( "Unexpected Command Child: " + child.Name.LocalName );
                }
            }
        }

        /// <summary>
        /// Gets the regex that triggers the command.
        /// </summary>
        public static Regex GetPrefixRegex( this CommandDefinition cmd )
        {
            StringBuilder builder = new StringBuilder();
            foreach( string prefix in cmd.Prefixes )
            {
                builder.Append( prefix );
                builder.Append( "|" );
            }

            builder.Remove( builder.Length - 1, 1 );

            return new Regex(
                $@"^(?<command>{builder})(\s+(?<args>.+))?$",
                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture
            );
        }
    }
}
