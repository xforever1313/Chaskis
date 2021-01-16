//
//          Copyright Seth Hendrick 2016-2020.
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
            defaultConfig.ChannelsToSendTo.Add( "#channel1" );
            defaultConfig.ChannelsToSendTo.Add( "#channel2" );

            NewVersionNotifierConfig sampleConfig = XmlLoader.LoadConfigFromFile( this.sampleConfigPath );
            Assert.AreEqual( defaultConfig, sampleConfig );
        }

        [Test]
        public void FileNotFoundTest()
        {
            Assert.Throws<FileNotFoundException>( () => XmlLoader.LoadConfigFromFile( "lol" ) );
        }

        /// <summary>
        /// Ensures if we are missing properties, we still provide a valid default configuration.
        /// </summary>
        [Test]
        public void EmptyFileCreatesDefaultConfig()
        {
            const string xmlString =
@"<?xml version=""1.0"" encoding=""utf-8"" ?>

<!-- Empty config should result in the default settings -->
<newversionnotifierconfig xmlns=""https://files.shendrick.net/projects/chaskis/schemas/newversionnotifierconfigschema/2018/NewVersionNotifierConfigSchema.xsd"">
</newversionnotifierconfig>
";

            NewVersionNotifierConfig defaultConfig = new NewVersionNotifierConfig();
            NewVersionNotifierConfig emptyConfig = XmlLoader.LoadConfigFromString( xmlString );
            Assert.AreEqual( defaultConfig, emptyConfig );
        }

        [Test]
        public void IncorrectXmlRoot()
        {
            const string xmlString =
@"<?xml version=""1.0"" encoding=""utf-8"" ?>

<!-- Invalid root node, should throw exceptions -->
<lolconfig>
</lolconfig>
";

            Assert.Throws<XmlException>(
                () => XmlLoader.LoadConfigFromString( xmlString )
            );
        }

        [Test]
        public void ValidFileTest()
        {
            const string xmlString =
@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<newversionnotifierconfig xmlns=""https://files.shendrick.net/projects/chaskis/schemas/newversionnotifierconfigschema/2018/NewVersionNotifierConfigSchema.xsd"">
    <message>Test Message</message>
</newversionnotifierconfig>
";

            NewVersionNotifierConfig expectedConfig = new NewVersionNotifierConfig()
            {
                Message = "Test Message"
            };
            NewVersionNotifierConfig emptyConfig = XmlLoader.LoadConfigFromString( xmlString );

            Assert.AreEqual( expectedConfig, emptyConfig );
        }

        // ---------------- Test Helpers ----------------
    }
}
