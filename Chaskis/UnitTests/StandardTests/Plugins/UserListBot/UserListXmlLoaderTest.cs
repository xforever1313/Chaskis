//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System.IO;
using Chaskis.Plugins.UserListBot;
using NUnit.Framework;

namespace Tests.Plugins.UserListBot
{
    [TestFixture]
    public class UserListXmlLoaderTest
    {
        // -------- Fields --------

        /// <summary>
        /// Path to the test XML files.
        /// </summary>
        private static readonly string testFilesPath = Path.Combine(
            TestHelpers.TestsBaseDir, "Plugins", "UserListBot", "TestFiles"
        );

        // -------- Setup / Teardown --------

        [SetUp]
        public void TestSetup()
        {
        }

        [TearDown]
        public void Teardown()
        {
        }

        // --------- Tests --------

        /// <summary>
        /// Ensures a missing file results in a FileNotFoundException.
        /// </summary>
        [Test]
        public void FileNotFoundTest()
        {
            Assert.Throws<FileNotFoundException>( () => XmlLoader.LoadConfig( "derp.xml" ) );
        }

        /// <summary>
        /// Ensures the default values match the default config.
        /// </summary>
        [Test]
        public void DefaultValueTest()
        {
            string path = Path.Combine( TestHelpers.PluginDir, "UserListBot", "Config", "SampleUserListBotConfig.xml" );

            UserListBotConfig defaultConfig = new UserListBotConfig();

            UserListBotConfig xmlConfig = XmlLoader.LoadConfig( path );

            Assert.AreEqual( defaultConfig.Command, xmlConfig.Command );
            Assert.AreEqual( defaultConfig.Cooldown, xmlConfig.Cooldown );
        }

        /// <summary>
        /// Ensures that an empty xml file results in the default stuff.
        /// </summary>
        [Test]
        public void EmptyValueTest()
        {
            UserListBotConfig defaultConfig = new UserListBotConfig();

            UserListBotConfig xmlConfig = XmlLoader.LoadConfig( Path.Combine( testFilesPath, "BlankConfig.xml" ) );

            Assert.AreEqual( defaultConfig.Command, xmlConfig.Command );
            Assert.AreEqual( defaultConfig.Cooldown, xmlConfig.Cooldown );
        }

        /// <summary>
        /// Ensures that a good config gets loaded correctly.
        /// </summary>
        [Test]
        public void GoodConfigTest()
        {
            UserListBotConfig xmlConfig = XmlLoader.LoadConfig( Path.Combine( testFilesPath, "GoodConfig.xml" ) );

            Assert.AreEqual( "!command", xmlConfig.Command );
            Assert.AreEqual( 0, xmlConfig.Cooldown );
        }
    }
}