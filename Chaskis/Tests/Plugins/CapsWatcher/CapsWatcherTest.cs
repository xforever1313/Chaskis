//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Tests.Plugins.CapsWatcher
{
    [TestFixture]
    public class CapsWatcherTest
    {
        // ---------------- Fields ----------------

        private List<string> ignores;

        private Regex ignoreRegex;

        // ---------------- Setup / Teardown ----------------

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            this.ignores = new List<string>();
            this.ignores.Add( "NY" );
            this.ignores.Add( "IRC BOT" );
            this.ignores.Add( "US" );
            this.ignores.Add( "USA" );

            this.ignoreRegex = new Regex(
                Chaskis.Plugins.CapsWatcher.CapsWatcher.CollectionToRegex( ignores ),
                RegexOptions.Compiled | RegexOptions.ExplicitCapture
            );
        }

        [TestFixtureTearDown]
        public void FixtureTeardown()
        {
        }

        [SetUp]
        public void TestSetup()
        {
        }

        [TearDown]
        public void TestTeardown()
        {
        }

        // ---------------- Tests ----------------

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

            // Ignores won't trigger the bot, but ignores WITH non-ignores will!
            GoodTest( "HI USA!" );
            GoodTest( "HELLO IRC BOT!" );
            GoodTest( "HI USA! HI" );
            GoodTest( "NY USA HELLO USA NY" );

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

            // Ignores shouldn't work.
            BadTest( "NY USA US" );
            BadTest( "NY" );
            BadTest( "USA" );
            BadTest( " USA " );
            BadTest( "USA USA" );
            BadTest( "IRC BOT" );
            BadTest( " IRC BOT " );
            BadTest( "USA IRC BOT" );
        }

        [Test]
        public void OneIgnoreTest()
        {
            List<string> oneIgnore = new List<string>();
            oneIgnore.Add( "NY" );
            Regex regex = new Regex(
                Chaskis.Plugins.CapsWatcher.CapsWatcher.CollectionToRegex( oneIgnore ),
                RegexOptions.Compiled | RegexOptions.ExplicitCapture
            );

            this.DoTest( "NY", false, regex );
            this.DoTest( "NY NY", false, regex );
            this.DoTest( "NYC NYC", true, regex );
            this.DoTest( "CNY NYC", true, regex );
            this.DoTest( "BILL NYE", true, regex ); // NY in middle.
            this.DoTest( "Bill NYE", false, regex ); // NY in middle.
            this.DoTest( "NY USA HELLO USA NY", true, regex );
        }

        [Test]
        public void NoIgnoreTest()
        {
            List<string> noIgnore = new List<string>();

            Regex regex = new Regex(
                Chaskis.Plugins.CapsWatcher.CapsWatcher.CollectionToRegex( noIgnore ),
                RegexOptions.Compiled | RegexOptions.ExplicitCapture
            );

            this.DoTest( "NY", true, regex );
            this.DoTest( "NY NY", true, regex );
            this.DoTest( "NY USA HELLO USA NY", true, regex );
        }

        // ---------------- Test Helpers ----------------

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

        private void DoTest( string str, bool expectSuccess, Regex ignore = null )
        {
            bool success = Chaskis.Plugins.CapsWatcher.CapsWatcher.CheckForCaps( str, ignore ?? this.ignoreRegex );
            if ( success != expectSuccess )
            {
                Console.WriteLine( "Caps watcher test failed.  String that failed:" + str );
            }

            Assert.AreEqual( expectSuccess, success );
        }
    }
}