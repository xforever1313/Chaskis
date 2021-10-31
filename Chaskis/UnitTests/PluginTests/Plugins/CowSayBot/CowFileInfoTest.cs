//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Chaskis.Plugins.CowSayBot;
using NUnit.Framework;
using SethCS.Exceptions;

namespace Chaskis.UnitTests.PluginTests.Plugins.CowSayBot
{
    [TestFixture]
    public sealed class CowFileInfoTest
    {
        /// <summary>
        /// Ensures that a cow file with a good string
        /// validates.
        /// </summary>
        [Test]
        public void ValidateSuccessTest()
        {
            string name = "name";
            string command = "namesay";

            CowFileInfo uut = new CowFileInfo();
            uut.CommandList[command] = name;
            Assert.DoesNotThrow( () => uut.Validate() );

            Assert.AreEqual( name, uut.CommandList[command] );
        }

        /// <summary>
        /// Ensures that a cow file with no files specfied
        /// results in a failure.
        /// </summary>
        [Test]
        public void ValidateFailEmptyList()
        {
            CowFileInfo uut = new CowFileInfo();
            Assert.Throws<ValidationException>( () => uut.Validate() );
        }

        /// <summary>
        /// Ensures that a cow file with a bad name
        /// does not validate
        /// </summary>
        [Test]
        public void ValidateFailEmptyNameTest()
        {
            string name = string.Empty;
            string command = "namesay";

            {
                CowFileInfo uut = new CowFileInfo();
                uut.CommandList[command] = name;
                Assert.Throws<ValidationException>( () => uut.Validate() );
            }

            name = null;

            {
                CowFileInfo uut = new CowFileInfo();
                uut.CommandList[command] = name;
                Assert.Throws<ValidationException>( () => uut.Validate() );
            }

            name = "     ";
            {
                CowFileInfo uut = new CowFileInfo();
                uut.CommandList[command] = name;
                Assert.Throws<ValidationException>( () => uut.Validate() );
            }
        }

        /// <summary>
        /// Ensures that a cow file with a bad name
        /// does not validate
        /// </summary>
        [Test]
        public void ValidateFailEmptyCommandTest()
        {
            string name = "name";
            string command = string.Empty;

            {
                CowFileInfo uut = new CowFileInfo();
                uut.CommandList[command] = name;
                Assert.Throws<ValidationException>( () => uut.Validate() );
            }

            command = "     ";
            {
                CowFileInfo uut = new CowFileInfo();
                uut.CommandList[command] = name;
                Assert.Throws<ValidationException>( () => uut.Validate() );
            }
        }

        /// <summary>
        /// Ensures the clone method works.
        /// </summary>
        [Test]
        public void CloneTest()
        {
            string name = "name";
            string command = "namesay";

            CowFileInfo uut1 = new CowFileInfo();
            uut1.CommandList[command] = name;
            CowFileInfo uut2 = uut1.Clone();

            Assert.AreNotSame( uut1, uut2 );

            Assert.AreEqual( uut1.CommandList[command], uut2.CommandList[command] );
        }
    }
}