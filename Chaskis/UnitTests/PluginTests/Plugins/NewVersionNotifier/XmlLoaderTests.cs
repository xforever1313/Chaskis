//
//          Copyright Seth Hendrick 2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.IO;
using System.Xml;
using Chaskis.Plugins.NewVersionNotifier;
using Chaskis.UnitTests.Common;
using NUnit.Framework;

namespace Chaskis.UnitTests.PluginTests.Plugins.NewVersionNotifier
{
    [TestFixture]
    public class XmlLoaderTests
    {
        // ---------------- Fields ----------------

        private string sampleConfigPath;

        private string testFilesPath;

        // ---------------- Setup / Teardown ----------------

        [OneTimeSetUp]
        public void FixtureSetup()
        {
            this.sampleConfigPath = Path.Combine(
                TestHelpers.PluginDir,
                "NewVersionNotifier",
                "Config",
                "SampleNewVersionNotifierConfig.xml"
            );

            this.testFilesPath = Path.Combine(
                TestHelpers.PluginTestsDir,
                "Plugins",
                "NewVersionNotifier",
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
        /// Ensures the sample configuration is the default configuration.
        /// </summary>
        [Test]
        public void DefaultTest()
        {
            NewVersionNotifierConfig defaultConfig = new NewVersionNotifierConfig();
            NewVersionNotifierConfig sampleConfig = XmlLoader.LoadConfig( this.sampleConfigPath );

            Assert.AreEqual( defaultConfig, sampleConfig );
        }

        [Test]
        public void FileNotFoundTest()
        {
            Assert.Throws<FileNotFoundException>( () => XmlLoader.LoadConfig( "lol" ) );
        }

        /// <summary>
        /// Ensures if we are missing properties, we still provide a valid default configuration.
        /// </summary>
        [Test]
        public void EmptyFileCreatesDefaultConfig()
        {
            NewVersionNotifierConfig defaultConfig = new NewVersionNotifierConfig();
            NewVersionNotifierConfig emptyConfig = XmlLoader.LoadConfig( Path.Combine( testFilesPath, "Empty.xml" ) );

            Assert.AreEqual( defaultConfig, emptyConfig );
        }

        [Test]
        public void IncorrectXmlRoot()
        {
            Assert.Throws<XmlException>(
                () => XmlLoader.LoadConfig( Path.Combine( testFilesPath, "BadRoot.xml" ) )
            );
        }

        [Test]
        public void ValidFileTest()
        {
            NewVersionNotifierConfig expectedConfig = new NewVersionNotifierConfig()
            {
                Message = "Test Message"
            };
            NewVersionNotifierConfig emptyConfig = XmlLoader.LoadConfig( Path.Combine( testFilesPath, "ValidFile.xml" ) );

            Assert.AreEqual( expectedConfig, emptyConfig );
        }

        // ---------------- Test Helpers ----------------
    }
}
