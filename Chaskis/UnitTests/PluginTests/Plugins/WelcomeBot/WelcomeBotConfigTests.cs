//
//          Copyright Seth Hendrick 2018.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Chaskis.UnitTests.Common;
using NUnit.Framework;
using WelcomeBot;

namespace Chaskis.UnitTests.PluginTests.Plugins.WelcomeBot
{
    [TestFixture]
    public class WelcomeBotConfigTests
    {
        // ---------------- Tests ----------------

        /// <summary>
        /// Ensures the default settings are correct.
        /// </summary>
        [Test]
        public void DefaultSettingsTest()
        {
            WelcomeBotConfig uut = new WelcomeBotConfig();

            Assert.IsTrue( uut.EnableJoinMessages );
            Assert.IsTrue( uut.EnableKickMessages );
            Assert.IsTrue( uut.EnablePartMessages );

            // Anything that depends on other plugins is disabled by default.
            Assert.IsFalse( uut.KarmaBotIntegration );
        }

        [Test]
        public void EqualsTest()
        {
            WelcomeBotConfig uut1 = new WelcomeBotConfig();
            WelcomeBotConfig uut2 = new WelcomeBotConfig();

            TestHelpers.EqualsTest( uut1, uut2 );
            Assert.IsFalse( uut1.Equals( 1 ) );

            // Start changing things.
            uut1.EnableJoinMessages = ( uut2.EnableJoinMessages == false );
            TestHelpers.NotEqualsTest( uut1, uut2 );
            uut1 = new WelcomeBotConfig();

            uut1.EnableKickMessages = ( uut2.EnableKickMessages == false );
            TestHelpers.NotEqualsTest( uut1, uut2 );
            uut1 = new WelcomeBotConfig();

            uut1.EnablePartMessages = ( uut2.EnablePartMessages == false );
            TestHelpers.NotEqualsTest( uut1, uut2 );
            uut1 = new WelcomeBotConfig();

            uut1.KarmaBotIntegration = ( uut2.KarmaBotIntegration == false );
            TestHelpers.NotEqualsTest( uut1, uut2 );
            uut1 = new WelcomeBotConfig();
        }

        [Test]
        public void CloneTest()
        {
            WelcomeBotConfig uut1 = new WelcomeBotConfig();
            WelcomeBotConfig clone = uut1.Clone();

            TestHelpers.CloneTest( uut1, clone );
        }
    }
}
