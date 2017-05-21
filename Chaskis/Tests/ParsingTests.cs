//
//          Copyright Seth Hendrick 2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using ChaskisCore;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class ParsingTests
    {
        // ---------------- Fields ----------------

        private IIrcConfig config;

        const string remoteUser = "someuser";

        // ---------------- Setup / Teardown ----------------

        [TestFixtureSetUp]
        public void FixtureSetup()
        {
            this.config = TestHelpers.GetTestIrcConfig();
        }

        // ---------------- Tests ----------------

        [Test]
        public void LiquefyStringWithIrcConfig_ArgumentNullTest()
        {
            // Argument check for null:
            Assert.Throws<ArgumentNullException>( () => Parsing.LiquefyStringWithIrcConfig( null ) );
        }

        /// <summary>
        /// Ensures parsing works correctly.
        /// </summary>
        [Test]
        public void LiquefyStringWithIrcConfig_ParseTest()
        {
            const string baseString = "Hello {0}, I'm {1} on channel {2}";
            this.LiquefyStringWithIrcConfig_DoParseTest( baseString );
        }

        /// <summary>
        /// Ensures parsing works correctly when there are multiple tags floating around.
        /// </summary>
        [Test]
        public void LiquefyStringWithIrcConfig_ParseTestMultipleTags()
        {
            const string baseString = "Hello {0} {0}, I'm {1} {1} on channel {2} {2}";
            this.LiquefyStringWithIrcConfig_DoParseTest( baseString );
        }

        // ---------------- Test Helpers ----------------

        /// <summary>
        /// Does the parse test based on the given base string.
        /// </summary>
        /// <param name="baseString">
        /// A string formatted with string.Format.
        /// {0} becomes {%nick%}
        /// {1} becomes {%user%}
        /// {2} becomes {%channel%}
        /// </param>
        private void LiquefyStringWithIrcConfig_DoParseTest( string baseString )
        {
            string startString = string.Format( baseString, "{%nick%}", "{%user%}", "{%channel%}" );

            // No nulls
            {
                string expectedEndString = string.Format(
                    baseString,
                    config.Nick,
                    remoteUser,
                    config.Channel
                );

                string endString = Parsing.LiquefyStringWithIrcConfig( startString, remoteUser, config.Nick, config.Channel );
                Assert.AreEqual( expectedEndString, endString );
            }

            // Remote user is null
            {
                string expectedEndString = string.Format(
                    baseString,
                    config.Nick,
                    "{%user%}",
                    config.Channel
                );

                string endString = Parsing.LiquefyStringWithIrcConfig( startString, null, config.Nick, config.Channel );
                Assert.AreEqual( expectedEndString, endString );
            }

            // Nick is null.
            {
                string expectedEndString = string.Format(
                    baseString,
                    "{%nick%}",
                    remoteUser,
                    config.Channel
                );

                string endString = Parsing.LiquefyStringWithIrcConfig( startString, remoteUser, null, config.Channel );
                Assert.AreEqual( expectedEndString, endString );
            }

            // Channel is null
            {
                string expectedEndString = string.Format(
                    baseString,
                    config.Nick,
                    remoteUser,
                    "{%channel%}"
                );

                string endString = Parsing.LiquefyStringWithIrcConfig( startString, remoteUser, config.Nick, null );
                Assert.AreEqual( expectedEndString, endString );
            }
        }
    }
}
