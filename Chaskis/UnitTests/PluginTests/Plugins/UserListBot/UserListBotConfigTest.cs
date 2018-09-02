//
//          Copyright Seth Hendrick 2016-2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Chaskis.Plugins.UserListBot;
using NUnit.Framework;
using SethCS.Exceptions;

namespace Chaskis.UnitTests.PluginTests.Plugins.UserListBot
{
    [TestFixture]
    public class UserListBotConfigTest
    {
        // -------- Fields --------

        private UserListBotConfig uut;

        // -------- Setup / Teardown --------

        [SetUp]
        public void TestSetup()
        {
            this.uut = new UserListBotConfig();
        }

        [TearDown]
        public void Teardown()
        {
        }

        // -------- Tests --------

        /// <summary>
        /// Ensures the validate() method works.
        /// </summary>
        [Test]
        public void ValidationTest()
        {
            // Default construction should pass.
            Assert.DoesNotThrow( () => this.uut.Validate() );

            // Null, empty, or whitespace command should fail.
            this.uut.Command = null;
            Assert.Throws<ValidationException>( () => this.uut.Validate() );
            this.uut = new UserListBotConfig();

            this.uut.Command = string.Empty;
            Assert.Throws<ValidationException>( () => this.uut.Validate() );
            this.uut = new UserListBotConfig();

            this.uut.Command = "   ";
            Assert.Throws<ValidationException>( () => this.uut.Validate() );
            this.uut = new UserListBotConfig();

            // Negative cooldown should fail.
            this.uut.Cooldown = -1;
            Assert.Throws<ValidationException>( () => this.uut.Validate() );
            this.uut = new UserListBotConfig();

            // Zero cooldown should not.
            this.uut.Cooldown = 0; ;
            Assert.DoesNotThrow( () => this.uut.Validate() );
            this.uut = new UserListBotConfig();
        }
    }
}