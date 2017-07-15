//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

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

            // Needs at least one space in order to work.
            BadTest( "123HI21" );
            BadTest( "HEY" );
            BadTest( "ABCDEFGHIJKLMNOPQRSTUVWXYZ" );

            // Lowercase doesn't work.
            BadTest( "LOWERcASE" );

            // Just numbers are ignored.
            BadTest( "1234567890" );

            // Not enough characters or spaces.
            BadTest( "HI" );

            // Null/Empty
            BadTest( string.Empty );
            BadTest( null );

            // No letters
            BadTest( "!@#$%^&*()" );

            // Just lowercase.
            BadTest( "abcdefghijklmnopqrstuvwxyz" );

            // Emoji's shouldn't work.
            BadTest( "¯\\_(ツ)_/¯" );
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