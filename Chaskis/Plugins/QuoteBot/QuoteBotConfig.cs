//
//          Copyright Seth Hendrick 2017-2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Text;
using SethCS.Exceptions;

namespace Chaskis.Plugins.QuoteBot
{
    public class QuoteBotConfig : IEquatable<QuoteBotConfig>
    {
        // ---------------- Constructor ----------------

        /// <summary>
        /// Constructor.  Sets everything to the default values.
        /// </summary>
        public QuoteBotConfig()
        {
            this.AddCommand = @"^!quote\s+add\s+\<(?<user>\S+)\>\s+(?<quote>.+)";
            this.DeleteCommand = @"^!quote\s+delete\s+(?<id>\d+)";
            this.RandomCommand = @"^!quote\s+random";
            this.GetCommand = @"^!quote\s+(get)?\s*(?<id>\d+)";
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// Regex used to add a quote.
        /// Must be a regex that contains groups "user" and "quote".
        /// </summary>
        public string AddCommand { get; set; }

        /// <summary>
        /// The command to delete a quote from the database based on 
        /// its ID. 
        /// Must be a regex that contains an "id" group.
        /// </summary>
        public string DeleteCommand { get; set; }

        /// <summary>
        /// Regex to tell the bot to post a 
        /// random quote from the database.
        /// </summary>
        public string RandomCommand { get; set; }

        /// <summary>
        /// The command used to get a quote with a specific ID 
        /// from the database, and post it to the channel.
        /// Must be a regex that contains an "id" group.
        /// </summary>
        public string GetCommand { get; set; }

        // ---------------- Functions ----------------

        /// <summary>
        /// Validates this configuration.
        /// </summary>
        public void Validate()
        {
            StringBuilder errorString = new StringBuilder();
            errorString.AppendLine( "QuoteBot configuration has validation errors:" );

            bool success = true;

            if( string.IsNullOrEmpty( this.AddCommand ) || string.IsNullOrWhiteSpace( this.AddCommand ) )
            {
                success = false;
                errorString.AppendLine( "\t-Add command can not be null, empty, or whitespace." );
            }

            if( string.IsNullOrEmpty( this.DeleteCommand ) || string.IsNullOrWhiteSpace( this.DeleteCommand ) )
            {
                success = false;
                errorString.AppendLine( "\t-Delete command can not be null, empty, or whitespace." );
            }

            if( string.IsNullOrEmpty( this.RandomCommand ) || string.IsNullOrWhiteSpace( this.RandomCommand ) )
            {
                success = false;
                errorString.AppendLine( "\t-Random command can not be null, empty, or whitespace." );
            }

            if( string.IsNullOrEmpty( this.GetCommand ) || string.IsNullOrWhiteSpace( this.GetCommand ) )
            {
                success = false;
                errorString.AppendLine( "\t-Get command can not be null, empty, or whitespace." );
            }

            if( success == false )
            {
                throw new ValidationException( errorString.ToString() );
            }
        }

        public override bool Equals( object obj )
        {
            QuoteBotConfig other = obj as QuoteBotConfig;
            return this.Equals( other );
        }

        public bool Equals( QuoteBotConfig other )
        {
            if( other == null )
            {
                return false;
            }

            return
               ( this.AddCommand == other.AddCommand ) &&
               ( this.DeleteCommand == other.DeleteCommand ) &&
               ( this.RandomCommand == other.RandomCommand ) &&
               ( this.GetCommand == other.GetCommand );
        }

        public override int GetHashCode()
        {
            return
                this.AddCommand.GetHashCode() +
                this.DeleteCommand.GetHashCode() +
                this.RandomCommand.GetHashCode() +
                this.GetCommand.GetHashCode();
        }

        /// <summary>
        /// Creates a copy of this object.
        /// </summary>
        public QuoteBotConfig Clone()
        {
            return (QuoteBotConfig)this.MemberwiseClone();
        }
    }
}
