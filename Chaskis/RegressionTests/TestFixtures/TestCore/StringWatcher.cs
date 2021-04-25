//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Text.RegularExpressions;
using SethCS.Basic;

namespace Chaskis.RegressionTests.TestCore
{
    /// <summary>
    /// Watches for a string on a <see cref="StringBuffer"/>
    /// and is flagged when it does.
    /// </summary>
    public class StringWatcher : IDisposable
    {
        // ---------------- Fields ----------------

        private readonly StringBuffer buffer;
        private readonly Regex pattern;
        private readonly GenericLogger log;

        // ---------------- Constructor ----------------

        public StringWatcher( StringBuffer buffer, string regexPattern, GenericLogger log )
            : this( buffer, new Regex( regexPattern, RegexOptions.ExplicitCapture ), log )
        {
        }

        public StringWatcher( StringBuffer buffer, Regex patternToWatch, GenericLogger log )
        {
            this.buffer = buffer;
            this.pattern = patternToWatch;
            this.log = log;

            this.SawString = false;

            this.buffer.OnNewLine += this.Buffer_OnNewLine;
        }

        // ---------------- Properties ----------------

        /// <summary>
        /// Did we see the matched pattern?
        /// </summary>
        public bool SawString { get; private set; }

        // ---------------- Functions ----------------

        public void Dispose()
        {
            this.buffer.OnNewLine -= this.Buffer_OnNewLine;
        }

        private void Buffer_OnNewLine( string obj )
        {
            if( this.pattern.IsMatch( obj ) )
            {
                this.log?.WriteLine( $"{nameof( StringWatcher )} matched '{obj}' with regex '{this.pattern}'" );
                this.SawString = true;
            }
        }
    }
}
