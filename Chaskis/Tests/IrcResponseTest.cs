//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using GenericIrcBot;
using NUnit.Framework;

namespace Tests
{
    /// <summary>
    /// Tests the IrcResponse class.
    /// </summary>
    [TestFixture]
    public class IrcResponseTest
    {
        // -------- Fields --------

        /// <summary>
        /// Unit under test.
        /// </summary>
        private IrcResponse uut;

        // -------- Setup / Teardown --------

        [SetUp]
        public void TestSetup()
        {
            this.uut = new IrcResponse(
                "SomeUser",
                "#SomeChannel",
                "Some Message"
            );
        }

        // -------- Tests --------

        /// <summary>
        /// Ensures the clone method works correctly.
        /// </summary>
        [Test]
        public void CloneTest()
        {
            IrcResponse clone = this.uut.Clone();

            // Ensure all the properties match.
            Assert.AreEqual( this.uut.Channel, clone.Channel );
            Assert.AreEqual( this.uut.Message, clone.Message );
            Assert.AreEqual( this.uut.RemoteUser, clone.RemoteUser );

            // Ensure they are NOT the same reference.
            Assert.AreNotSame( this.uut, clone );
        }

        /// <summary>
        /// Ensures the Equals function works.
        /// </summary>
        [Test]
        public void EqualsTest()
        {
            // Ensure false returns when a null or an unrelated
            // reference goes in.
            Assert.IsFalse( this.uut.Equals( null ) );
            Assert.IsFalse( this.uut.Equals( 1 ) );
            Assert.IsFalse( this.uut.Equals( this ) );

            IrcResponse other = this.uut.Clone();

            // Ensure they are equal.
            Assert.AreEqual( this.uut, other );

            // Now, start changing properties and making sure they are not.
            other = new IrcResponse( this.uut.RemoteUser, this.uut.Channel, "A different Message" );
            Assert.AreNotEqual( this.uut, other );

            other = new IrcResponse( this.uut.RemoteUser, "#newChannel", this.uut.Message );
            Assert.AreNotEqual( this.uut, other );

            other = new IrcResponse( "newUser", this.uut.Channel, this.uut.Message );
            Assert.AreNotEqual( this.uut, other );
        }
    }
}