//
//          Copyright Seth Hendrick 2016-2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
// http://www.boost.org/LICENSE_1_0.txt)
//

using System.IO;
using System.Xml;
using Chaskis.Plugins.CapsWatcher;
using NUnit.Framework;
using SethCS.Exceptions;

namespace Tests.Plugins.CapsWatcher
{
    [TestFixture]
    public class CapsWatcherXmlLoaderTest
    {
        // -------- Fields --------

        /// <summary>
        /// Path to the test XML files.
        /// </summary>
        private static readonly string testFilesPath = Path.Combine(
            TestHelpers.TestsBaseDir, "Plugins", "CapsWatcher", "TestFiles"
        );

        // -------- Setup / Teardown --------

        [SetUp]
        public void Setup()
        {
        }

        [TearDown]
        public void Teardown()
        {
        }

        // -------- Tests --------

        /// <summary>
        /// Ensures a good XML file works.
        /// </summary>
        [Test]
        public void GoodTest()
        {
            string path = Path.Combine( TestHelpers.PluginDir, "CapsWatcher", "Config", "SampleCapsWatcherConfig.xml" );

            CapsWatcherConfig config = XmlLoader.LoadCapsWatcherConfig( path );

            Assert.AreEqual( 3, config.Messages.Count );
            Assert.AreEqual( "LOUD NOISES!", config.Messages[0] );
            Assert.AreEqual( "@{%user%}: shhhhhhhhhh!", config.Messages[1] );
            Assert.AreEqual( "Contrary to popular belief, caps lock is not cruise control for cool :/", config.Messages[2] );
        }

        /// <summary>
        /// Ensures a missing file results in a FileNotFoundException.
        /// </summary>
        [Test]
        public void FileNotFoundTest()
        {
            Assert.Throws<FileNotFoundException>( () => XmlLoader.LoadCapsWatcherConfig( "derp.xml" ) );
        }

        /// <summary>
        /// Ensures all the bad XML file cases are taken care of.
        /// </summary>
        [Test]
        public void BadXmlTests()
        {
            Assert.Throws<ValidationException>(
                () => XmlLoader.LoadCapsWatcherConfig( Path.Combine( testFilesPath, "EmptyMessage.xml" ) )
            );

            Assert.Throws<ValidationException>(
                () => XmlLoader.LoadCapsWatcherConfig( Path.Combine( testFilesPath, "NoMessages.xml" ) )
            );

            Assert.Throws<XmlException>(
                () => XmlLoader.LoadCapsWatcherConfig( Path.Combine( testFilesPath, "BadRoot.xml" ) )
            );
        }
    }
}