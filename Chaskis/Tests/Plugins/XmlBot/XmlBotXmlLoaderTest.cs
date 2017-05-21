//
//          Copyright Seth Hendrick 2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;
using System.IO;
using ChaskisCore;
using Chaskis.Plugins.XmlBot;
using NUnit.Framework;

namespace Tests.Plugins.XmlBot
{
    [TestFixture]
    public class XmlBotXmlLoaderTest
    {
        // ---------------- Fields ----------------

        private static readonly string goodConfig = Path.Combine(
            TestHelpers.PluginDir,
            "XmlBot",
            "Config",
            "SampleXmlBotConfig.xml"
        );

        private static readonly string testFilesDir = Path.Combine(
            TestHelpers.TestsBaseDir,
            "Plugins",
            "XmlBot",
            "TestFiles"
        );

        private IIrcConfig testConfig;

        // ---------------- Setup / Teardown ----------------

        [TestFixtureSetUp]
        public void TestFixtureSetup()
        {
            this.testConfig = TestHelpers.GetTestIrcConfig();
        }

        // ---------------- Tests ----------------
        
        /// <summary>
        /// Ensures a valid config works correctly.
        /// </summary>
        [Test]
        public void GoodConfigTest()
        {
            IList<IIrcHandler> handlers = XmlLoader.LoadXmlBotConfig( goodConfig );

            // Slot 0 should be a Message Handler
            {
                MessageHandler handler0 = handlers[0] as MessageHandler;
                Assert.IsNotNull( handler0 );

                Assert.AreEqual( 0, handler0.CoolDown );
                Assert.AreEqual( "[Hh]ello {%nick%}", handler0.LineRegex );
                Assert.AreEqual( ResponseOptions.ChannelAndPms, handler0.ResponseOption );
            }

            // Slot 1 should be a Message Handler
            {
                MessageHandler handler1 = handlers[1] as MessageHandler;
                Assert.IsNotNull( handler1 );

                Assert.AreEqual( 1, handler1.CoolDown );
                Assert.AreEqual( @"[Mm]y\s+name\s+is\s+(?<name>\w+)", handler1.LineRegex );
                Assert.AreEqual( ResponseOptions.ChannelOnly, handler1.ResponseOption );
            }
        }
    }
}
