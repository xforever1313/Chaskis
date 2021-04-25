//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.IO;
using System.Xml;
using Chaskis.Plugins.HttpServer;
using Chaskis.UnitTests.Common;
using NUnit.Framework;

namespace Chaskis.UnitTests.PluginTests.Plugins.HttpServer
{
    [TestFixture]
    public class XmlLoaderTests
    {
        // ---------------- Fields ----------------

        private string sampleConfigPath;

        private string testFilesDir;

        // ---------------- Setup / Teardown ----------------

        [OneTimeSetUp]
        public void FixtureSetup()
        {
            this.sampleConfigPath = Path.Combine(
                TestHelpers.PluginDir,
                "HttpServer",
                "Config",
                "SampleHttpServerConfig.xml"
            );

            this.testFilesDir = Path.Combine(
                TestHelpers.PluginTestsDir,
                "Plugins",
                "HttpServer",
                "TestFiles"
            );
        }

        [OneTimeTearDown]
        public void FixtureTeardown()
        {
        }

        [SetUp]
        public void TestSetup()
        {
        }

        [TearDown]
        public void TestTeardown()
        {
        }

        // ---------------- Tests ----------------

        /// <summary>
        /// Ensures that the config in the XML file is the same
        /// configuration as the default HTTP Server settings.
        /// </summary>
        [Test]
        public void DefaultSettingsTest()
        {
            HttpServerConfig defaultConfig = new HttpServerConfig();
            HttpServerConfig loadedConfig = XmlLoader.LoadConfig( this.sampleConfigPath );

            Assert.AreEqual( defaultConfig, loadedConfig );
        }

        /// <summary>
        /// Ensures if the path is not found, an exception happens.
        /// </summary>
        [Test]
        public void FileNotFoundTest()
        {
            Assert.Throws<FileNotFoundException>( () => XmlLoader.LoadConfig( "nothere.xml" ) );
        }

        /// <summary>
        /// Ensures if our XML root is invalid, we get an exception.
        /// </summary>
        [Test]
        public void BadRootTest()
        {
            string filePath = Path.Combine( this.testFilesDir, "BadRoot.xml" );
            Assert.Throws<XmlException>( () => XmlLoader.LoadConfig( filePath ) );
        }

        /// <summary>
        /// Ensures that if an XML file is loaded with a valid config,
        /// the config is loaded properly.
        /// </summary>
        [Test]
        public void LoadTest()
        {
            HttpServerConfig expectedConfig = new HttpServerConfig
            {
                Port = 80
            };

            HttpServerConfig actualConfig = XmlLoader.LoadConfig( Path.Combine( this.testFilesDir, "GoodConfig.xml" ) );
            Assert.AreEqual( expectedConfig, actualConfig );
        }
    }
}
