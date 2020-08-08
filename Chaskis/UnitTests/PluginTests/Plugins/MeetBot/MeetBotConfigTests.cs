//
//          Copyright Seth Hendrick 2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Chaskis.Plugins.MeetBot;
using NUnit.Framework;

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
    }
}
