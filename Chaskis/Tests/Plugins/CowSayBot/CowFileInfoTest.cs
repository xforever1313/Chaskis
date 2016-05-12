using System;
using Chaskis.Plugins.CowSayBot;
using NUnit.Framework;

namespace Tests.Plugins.CowSayBot
{
    [TestFixture]
    public class CowFileInfoTest
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

            CowFileInfo uut = new CowFileInfo( name, command );
            Assert.DoesNotThrow( () => uut.Validate() );

            Assert.AreEqual( name, uut.Name );
            Assert.AreEqual( command, uut.Command );
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
                CowFileInfo uut = new CowFileInfo( name, command );
                Assert.Throws<InvalidOperationException>( () => uut.Validate() );
            }

            name = null;

            {
                CowFileInfo uut = new CowFileInfo( name, command );
                Assert.Throws<InvalidOperationException>( () => uut.Validate() );
            }

            name = "     ";
            {
                CowFileInfo uut = new CowFileInfo( name, command );
                Assert.Throws<InvalidOperationException>( () => uut.Validate() );
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
                CowFileInfo uut = new CowFileInfo( name, command );
                Assert.Throws<InvalidOperationException>( () => uut.Validate() );
            }

            command = null;

            {
                CowFileInfo uut = new CowFileInfo( name, command );
                Assert.Throws<InvalidOperationException>( () => uut.Validate() );
            }

            command = "     ";
            {
                CowFileInfo uut = new CowFileInfo( name, command );
                Assert.Throws<InvalidOperationException>( () => uut.Validate() );
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

            CowFileInfo uut1 = new CowFileInfo( name, command );
            CowFileInfo uut2 = uut1.Clone();

            Assert.AreNotSame( uut1, uut2 );

            Assert.AreEqual( uut1.Name, uut2.Name );
            Assert.AreEqual( uut1.Command, uut2.Command );

            Assert.AreEqual( name, uut1.Name );
            Assert.AreEqual( command, uut1.Command );

            Assert.AreEqual( name, uut2.Name );
            Assert.AreEqual( command, uut2.Command );
        }
    }
}
