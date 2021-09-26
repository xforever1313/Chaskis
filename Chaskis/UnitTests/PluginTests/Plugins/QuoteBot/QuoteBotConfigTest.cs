//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Chaskis.Plugins.QuoteBot;
using NUnit.Framework;
using SethCS.Exceptions;

namespace Chaskis.UnitTests.PluginTests.Plugins.QuoteBot
{
    [TestFixture]
    public sealed class QuoteBotConfigTest
    {
        // ---------------- Tests ----------------

        /// <summary>
        /// Ensures our validate method works correctly.
        /// </summary>
        [Test]
        public void ValidationTest()
        {
            QuoteBotConfig config = new QuoteBotConfig();

            // Default config should not throw.
            Assert.DoesNotThrow( () => config.Validate() );

            // Start removing things.
            config.AddCommand = null;
            Assert.Throws<ValidationException>( () => config.Validate() );
            config.AddCommand = string.Empty;
            Assert.Throws<ValidationException>( () => config.Validate() );
            config.AddCommand = "           ";
            Assert.Throws<ValidationException>( () => config.Validate() );
            config = new QuoteBotConfig();

            config.DeleteCommand = null;
            Assert.Throws<ValidationException>( () => config.Validate() );
            config.DeleteCommand = string.Empty;
            Assert.Throws<ValidationException>( () => config.Validate() );
            config.DeleteCommand = "           ";
            Assert.Throws<ValidationException>( () => config.Validate() );
            config = new QuoteBotConfig();

            config.RandomCommand = null;
            Assert.Throws<ValidationException>( () => config.Validate() );
            config.RandomCommand = string.Empty;
            Assert.Throws<ValidationException>( () => config.Validate() );
            config.RandomCommand = "           ";
            Assert.Throws<ValidationException>( () => config.Validate() );
            config = new QuoteBotConfig();

            config.GetCommand = null;
            Assert.Throws<ValidationException>( () => config.Validate() );
            config.GetCommand = string.Empty;
            Assert.Throws<ValidationException>( () => config.Validate() );
            config.GetCommand = "           ";
            Assert.Throws<ValidationException>( () => config.Validate() );
            config = new QuoteBotConfig();
        }

        /// <summary>
        /// Whether or not the equals function is working right.
        /// </summary>
        [Test]
        public void EqualsTest()
        {
            QuoteBotConfig config1 = new QuoteBotConfig();
            QuoteBotConfig config2 = new QuoteBotConfig();

            Assert.AreEqual( config1, config2 );
            Assert.AreEqual( config2, config1 );
            Assert.AreEqual( config1.GetHashCode(), config2.GetHashCode() );

            Assert.IsFalse( config1.Equals( 2 ) );
            Assert.IsFalse( config1.Equals( null ) );

            // Start changing things:
            config1.AddCommand += "1";
            Assert.AreNotEqual( config1, config2 );
            Assert.AreNotEqual( config2, config1 );
            Assert.AreNotEqual( config1.GetHashCode(), config2.GetHashCode() );
            config1 = new QuoteBotConfig();

            config1.DeleteCommand += "1";
            Assert.AreNotEqual( config1, config2 );
            Assert.AreNotEqual( config2, config1 );
            Assert.AreNotEqual( config1.GetHashCode(), config2.GetHashCode() );
            config1 = new QuoteBotConfig();

            config1.RandomCommand+= "1";
            Assert.AreNotEqual( config1, config2 );
            Assert.AreNotEqual( config2, config1 );
            Assert.AreNotEqual( config1.GetHashCode(), config2.GetHashCode() );
            config1 = new QuoteBotConfig();

            config1.GetCommand+= "1";
            Assert.AreNotEqual( config1, config2 );
            Assert.AreNotEqual( config2, config1 );
            Assert.AreNotEqual( config1.GetHashCode(), config2.GetHashCode() );
            config1 = new QuoteBotConfig();
        }

        /// <summary>
        /// Ensures our clone method works correctly.
        /// </summary>
        [Test]
        public void CloneTest()
        {
            QuoteBotConfig config1 = new QuoteBotConfig();
            QuoteBotConfig config2 = config1.Clone();

            // Equivalent, but not same instance.
            Assert.AreEqual( config1, config2 );
            Assert.AreNotSame( config1, config2 );
        }
    }
}
