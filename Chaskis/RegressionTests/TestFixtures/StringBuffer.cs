//
//          Copyright Seth Hendrick 2016-2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SethCS.Basic;

namespace Chaskis.RegressionTests
{
    /// <summary>
    /// This class enqueues strings and has methods that search for strings.
    /// As it searches, it throws out strings we don't need.
    /// 
    /// Thread-safe.
    /// </summary>
    public class StringBuffer
    {
        // ---------------- Fields ----------------

        private ConcurrentQueue<string> buffer;

        // ---------------- Constructor ----------------

        public StringBuffer()
        {
            this.buffer = new ConcurrentQueue<string>();
        }

        // ---------------- Functions ----------------

        /// <summary>
        /// Enqueues the given string to the buffer.
        /// </summary>
        public void EnqueueString( string str )
        {
            this.buffer.Enqueue( str );
        }

        /// <summary>
        /// Waits for a string that matches the given regex pattern to appear.
        /// </summary>
        /// <param name="regex">The regex to search for.</param>
        /// <param name="timeout">How long to wait before giving up.</param>
        /// <returns>True if we found a match before the timeout, else false.</returns>
        public bool WaitForString( string regex, int timeout )
        {
            return WaitForString( new Regex( regex ), timeout );
        }

        /// <summary>
        /// Waits for a string that matches the given regex pattern to appear.
        /// </summary>
        /// <param name="regex">The regex to search for.</param>
        /// <param name="timeout">How long to wait before giving up.</param>
        /// <returns>True if we found a match before the timeout, else false.</returns>
        public bool WaitForString( Regex regex, int timeout )
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();

            bool success = false;
            while( ( success == false ) && ( watch.ElapsedMilliseconds <= timeout ) )
            {
                string foundString;
                success = this.buffer.TryDequeue( out foundString );

                if( success == false )
                {
                    Thread.Sleep( 100 );
                    continue;
                }

                success &= regex.IsMatch( foundString );
            }

            return success;
        }
    }
}
