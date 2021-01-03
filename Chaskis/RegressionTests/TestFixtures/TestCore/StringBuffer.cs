//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;

namespace Chaskis.RegressionTests.TestCore
{
    /// <summary>
    /// This class enqueues strings and has methods that search for strings.
    /// As it searches, it throws out strings we don't need.
    /// 
    /// Thread-safe.
    /// </summary>
    public class StringBuffer
    {
        // ---------------- Events ----------------

        /// <summary>
        /// Event that is fired when we get a new line on the string, but before
        /// it gets enqueued to the buffer.
        /// </summary>
        public event Action<string> OnNewLine;

        // ---------------- Fields ----------------

        private readonly ConcurrentQueue<string> buffer;

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
            this.OnNewLine?.Invoke( str );
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

        public void FlushQueue()
        {
            while( this.buffer.Count > 0 )
            {
                this.buffer.TryDequeue( out string result );
            }
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
