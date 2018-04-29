//
//          Copyright Seth Hendrick 2017-2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using NetRunner.ExternalLibrary;
using SethCS.Basic;

namespace Chaskis.RegressionTests
{
    /// <summary>
    /// This class contains common actions all tests of ANY project can use.
    /// </summary>
    public class CommonActions : BaseTestContainer
    {
        // ---------------- Fields ----------------

        private GenericLogger consoleOut;

        // ---------------- Constructor ----------------

        /// <summary>
        /// Constructor.
        /// </summary>
        public CommonActions()
        {
            this.consoleOut = Logger.GetConsoleOutLog();
        }

        // ---------------- Functions ----------------

        /// <summary>
        /// Sleeps for the specified amount of time.
        /// </summary>
        /// <param name="time">How long to sleep for in milliseconds.</param>
        /// <returns>Always returns true.</returns>
        public bool Sleep( int time )
        {
            this.consoleOut.WriteLine( "Sleeping for " + time + "ms..." );
            Thread.Sleep( time );
            this.consoleOut.WriteLine( "Sleeping for " + time + "ms...Done!" );
            return true;
        }

        public void WriteLine( string line )
        {
            this.consoleOut.WriteLine( line );
        }

        public bool FileContains( string fileName, string regex )
        {
            if( File.Exists( fileName ) == false )
            {
                this.consoleOut.WriteLine( "File '{0}' does not exist!", fileName );
                return false;
            }

            string fileContents = File.ReadAllText( fileName );
            bool success = Regex.IsMatch( fileContents, regex );

            if( success == false )
            {
                this.consoleOut.WriteLine( "Could not match regex {0} with file contents {1}", regex, fileContents );
            }

            return success;
        }
    }
}
