//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using Chaskis.Core;
using Chaskis.Plugins.WelcomeBot;
using Moq;
using NUnit.Framework;

namespace Chaskis.UnitTests.PluginTests.Plugins.WelcomeBot
{
    [TestFixture]
    public sealed class WelcomeBotInitTests
    {
        // ---------------- Fields ----------------

        private Mock<IInterPluginEventCreator> mockEventCreator;
        private Mock<IInterPluginEventSender> mockEventSender;

        private PluginInitor initor;

        // ---------------- Setup / Teardown ----------------

        [SetUp]
        public void TestSetup()
        {
            this.mockEventCreator = new Mock<IInterPluginEventCreator>( MockBehavior.Strict );
            this.mockEventSender = new Mock<IInterPluginEventSender>( MockBehavior.Strict );

            this.initor = new PluginInitor
            {
                ChaskisEventCreator = this.mockEventCreator.Object,
                ChaskisEventSender = this.mockEventSender.Object
            };
        }

        // ---------------- Tests ----------------

        /// <summary>
        /// Ensures that if everything is enabled, we get the correct count of handlers.
        /// </summary>
        [Test]
        public void AllEnabledTest()
        {
            WelcomeBotConfig config = new WelcomeBotConfig
            {
                EnableJoinMessages = true,
                EnableKickMessages = true,
                EnablePartMessages = true,
                KarmaBotIntegration = true
            };

            this.mockEventCreator.Setup(
                m => m.CreatePluginEventHandler( "karmabot", It.IsAny<Action<ChaskisEventHandlerLineActionArgs>>() )
            ).Returns(
                delegate ( string pluginName, Action<ChaskisEventHandlerLineActionArgs> action )
                {
                    return new InterPluginEventHandler( pluginName, "welcomebot", action );
                }
            );

            using( Chaskis.Plugins.WelcomeBot.WelcomeBot uut = new Chaskis.Plugins.WelcomeBot.WelcomeBot() )
            {
                uut.Init( this.initor, config );

                Assert.AreEqual( 4, uut.GetHandlers().Count );
            }
        }

        /// <summary>
        /// Ensures that if just join is enabled, that's the only thing that appears.
        /// </summary>
        [Test]
        public void JustJoinEnabledTest()
        {
            WelcomeBotConfig config = new WelcomeBotConfig
            {
                EnableJoinMessages = true,
                EnableKickMessages = false,
                EnablePartMessages = false,
                KarmaBotIntegration = false
            };

            using( Chaskis.Plugins.WelcomeBot.WelcomeBot uut = new Chaskis.Plugins.WelcomeBot.WelcomeBot() )
            {
                uut.Init( this.initor, config );

                Assert.AreEqual( 1, uut.GetHandlers().Count );
                Assert.IsTrue( uut.GetHandlers()[0] is JoinHandler );
            }
        }

        /// <summary>
        /// Ensures that if just kick is enabled, that's the only thing that appears.
        /// </summary>
        [Test]
        public void JustKickEnabledTest()
        {
            WelcomeBotConfig config = new WelcomeBotConfig
            {
                EnableJoinMessages = false,
                EnableKickMessages = true,
                EnablePartMessages = false,
                KarmaBotIntegration = false
            };

            using( Chaskis.Plugins.WelcomeBot.WelcomeBot uut = new Chaskis.Plugins.WelcomeBot.WelcomeBot() )
            {
                uut.Init( this.initor, config );

                Assert.AreEqual( 1, uut.GetHandlers().Count );
            }
        }

        /// <summary>
        /// Ensures that if just part is enabled, that's the only thing that appears.
        /// </summary>
        [Test]
        public void JustPartEnabledTest()
        {
            WelcomeBotConfig config = new WelcomeBotConfig
            {
                EnableJoinMessages = false,
                EnableKickMessages = false,
                EnablePartMessages = true,
                KarmaBotIntegration = false
            };

            using( Chaskis.Plugins.WelcomeBot.WelcomeBot uut = new Chaskis.Plugins.WelcomeBot.WelcomeBot() )
            {
                uut.Init( this.initor, config );

                Assert.AreEqual( 1, uut.GetHandlers().Count );
                Assert.IsTrue( uut.GetHandlers()[0] is PartHandler );
            }
        }

        /// <summary>
        /// Ensures that if just karmabot integration is enabled, nothing appears, as join must be
        /// enabled too.
        /// </summary>
        [Test]
        public void JustKarmaBotIntegrationEnabledTest()
        {
            WelcomeBotConfig config = new WelcomeBotConfig
            {
                EnableJoinMessages = false,
                EnableKickMessages = false,
                EnablePartMessages = false,
                KarmaBotIntegration = true
            };

            using( Chaskis.Plugins.WelcomeBot.WelcomeBot uut = new Chaskis.Plugins.WelcomeBot.WelcomeBot() )
            {
                uut.Init( this.initor, config );

                Assert.AreEqual( 0, uut.GetHandlers().Count );
            }
        }

        /// <summary>
        /// Ensures that if just karmabot integration and join is enabled, both handlers are added.
        /// </summary>
        [Test]
        public void KarmaBotIntegrationAndJoinEnabledTest()
        {
            WelcomeBotConfig config = new WelcomeBotConfig
            {
                EnableJoinMessages = true,
                EnableKickMessages = false,
                EnablePartMessages = false,
                KarmaBotIntegration = true
            };

            this.mockEventCreator.Setup(
                m => m.CreatePluginEventHandler( "karmabot", It.IsAny<Action<ChaskisEventHandlerLineActionArgs>>() )
            ).Returns(
                delegate ( string pluginName, Action<ChaskisEventHandlerLineActionArgs> action )
                {
                    return new InterPluginEventHandler( pluginName, "welcomebot", action );
                }
            );

            using( Chaskis.Plugins.WelcomeBot.WelcomeBot uut = new Chaskis.Plugins.WelcomeBot.WelcomeBot() )
            {
                uut.Init( this.initor, config );

                Assert.AreEqual( 2, uut.GetHandlers().Count );

                // In reality, we don't care about order, but this is easier to test.
                Assert.IsTrue( uut.GetHandlers()[0] is JoinHandler );
                Assert.IsTrue( uut.GetHandlers()[1] is InterPluginEventHandler );
            }
        }
    }
}
