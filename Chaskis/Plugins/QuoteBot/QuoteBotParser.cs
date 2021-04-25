//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Text.RegularExpressions;
using SethCS.Exceptions;

namespace Chaskis.Plugins.QuoteBot
{
    public class QuoteBotParser
    {
        // ---------------- Fields ----------------

        private readonly Regex addRegex;
        private readonly Regex deleteRegex;
        private readonly Regex randomRegex;
        private readonly Regex getRegex;

        // ---------------- Constructor ----------------

        public QuoteBotParser( QuoteBotConfig config )
        {
            ArgumentChecker.IsNotNull( config, nameof( config ) );

            this.addRegex = new Regex( config.AddCommand, RegexOptions.Compiled );
            this.deleteRegex = new Regex( config.DeleteCommand, RegexOptions.Compiled );
            this.randomRegex = new Regex( config.RandomCommand, RegexOptions.Compiled );
            this.getRegex = new Regex( config.GetCommand, RegexOptions.Compiled );
        }

        // ---------------- Functions ----------------

        /// <summary>
        /// Tries to parse the given add command.
        /// </summary>
        /// <param name="str">The string to parse.</param>
        /// <param name="adder">IRC user who requested the quote be added.</param>
        /// <param name="quote">The parsed quote as a Quote object (ID is set to null).  Null if not successful.</param>
        /// <param name="errorString">Reason why the quote did not parse (if any).</param>
        /// <returns>True if we were able to parse the command, else false.</returns>
        public bool TryParseAddCommand( string str, string adder, out Quote quote, out string errorString )
        {
            Match match = this.addRegex.Match( str );
            if( match.Success == false )
            {
                quote = null;
                errorString = "Passed in string does not match add command regex.";
                return false;
            }

            Quote tempQuote = new Quote();
            tempQuote.Author = match.Groups["user"].Value;
            tempQuote.QuoteText = match.Groups["quote"].Value;
            tempQuote.Adder = adder;
            tempQuote.TimeStamp = DateTime.UtcNow;

            bool success = tempQuote.TryValidate( out errorString );
            if( success )
            {
                quote = tempQuote;
            }
            else
            {
                quote = null;
            }

            return success;
        }

        /// <summary>
        /// Tries to parse the given delete command.
        /// </summary>
        /// <param name="str">The string to parse.</param>
        /// <param name="quoteId">The parsed quote ID to delete. -1 if not successful.</param>
        /// <param name="errorString">Reason why we didn't parse.</param>
        /// <returns>True if we were able to parse the command, else false.</returns>
        public bool TryParseDeleteCommand( string str, out int quoteId, out string errorString )
        {
            Match match = this.deleteRegex.Match( str );
            return this.TryParseId( str, match, "delete", out quoteId, out errorString );
        }

        /// <summary>
        /// Tries to parse the random command.
        /// </summary>
        /// <param name="str">The string to parse.</param>
        /// <param name="errorString">Reason why we didn't parse.</param>
        /// <returns>True if we were able to parse the command, else false.</returns>
        public bool TryParseRandomCommand( string str, out string errorString )
        {
            Match match = this.randomRegex.Match( str );
            if( match.Success )
            {
                errorString = string.Empty;
            }
            else
            {
                errorString = "Passed in string does not match random command regex.";
            }

            return match.Success;
        }

        /// <summary>
        /// Tries to parse the given get command.
        /// </summary>
        /// <param name="str">The string to parse.</param>
        /// <param name="quoteId">The parsed quote ID to get. -1 if not successful.</param>
        /// <param name="errorString">Reason why we didn't parse.</param>
        /// <returns>True if we were able to parse the command, else false.</returns>
        public bool TryParseGetCommand( string str, out int quoteId, out string errorString )
        {
            Match match = this.getRegex.Match( str );
            return this.TryParseId( str, match, "get", out quoteId, out errorString );
        }

        private bool TryParseId( string str, Match match, string context, out int quoteId, out string errorString )
        {
            if( match.Success == false )
            {
                quoteId = -1;
                errorString = "Passed in string does not match " + context + " command regex.";
                return false;
            }

            bool success = int.TryParse( match.Groups["id"].Value, out quoteId );

            if( success )
            {
                success = quoteId >= 0;
                if( success )
                {
                    errorString = string.Empty;
                }
                else
                {
                    errorString = "ID can not be negative!";
                    quoteId = -1;
                }
            }
            else
            {
                errorString = "ID is not a valid int.  Got: " + match.Groups["id"].Value;
                quoteId = -1;
            }

            return success;
        }
    }
}
