//
//          Copyright Seth Hendrick 2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using Chaskis.Plugins.NewVersionNotifier;
using NUnit.Framework;
using SethCS.Exceptions;

namespace Tests.Plugins.NewVersionNotifier
{
    [TestFixture]
    public class NewVersionNotifierConfigTests
    {
        // ---------------- Fields ----------------

        // ---------------- Setup / Teardown ----------------

        [TestFixtureSetUp]
        public void FixtureSetup()
        {

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

        [Test]
        public void ValidateTest()
        {
            NewVersionNotifierConfig uut = new NewVersionNotifierConfig();

            // Ensure default constructor validates.
            Assert.DoesNotThrow( () => uut.Validate() );

            uut.Delay = TimeSpan.Zero; // This is okay.
            Assert.DoesNotThrow( () => uut.Validate() );

            // Start changing things.
            uut.Delay = new TimeSpan( 0, 0, -1 );
            Assert.Throws<ValidationException>( () => uut.Validate() );
            uut = new NewVersionNotifierConfig();

            uut.Message = null;
            Assert.Throws<ValidationException>( () => uut.Validate() );

            uut.Message = string.Empty;
            Assert.Throws<ValidationException>( () => uut.Validate() );

            uut.Message = "      ";
            Assert.Throws<ValidationException>( () => uut.Validate() );
        }

        [Test]
        public void EqualsTest()
        {
            NewVersionNotifierConfig uut1 = new NewVersionNotifierConfig();
            NewVersionNotifierConfig uut2 = new NewVersionNotifierConfig();

            Assert.IsFalse( uut2.Equals( 1 ) );
            Assert.IsFalse( uut2.Equals( null ) );

            Assert.AreEqual( uut1, uut2 );
            Assert.AreEqual( uut1.GetHashCode(), uut2.GetHashCode() );
            Assert.AreNotSame( uut1, uut2 );

            // Start changing things.

            uut1.Delay = uut2.Delay + new TimeSpan( 1 );
            Assert.AreNotEqual( uut1, uut2 );
            Assert.AreNotEqual( uut1.GetHashCode(), uut2.GetHashCode() );
            uut1.Delay = uut2.Delay;

            uut1.Message = uut2.Message + "1";
            Assert.AreNotEqual( uut1, uut2 );
            Assert.AreNotEqual( uut1.GetHashCode(), uut2.GetHashCode() );
            uut1.Message = uut2.Message;
        }

        // ---------------- Test Helpers ----------------
    }
}
