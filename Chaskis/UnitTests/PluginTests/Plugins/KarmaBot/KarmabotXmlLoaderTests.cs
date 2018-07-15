//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System.IO;
using Chaskis.Plugins.KarmaBot;
using NUnit.Framework;

namespace Tests.Plugins.KarmaBot
{
    [TestFixture]
    public class KarmabotXmlLoaderTests
    {
        // -------- Fields --------

        /// <summary>
        /// Directory to the karmabot project.
        /// </summary>
        private static readonly string karmaBotDir = Path.Combine(
            TestHelpers.ProjectRoot, "Plugins", "KarmaBot"
        );

        /// <summary>
        /// Directory to the test xml files.
        /// </summary>
        private static readonly string testConfigDir = Path.Combine(
            TestHelpers.PluginTestsDir, "Plugins", "KarmaBot", "TestFiles"
        );

        // -------- Tests --------

        /// <summary>
        /// Ensures the default config object matches the settings
        /// in the sample XML file.
        ///
        /// This also ensures that our default regexes will work with the bot,
        /// as KarmaBotConfigTest tests those regexes.
        /// </summary>
        [Test]
        public void DefaultConfigTest()
        {
            KarmaBotConfig defaultConfig = new KarmaBotConfig();

            KarmaBotConfig xmlConfig = XmlLoader.LoadKarmaBotConfig(
                Path.Combine( karmaBotDir, "Config", "SampleKarmaBotConfig.xml" )
            );

            Assert.AreEqual( defaultConfig.IncreaseCommandRegex, xmlConfig.IncreaseCommandRegex );
            Assert.AreEqual( defaultConfig.DecreaseCommandRegex, xmlConfig.DecreaseCommandRegex );
            Assert.AreEqual( defaultConfig.QueryCommand, xmlConfig.QueryCommand );
        }

        /// <summary>
        /// Ensures having an empty config results in default settings.
        /// </summary>
        [Test]
        public void EmptyConfigTest()
        {
            KarmaBotConfig defaultConfig = new KarmaBotConfig();

            KarmaBotConfig xmlConfig = XmlLoader.LoadKarmaBotConfig(
                Path.Combine( testConfigDir, "EmptyConfig.xml" )
            );

            Assert.AreEqual( defaultConfig.IncreaseCommandRegex, xmlConfig.IncreaseCommandRegex );
            Assert.AreEqual( defaultConfig.DecreaseCommandRegex, xmlConfig.DecreaseCommandRegex );
            Assert.AreEqual( defaultConfig.QueryCommand, xmlConfig.QueryCommand );
        }

        /// <summary>
        /// Ensures a valid config is loaded properly.
        /// </summary>
        [Test]
        public void ValidConfigTest()
        {
            KarmaBotConfig xmlConfig = XmlLoader.LoadKarmaBotConfig(
                Path.Combine( testConfigDir, "ValidConfig.xml" )
            );

            // Values from the XML file.
            Assert.AreEqual( @"^(\^\^(?<name>\S+)", xmlConfig.IncreaseCommandRegex );
            Assert.AreEqual( @"^VV(?<name>\S+)", xmlConfig.DecreaseCommandRegex );
            Assert.AreEqual( @"^!points\s+(?<name>\S+)", xmlConfig.QueryCommand );
        }
    }
}