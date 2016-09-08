
//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Tests.Plugins.CapsWatcher
{
    [TestFixture]
    public class CapsWatcherTest
    {
        // -------- Tests --------

        /// <summary>
        /// Ensures the CheckForCaps function is working.
        /// </summary>
        [Test]
        public void CheckForCapsTest()
        {
            GoodTest( "HELLO WORLD!" );
            GoodTest( "123HI21" );
            GoodTest( "I AM TALKING VERY LOUDLY!" );
            GoodTest( "HEY" );
            GoodTest( "ABCDEFGHIJKLMNOPQRSTUVWXYZ" );

            BadTest( "LOWERcASE" );
            BadTest( "1234567890" );
            BadTest( "HI" );
            BadTest( string.Empty );
            BadTest( null );
            BadTest( "!@#$%^&*()" );
            BadTest( "abcdefghijklmnopqrstuvwxyz" );
        }

        // -------- Test Helpers -------

        /// <summary>
        /// Ensures the given string is good.
        /// </summary>
        /// <param name="str">The string to try.</param>
        private void GoodTest( string str )
        {
            Assert.IsTrue( Chaskis.Plugins.CapsWatcher.CapsWatcher.CheckForCaps( str ) );
        }

        /// <summary>
        /// Ensures the given string is bad.
        /// </summary>
        /// <param name="str">The string to try.</param>
        private void BadTest( string str )
        {
            Assert.IsFalse( Chaskis.Plugins.CapsWatcher.CapsWatcher.CheckForCaps( str ) );
        }
    }
}
