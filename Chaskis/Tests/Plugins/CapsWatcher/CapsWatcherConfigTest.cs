using System;
using Chaskis.Plugins.CapsWatcher;
using NUnit.Framework;

namespace Tests.Plugins.CapsWatcher
{
    [TestFixture]
    public class CapsWatcherConfigTest
    {
        // -------- Fields --------

        CapsWatcherConfig uut;

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
        }

        /// <summary>
        /// Ensures things get thrown when validation fails.
        /// </summary>
        [Test]
        public void BadValidateTest()
        {
            // Empty list should fail.
            Assert.Throws<InvalidOperationException>( () => this.uut.Validate() );

            // Just an empty string should fail.
            this.uut.Messages.Add( string.Empty );
            Assert.Throws<InvalidOperationException>( () => this.uut.Validate() );

            this.uut.Messages.Clear();
            this.uut.Messages.Add( null );
            Assert.Throws<InvalidOperationException>( () => this.uut.Validate() );
        }
    }
}
