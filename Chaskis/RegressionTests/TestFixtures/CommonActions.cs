//
//          Copyright Seth Hendrick 2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

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
    }
}
