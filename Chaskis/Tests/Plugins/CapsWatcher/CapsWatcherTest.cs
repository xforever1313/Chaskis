//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
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
            GoodTest( "I AM TALKING VERY LOUDLY!" );
            GoodTest( "HELLO THERE WORLD" );
            GoodTest( "I AM, COOL!?" );
            GoodTest( "THIS :D IS :D AWESOME! :D" );
            GoodTest( "HELLO C: WORLD!" );
            GoodTest( ":P GREETINGS WORLD!" );
            GoodTest( "GREETINGS WORLD :D" );
            GoodTest( ":p HELLO WORLD!" ); // Emoticons should be ignored.
            GoodTest( "WHY HELLO :p WORLD" );
            GoodTest( "THIS IS A GOOD WORLD :p" );
            GoodTest( ":p THIS IS A GOOD WORLD :p" );
            GoodTest( "123HI21" );
            GoodTest( "HEY" );
            GoodTest( "ABCDEFGHIJKLMNOPQRSTUVWXYZ" );

            // Two characters should work.
            GoodTest( "HI" );

            // One character should not.
            BadTest( "I" );

            // Lowercase doesn't work.
            BadTest( "LOWERcASE" );
            BadTest( "This :D is :D awesome! :D" );

            // Just numbers are ignored.
            BadTest( "1234567890" );

            // Null/Empty
            BadTest( string.Empty );
            BadTest( null );

            // No letters
            BadTest( "!@#$%^&*()" );

            // Just lowercase.
            BadTest( "abcdefghijklmnopqrstuvwxyz" );

            // Emoji's shouldn't work.
            BadTest( "¯\\_(ツ)_/¯" );
            BadTest( ":D :D :D" );
            BadTest( ":P :D C: D:" );

            // Just whitespace should fail.
            BadTest( "      " );
        }

        // -------- Test Helpers -------

        /// <summary>
        /// Ensures the given string is good.
        /// </summary>
        /// <param name="str">The string to try.</param>
        private void GoodTest( string str )
        {
            DoTest( str, true );
        }

        /// <summary>
        /// Ensures the given string is bad.
        /// </summary>
        /// <param name="str">The string to try.</param>
        private void BadTest( string str )
        {
            DoTest( str, false );
        }

        private void DoTest( string str, bool expectSuccess )
        {
            bool success = Chaskis.Plugins.CapsWatcher.CapsWatcher.CheckForCaps( str );
            if ( success != expectSuccess )
            {
                Console.WriteLine( "Caps watcher test failed.  String that failed:" + str );
            }

            Assert.AreEqual( expectSuccess, success );
        }
    }
}