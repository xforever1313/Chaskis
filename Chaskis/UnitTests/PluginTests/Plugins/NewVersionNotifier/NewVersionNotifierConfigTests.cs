//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using Chaskis.Plugins.NewVersionNotifier;
using NUnit.Framework;
using SethCS.Exceptions;

namespace Chaskis.UnitTests.PluginTests.Plugins.NewVersionNotifier
{
    [TestFixture]
    public class NewVersionNotifierConfigTests
    {
        // ---------------- Fields ----------------

        // ---------------- Setup / Teardown ----------------

        [OneTimeSetUp]
        public void FixtureSetup()
        {

        }

        [OneTimeTearDown]
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
        public void ValidateMessageTest()
        {
            NewVersionNotifierConfig uut = new NewVersionNotifierConfig();

            // Ensure default constructor validates.
            Assert.DoesNotThrow( () => uut.Validate() );

            // Message can not be null, empty, or whitespace.
            uut.Message = null;
            Assert.Throws<ValidationException>( () => uut.Validate() );

            uut.Message = string.Empty;
            Assert.Throws<ValidationException>( () => uut.Validate() );

            uut.Message = "      ";
            Assert.Throws<ValidationException>( () => uut.Validate() );
        }

        [Test]
        public void ValidateChannelsToSendToTest()
        {
            NewVersionNotifierConfig uut = new NewVersionNotifierConfig();

            // Ensure default constructor validates.
            Assert.DoesNotThrow( () => uut.Validate() );

            uut = new NewVersionNotifierConfig();
            uut.ChannelsToSendTo.Add( "Hello" );
            Assert.DoesNotThrow( () => uut.Validate() );

            // Channels an not be null, empty, whitespace.
            uut.ChannelsToSendTo.Clear();
            uut.ChannelsToSendTo.Add( null );
            Assert.Throws<ValidationException>( () => uut.Validate() );

            uut.ChannelsToSendTo.Clear();
            uut.ChannelsToSendTo.Add( string.Empty );
            Assert.Throws<ValidationException>( () => uut.Validate() );

            uut.ChannelsToSendTo.Clear();
            uut.ChannelsToSendTo.Add( "    " );
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
            Assert.AreNotSame( uut1, uut2 );

            // Start changing things.
            uut1.Message = uut2.Message + "1";
            Assert.AreNotEqual( uut1, uut2 );
            uut1.Message = uut2.Message;

            uut1.ChannelsToSendTo.Add( "hello" );
            Assert.AreNotEqual( uut1, uut2 );
        }

        // ---------------- Test Helpers ----------------
    }
}
