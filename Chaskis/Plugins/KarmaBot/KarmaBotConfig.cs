//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using SethCS.Exceptions;

namespace Chaskis.Plugins.KarmaBot
{
    /// <summary>
    /// Configuration for karma bot.
    /// </summary>
    public class KarmaBotConfig
    {
        // -------- Constructor --------

        /// <summary>
        /// Constructor.  Sets all properties to default values.
        /// </summary>
        public KarmaBotConfig()
        {
            this.IncreaseCommandRegex = @"^((\+\+(?<name>\S+))|((?<name>\S+)\+\+))";
            this.DecreaseCommandRegex = @"^((--(?<name>\S+))|((?<name>\S+)--))";
            this.QueryCommand = @"^!karma\s+(?<name>\S+)";
        }

        // -------- Properties --------

        /// <summary>
        /// Command to increase karma.  Must include a regex group
        /// labeled "name".  Defaulted to ^(++(?<name>\S+))|((?<name>\S+)++)
        /// </summary>
        public string IncreaseCommandRegex { get; set; }

        /// <summary>
        /// Command to decrase karma.  Must include a regex group
        /// labeled "name".  Defaulted to ^(--(?<name>\S+))|((?<name>\S+)--)
        /// </summary>
        public string DecreaseCommandRegex { get; set; }

        /// <summary>
        /// Command to query for a user's karma.
        /// Must include a regex group labeled "name".
        /// Defaulted to ^!karma\s+(?<name>\S+)
        /// </summary>
        public string QueryCommand { get; set; }

        // -------- Functions --------

        /// <summary>
        /// Ensures this class is in a correct state.
        /// </summary>
        /// <exception cref="ValidationException">Thrown when this does not validate.</exception>
        public void Validate()
        {
            string errors = "Can not validate this KarmaBotConfig object:" + Environment.NewLine;
            bool success = true;

            if( string.IsNullOrEmpty( this.IncreaseCommandRegex ) )
            {
                errors += "Increase command can not be null or empty." + Environment.NewLine;
                success = false;
            }
            else if( this.IncreaseCommandRegex.Contains( "<name>" ) == false )
            {
                errors += "Increase command must contain the regex group 'name'.  Got: " + this.IncreaseCommandRegex + Environment.NewLine;
                success = false;
            }

            if( string.IsNullOrEmpty( this.DecreaseCommandRegex ) )
            {
                errors += "Decrease command can not be null or empty." + Environment.NewLine;
                success = false;
            }
            else if( this.DecreaseCommandRegex.Contains( "<name>" ) == false )
            {
                errors += "Decrease command must contain the regex group 'name'.  Got: " + this.IncreaseCommandRegex + Environment.NewLine;
                success = false;
            }

            if( string.IsNullOrEmpty( this.QueryCommand ) )
            {
                errors += "Query command can not be null or empty." + Environment.NewLine;
                success = false;
            }
            else if( this.QueryCommand.Contains( "<name>" ) == false )
            {
                errors += "Query command must contain the regex group 'name'.  Got: " + this.IncreaseCommandRegex + Environment.NewLine;
                success = false;
            }

            if( success == false )
            {
                throw new ValidationException( errors );
            }
        }
    }
}