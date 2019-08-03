using System;
using System.Data;
using System.Text.RegularExpressions;

namespace Chaskis.Plugins.MathBot
{
    /// <summary>
    /// This is the class tha actually does the calculating from the user.
    /// </summary>
    public static class MathBotCalculator
    {
        // -------- Fields --------

        /// <summary>
        /// Regex of what is allowed to be passed to the calculator (ignoring case).
        /// Whitespace before and after the regex is allowed.
        /// The ^ and $ are there at the start and end so we don't get something like Derp 3 + 3, which calculates to 6
        /// (which is wrong).
        /// These are the only things allowed since DataTable allows for a lot more things that are way outside the
        /// scope of this bot (See: https://msdn.microsoft.com/en-us/library/system.data.datacolumn.expression(v=vs.110).aspx)
        /// </summary>
        public static readonly Regex calculatorRegex = new Regex(
            @"^\s*(TRUE|FALSE|AND|OR|NOT| |[0-9\+\-*/% <>=!.()])+\s*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        /// <summary>
        /// Table used to compute the expression.
        /// </summary>
        private static readonly DataTable table = new DataTable();

        /// <summary>
        /// Calculates the given string and returns a string of what its
        /// equal to.
        /// This will throw exceptions in the case of syntax errors, or the string
        /// does not match the regex.  Anything .net's DataTable.Compute will throw, so will this.
        /// </summary>
        /// <param name="expression">The expression to calculate.</param>
        /// <returns>A string of the result of the expression.</returns>
        public static string Calculate( string expression )
        {
            Match match = calculatorRegex.Match( expression );
            if( match.Success == false )
            {
                throw new ArgumentException(
                    expression + " is not a valid expression for MathBot.  Syntax Error."
                );
            }

            // In DataTable, <> is the same as !=.  We should support both.
            expression = expression.Replace( "!=", "<>" );

            // In datatable, there is operator==, its just '='.
            expression = expression.Replace( "==", "=" );

            return table.Compute( expression, string.Empty ).ToString();
        }
    }
}