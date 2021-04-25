//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.IO;
using System.Xml;
using Chaskis.Plugins.QuoteBot;
using Chaskis.UnitTests.Common;
using NUnit.Framework;
using SethCS.Exceptions;

namespace Chaskis.UnitTests.PluginTests.Plugins.QuoteBot
{
    [TestFixture]
    public class QuoteBotXmlLoaderTests
    {
        // ---------------- Fields ----------------

        private static readonly string quoteBotPluginDir =
            Path.Combine( TestHelpers.PluginDir, "QuoteBot" );

        private static readonly string quoteBotTestDir =
            Path.Combine( TestHelpers.PluginTestsDir, "Plugins", "QuoteBot" );

        // ---------------- Tests ----------------

        /// <summary>
        /// Ensures we get a FileNotFoundException when we pass in a file
        /// that does not exist.
        /// </summary>
        [Test]
        public void FileNotFoundTest()
        {
            Assert.Throws<FileNotFoundException>(
                () => XmlLoader.LoadConfig( "Derp.xml" )
            );
        }

        /// <summary>
        /// Ensures that our sample config matches
        /// our default config's settings.
        /// </summary>
        [Test]
        public void DefaultConfigTest()
        {
            QuoteBotConfig defaultConfig = new QuoteBotConfig();
            QuoteBotConfig loadedConfig = XmlLoader.LoadConfig(
                Path.Combine( quoteBotPluginDir, "Config", "SampleQuoteBotConfig.xml" )
            );

            Assert.AreEqual( defaultConfig, loadedConfig );
        }

        /// <summary>
        /// Ensures that our sample config matches
        /// our default config's settings.
        /// </summary>
        [Test]
        public void XmlLoadTest()
        {
            QuoteBotConfig expectedConfig = new QuoteBotConfig();
            expectedConfig.AddCommand = @"^@quote\s+add\s+\<(?<user>\S+)\>\s+(?<quote>.+)";
            expectedConfig.DeleteCommand = @"^@quote\s+delete\s+(?<id>\d+)";
            expectedConfig.RandomCommand = @"^@quote\s+random";
            expectedConfig.GetCommand = @"^@quote\s+(get)?\s*(?<id>\d+)";

            QuoteBotConfig loadedConfig = XmlLoader.LoadConfig(
                Path.Combine( quoteBotTestDir, "TestFiles", "GoodConfig.xml" )
            );

            Assert.AreEqual( expectedConfig, loadedConfig );
        }

        /// <summary>
        /// Ensures that our sample config matches
        /// our default config's settings.
        /// </summary>
        [Test]
        public void XmlLoadValidationErrorTest()
        {
            Assert.Throws<ValidationException>( () =>
                XmlLoader.LoadConfig(
                    Path.Combine( quoteBotTestDir, "TestFiles", "EmptyStringConfig.xml" )
                )
            );
        }

        /// <summary>
        /// Ensures that our sample config matches
        /// our default config's settings.
        /// </summary>
        [Test]
        public void XmlLoadValidationBadRoot()
        {
            Assert.Throws<XmlException>( () =>
                XmlLoader.LoadConfig(
                    Path.Combine( quoteBotTestDir, "TestFiles", "BadRootConfig.xml" )
                )
            );
        }
    }
}
