//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Text;

namespace Chaskis.Plugins.QuoteBot
{
    /// <summary>
    /// Represents a quote.
    /// </summary>
    public class Quote
    {
        // ---------------- Constructor ----------------

        public Quote()
        {
            this.Author = string.Empty;
            this.QuoteText = string.Empty;
            this.Adder = string.Empty;
            this.TimeStamp = DateTime.MinValue;
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// The ID of the quote in the database.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Who said the quote.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// The text of the quote.
        /// </summary>
        public string QuoteText { get; set; }

        /// <summary>
        /// The timestamp of the quote.
        /// 
        /// Defaulted to DateTime.MinValue.
        /// </summary>
        public DateTime TimeStamp { get; set; }

        /// <summary>
        /// The person who added the quote.
        /// </summary>
        public string Adder { get; set; }

        // ---------------- Functions ----------------

        /// <summary>
        /// Ensures our config is valid before going into the database.
        /// </summary>
        /// <param name="errorString">Why we failed validation.</param>
        /// <returns>True if we are valid, else false.</returns>
        public bool TryValidate( out string errorString )
        {
            bool success = true;

            StringBuilder builder = new StringBuilder();
            builder.AppendLine( "Could not validate Quote for the following reasons:" );

            if( string.IsNullOrEmpty( this.Author ) || string.IsNullOrWhiteSpace( this.Author ) )
            {
                success = false;
                builder.AppendLine( "\t-Author can not be null, empty, or whitespace." );
            }

            if( string.IsNullOrEmpty( this.QuoteText ) || string.IsNullOrWhiteSpace( this.QuoteText ) )
            {
                success = false;
                builder.AppendLine( "\t-Quote Text can not be null, empty, or whitespace." );
            }

            if( string.IsNullOrEmpty( this.Adder ) || string.IsNullOrWhiteSpace( this.Adder ) )
            {
                success = false;
                builder.AppendLine( "\t-Adder can not be null, empty, or whitespace." );
            }

            if( success )
            {
                errorString = string.Empty;
            }
            else
            {
                errorString = builder.ToString();
            }

            return success;
        }

        public override string ToString()
        {
            return string.Format(
                "'{0}' -{1}. Added by {2} on {3}.",
                this.QuoteText,
                this.Author,
                this.Adder,
                this.TimeStamp.ToShortDateString()
            );
        }

        public Quote Clone()
        {
            return (Quote)this.MemberwiseClone();
        }
    }
}
