//
//          Copyright Seth Hendrick 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.IO;
using Chaskis.Plugins.MeetBot;
using NUnit.Framework;
using SethCS.Exceptions;

namespace Chaskis.UnitTests.PluginTests.Plugins.MeetBot
{
    [TestFixture]
    public class MeetBotConfigTests
    {
        // ---------------- Fields ----------------

        private string meetbotPath;

        // ---------------- Setup / Teardown ----------------

        [OneTimeSetUp]
        public void FixtureSetup()
        {
            this.meetbotPath = Path.Combine( "Chaskis", "Plugins", "MeetBot" );
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

        [Test]
        public void DefaultSettingsTest()
        {
            MeetBotConfig uut = new MeetBotConfig( this.meetbotPath );

            Assert.AreEqual( meetbotPath, uut.MeetBotRoot );
            Assert.IsNull( uut.CommandConfigPath ); // <- Null means use the default config file.
            Assert.IsTrue( uut.EnableBackups ); // <- Backups defaulted to true.
            Assert.AreEqual( 0, uut.Generators.Count ); // <- No generators by default.
        }

        /// <summary>
        /// Default settings should validate.
        /// </summary>
        [Test]
        public void DefaultSettingsValidateTest()
        {
            MeetBotConfig uut = new MeetBotConfig( this.meetbotPath );

            Assert.DoesNotThrow( () => uut.Validate() );
        }

        /// <summary>
        /// An invalid generator should not validate.
        /// </summary>
        [Test]
        public void InvalidGeneratorValidateTest()
        {
            MeetBotConfig uut = new MeetBotConfig( this.meetbotPath );

            GeneratorConfig config = new GeneratorConfig( this.meetbotPath )
            {
                Type = MeetingNotesGeneratorType.Unknown
            };

            uut.Generators.Add( config );

            Assert.Throws<ListedValidationException>( () => uut.Validate() );
        }
    }
}
