//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Text.RegularExpressions;
using Chaskis.Plugins.KarmaBot;
using NUnit.Framework;
using SethCS.Exceptions;

namespace Chaskis.UnitTests.PluginTests.Plugins.KarmaBot
{
    [TestFixture]
    public sealed class KarmaBotConfigTest
    {
        /// <summary>
        /// Ensures the default constructor validates.
        /// </summary>
        [Test]
        public void DefaultConstructorValidateTest()
        {
            KarmaBotConfig uut = new KarmaBotConfig();
            Assert.DoesNotThrow( () => uut.Validate() );
        }

        /// <summary>
        /// Ensures the default increase regex works properly.
        /// </summary>
        [Test]
        public void DefaultIncreaseRegexTest()
        {
            KarmaBotConfig uut = new KarmaBotConfig();

            Regex increaseRegex = new Regex( uut.IncreaseCommandRegex );

            // ++name is good.
            {
                Match goodMatch1 = increaseRegex.Match( "++derp" );
                Assert.IsTrue( goodMatch1.Success );
                Assert.AreEqual( "derp", goodMatch1.Groups["name"].Value );
            }

            // name++ is good.
            {
                Match goodMatch2 = increaseRegex.Match( "derp++" );
                Assert.IsTrue( goodMatch2.Success );
                Assert.AreEqual( "derp", goodMatch2.Groups["name"].Value );
            }

            // ++name something is good.
            {
                Match goodMatch3 = increaseRegex.Match( "++derp for being awesome!" );
                Assert.IsTrue( goodMatch3.Success );
                Assert.AreEqual( "derp", goodMatch3.Groups["name"].Value );
            }

            // name++ something is good.
            {
                Match goodMatch4 = increaseRegex.Match( "derp++ for being cool!" );
                Assert.IsTrue( goodMatch4.Success );
                Assert.AreEqual( "derp", goodMatch4.Groups["name"].Value );
            }

            // Something in front of ++name is bad
            {
                Match badMatch1 = increaseRegex.Match( " ++derp" );
                Assert.IsFalse( badMatch1.Success );
            }

            // something in front of name++ is bad.
            {
                Match badMatch2 = increaseRegex.Match( "sdfdsaf derp++" );
                Assert.IsFalse( badMatch2.Success );
            }
        }

        /// <summary>
        /// Ensures the default decrease regex works properly.
        /// </summary>
        [Test]
        public void DefaultDecreaseRegexTest()
        {
            KarmaBotConfig uut = new KarmaBotConfig();

            Regex decreaseRegex = new Regex( uut.DecreaseCommandRegex );

            // --name is good.
            {
                Match goodMatch1 = decreaseRegex.Match( "--derp" );
                Assert.IsTrue( goodMatch1.Success );
                Assert.AreEqual( "derp", goodMatch1.Groups["name"].Value );
            }

            // name-- is good.
            {
                Match goodMatch2 = decreaseRegex.Match( "derp--" );
                Assert.IsTrue( goodMatch2.Success );
                Assert.AreEqual( "derp", goodMatch2.Groups["name"].Value );
            }

            // --name something is good.
            {
                Match goodMatch3 = decreaseRegex.Match( "--derp for being terrible!" );
                Assert.IsTrue( goodMatch3.Success );
                Assert.AreEqual( "derp", goodMatch3.Groups["name"].Value );
            }

            // name-- something is good.
            {
                Match goodMatch4 = decreaseRegex.Match( "derp-- for being a jerk!" );
                Assert.IsTrue( goodMatch4.Success );
                Assert.AreEqual( "derp", goodMatch4.Groups["name"].Value );
            }

            // Something in front of --name is bad
            {
                Match badMatch1 = decreaseRegex.Match( " --derp" );
                Assert.IsFalse( badMatch1.Success );
            }

            // something in front of name-- is bad.
            {
                Match badMatch2 = decreaseRegex.Match( "sdfdsaf derp--" );
                Assert.IsFalse( badMatch2.Success );
            }
        }

        /// <summary>
        /// Ensures the default query regex works properly.
        /// </summary>
        [Test]
        public void DefaultQueryRegexTest()
        {
            KarmaBotConfig uut = new KarmaBotConfig();

            Regex queryRegex = new Regex( uut.QueryCommand );

            {
                Match goodMatch = queryRegex.Match( "!karma derp" );
                Assert.IsTrue( goodMatch.Success );
                Assert.AreEqual( "derp", goodMatch.Groups["name"].Value );
            }

            // Characters in front are bad.
            {
                Match badMatch = queryRegex.Match( "   !karma derp" );
                Assert.IsFalse( badMatch.Success );
            }

            {
                Match badMatch = queryRegex.Match( "asdfsad !karma derp" );
                Assert.IsFalse( badMatch.Success );
            }
        }

        /// <summary>
        /// Ensures an exception is thrown if we have a bad
        /// increase command.
        /// </summary>
        [Test]
        public void BadIncreaseCommandTest()
        {
            KarmaBotConfig uut = new KarmaBotConfig();
            uut.IncreaseCommandRegex = null; // Null is bad.
            Assert.Throws<ValidationException>( () => uut.Validate() );

            uut.IncreaseCommandRegex = string.Empty; // Empty is bad.
            Assert.Throws<ValidationException>( () => uut.Validate() );

            // No <name> group.
            uut.IncreaseCommandRegex = "++derp";
            Assert.Throws<ValidationException>( () => uut.Validate() );
        }

        /// <summary>
        /// Ensures an exception is thrown if we have a bad
        /// decrease command.
        /// </summary>
        [Test]
        public void BadDecreaseCommandTest()
        {
            KarmaBotConfig uut = new KarmaBotConfig();
            uut.DecreaseCommandRegex = null; // Null is bad.
            Assert.Throws<ValidationException>( () => uut.Validate() );

            uut.DecreaseCommandRegex = string.Empty; // Empty is bad.
            Assert.Throws<ValidationException>( () => uut.Validate() );

            // No <name> group.
            uut.DecreaseCommandRegex = "++derp";
            Assert.Throws<ValidationException>( () => uut.Validate() );
        }

        /// <summary>
        /// Ensures an exception is thrown if we have a bad
        /// query command.
        /// </summary>
        [Test]
        public void BadQueryCommandTest()
        {
            KarmaBotConfig uut = new KarmaBotConfig();
            uut.QueryCommand = null; // Null is bad.
            Assert.Throws<ValidationException>( () => uut.Validate() );

            uut.QueryCommand = string.Empty; // Empty is bad.
            Assert.Throws<ValidationException>( () => uut.Validate() );

            // No <name> group.
            uut.QueryCommand = "++derp";
            Assert.Throws<ValidationException>( () => uut.Validate() );
        }
    }
}