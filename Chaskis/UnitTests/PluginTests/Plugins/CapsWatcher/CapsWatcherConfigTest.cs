//
//          Copyright Seth Hendrick 2016-2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)
//

using Chaskis.Plugins.CapsWatcher;
using NUnit.Framework;
using SethCS.Exceptions;

namespace Tests.Plugins.CapsWatcher
{
    [TestFixture]
    public class CapsWatcherConfigTest
    {
        // -------- Fields --------

        private CapsWatcherConfig uut;

        // -------- Setup / Teardown --------

        [SetUp]
        public void Setup()
        {
            this.uut = new CapsWatcherConfig();
        }

        [TearDown]
        public void Teardown()
        {
        }

        // -------- Tests --------

        /// <summary>
        /// Ensures that when everything is good,
        /// the bot validates.
        /// </summary>
        [Test]
        public void GoodValidateTest()
        {
            this.uut.Messages.Add( "A message" );
            Assert.DoesNotThrow( () => this.uut.Validate() );

            this.uut.Messages.Add( "Another message" );
            Assert.DoesNotThrow( () => this.uut.Validate() );

            this.uut.Ignores.Add( "HELLO" );
            Assert.DoesNotThrow( () => this.uut.Validate() );
        }

        /// <summary>
        /// Ensures things get thrown when validation fails.
        /// </summary>
        [Test]
        public void BadValidateTest()
        {
            // Empty list should fail.
            Assert.Throws<ValidationException>( () => this.uut.Validate() );

            // Just an empty string should fail.
            this.uut.Messages.Add( string.Empty );
            Assert.Throws<ValidationException>( () => this.uut.Validate() );

            this.uut.Messages.Clear();
            this.uut.Messages.Add( null );
            Assert.Throws<ValidationException>( () => this.uut.Validate() );

            this.uut.Ignores.Add( null );
            Assert.Throws<ValidationException>( () => this.uut.Validate() );
            this.uut.Ignores.Clear();

            this.uut.Ignores.Add( string.Empty );
            Assert.Throws<ValidationException>( () => this.uut.Validate() );
        }
    }
}