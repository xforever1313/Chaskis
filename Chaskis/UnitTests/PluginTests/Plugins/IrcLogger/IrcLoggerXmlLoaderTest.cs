//
//          Copyright Seth Hendrick 2016-2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.IO;
using Chaskis.Plugins.IrcLogger;
using Chaskis.UnitTests.Common;
using NUnit.Framework;

namespace Chaskis.UnitTests.PluginTests.Plugins.IrcLogger
{
    [TestFixture]
    public class IrcLoggerXmlLoaderTest
    {
        // -------- Fields --------

        /// <summary>
        /// Path to this plugin's test files.
        /// </summary>
        private static readonly string testFilesPath = Path.Combine(
            TestHelpers.PluginTestsDir,
            "Plugins",
            "IrcLogger",
            "TestFiles"
        );

        // -------- Tests --------

        /// <summary>
        /// Ensures loading the XML with empty tags
        /// results in the default values being used.
        /// </summary>
        [Test]
        public void DefaultValueTestEmptyTags()
        {
            string location = Path.Combine(
                TestHelpers.PluginDir,
                "IrcLogger",
                "Config",
                "SampleIrcLoggerConfig.xml"
            );

            IrcLoggerConfig config = XmlLoader.LoadIrcLoggerConfig( location );

            // Empty tags result in an empty string, not null.
            Assert.AreEqual( string.Empty, config.LogName );
            Assert.AreEqual( string.Empty, config.LogFileLocation );
            Assert.AreEqual( 1000, config.MaxNumberMessagesPerLog );
            Assert.AreEqual( 1, config.IgnoreRegexes.Count );
            Assert.AreEqual( @"^:\S+\s+PONG\s+\S+\s+:.+$", config.IgnoreRegexes[0].ToString() );
        }

        /// <summary>
        /// Ensures loading the XML with no tags
        /// results in the default values being used.
        /// </summary>
        [Test]
        public void DefaultValueTestNoTags()
        {
            string location = Path.Combine(
                testFilesPath,
                "NoTags.xml"
            );

            IrcLoggerConfig config = XmlLoader.LoadIrcLoggerConfig( location );

            Assert.IsNull( config.LogName );
            Assert.IsNull( config.LogFileLocation );
            Assert.AreEqual( 1000, config.MaxNumberMessagesPerLog );
            Assert.AreEqual( 0, config.IgnoreRegexes.Count );
        }

        /// <summary>
        /// Ensures loading the XML with non-default settings
        /// works.
        /// </summary>
        [Test]
        public void LoadTest()
        {
            string location = Path.Combine(
                testFilesPath,
                "GoodDefault.xml"
            );

            IrcLoggerConfig config = XmlLoader.LoadIrcLoggerConfig( location );

            Assert.AreEqual( "chaskis", config.LogName );
            Assert.AreEqual( "/home/me/logs/ChaskisLogs", config.LogFileLocation );
            Assert.AreEqual( 90, config.MaxNumberMessagesPerLog );
            Assert.AreEqual( 0, config.IgnoreRegexes.Count );
        }
    }
}