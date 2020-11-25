//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using SethCS.Basic;

namespace Chaskis.RegressionTests.TestCore
{
    /// <summary>
    /// This class contains common actions all tests of ANY project can use.
    /// </summary>
    public static class CommonActions
    {
        // ---------------- Fields ----------------

        private static readonly GenericLogger consoleOut;

        // ---------------- Constructor ----------------

        /// <summary>
        /// Constructor.
        /// </summary>
        static CommonActions()
        {
            consoleOut = Logger.GetConsoleOutLog();
        }

        // ---------------- Functions ----------------

        /// <summary>
        /// Sleeps for the specified amount of time.
        /// </summary>
        /// <param name="time">How long to sleep for in milliseconds.</param>
        /// <returns>Always returns true.</returns>
        public static bool Sleep( int time )
        {
            consoleOut.WriteLine( "Sleeping for " + time + "ms..." );
            Thread.Sleep( time );
            consoleOut.WriteLine( "Sleeping for " + time + "ms...Done!" );
            return true;
        }

        public static void WriteLine( string line )
        {
            consoleOut.WriteLine( line );
        }

        public static bool FileContains( string fileName, string regex )
        {
            if( File.Exists( fileName ) == false )
            {
                consoleOut.WriteLine( "File '{0}' does not exist!", fileName );
                return false;
            }

            string fileContents = File.ReadAllText( fileName );
            bool success = Regex.IsMatch( fileContents, regex );

            if( success == false )
            {
                consoleOut.WriteLine( "Could not match regex {0} with file contents {1}", regex, fileContents );
            }

            return success;
        }
    }
}
