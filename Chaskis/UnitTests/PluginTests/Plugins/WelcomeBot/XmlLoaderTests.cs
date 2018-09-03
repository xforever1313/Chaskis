//
//          Copyright Seth Hendrick 2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.IO;
using System.Xml;
using Chaskis.Plugins.WelcomeBot;
using Chaskis.UnitTests.Common;
using NUnit.Framework;

namespace Chaskis.UnitTests.PluginTests.Plugins.WelcomeBot
{
    [TestFixture]
    public class XmlLoaderTests
    {
        // ---------------- Fields ----------------

        private string sampleConfigDir;

        private string testFileDir;

        // ---------------- Setup / Teardown ----------------

        [OneTimeSetUp]
        public void FixtureSetup()
        {
            this.sampleConfigDir = Path.Combine(
                TestHelpers.PluginDir,
                "WelcomeBot",
                "Config"
            );

            this.testFileDir = Path.Combine(
                TestHelpers.PluginTestsDir,
                "Plugins",
                "WelcomeBot",
                "TestFiles"
            );
        }

        // ---------------- Tests ----------------

        /// <summary>
        /// Ensures that the sample config matches the default settings.
        /// </summary>
        [Test]
        public void DefaultConfigTest()
        {
            WelcomeBotConfig expectedConfig = new WelcomeBotConfig();

            string xmlFile = Path.Combine( this.sampleConfigDir, "SampleWelcomeBotConfig.xml" );
            WelcomeBotConfig actualConfig = XmlLoader.LoadConfig( xmlFile );

            Assert.AreEqual( expectedConfig, actualConfig );
        }

        /// <summary>
        /// Ensures that a good config is parsed correctly.
        /// </summary>
        [Test]
        public void GoodConfigTest()
        {
            WelcomeBotConfig expectedConfig = new WelcomeBotConfig
            {
                EnableJoinMessages = false,
                EnableKickMessages = false,
                EnablePartMessages = false,
                KarmaBotIntegration = true
            };

            string xmlFile = Path.Combine( this.testFileDir, "GoodConfig.xml" );
            WelcomeBotConfig actualConfig = XmlLoader.LoadConfig( xmlFile );

            Assert.AreEqual( expectedConfig, actualConfig );
        }

        /// <summary>
        /// Ensures that if the XML file is empty, we just use the default settings.
        /// </summary>
        [Test]
        public void EmptyXmlTest()
        {
            WelcomeBotConfig expectedConfig = new WelcomeBotConfig();

            string xmlFile = Path.Combine( this.testFileDir, "EmptyConfig.xml" );
            WelcomeBotConfig actualConfig = XmlLoader.LoadConfig( xmlFile );

            Assert.AreEqual( expectedConfig, actualConfig );
        }

        /// <summary>
        /// Ensures if an XML file is not specified, we get an exception.
        /// </summary>
        [Test]
        public void FileNotFoundTest()
        {
            Assert.Throws<FileNotFoundException>( () => XmlLoader.LoadConfig( "derp" ) );
        }

        /// <summary>
        /// Ensures that if the XML root node is not correct, we get an Exception.
        /// </summary>
        [Test]
        public void BadRootTest()
        {
            WelcomeBotConfig expectedConfig = new WelcomeBotConfig();

            string xmlFile = Path.Combine( this.testFileDir, "BadRoot.xml" );
            Assert.Throws<XmlException>( () => XmlLoader.LoadConfig( xmlFile ) );
        }
    }
}
