//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Chaskis.Core;
using Chaskis.UnitTests.Common;
using NUnit.Framework;
using SethCS.Exceptions;

namespace Chaskis.UnitTests.CoreTests.Handlers.Disconnecting
{
    [TestFixture]
    public class DisconnectingEventHandlerTests
    {
        // ---------------- Fields ----------------

        private DisconnectingEventHandler uut;

        private IrcConfig ircConfig;

        private DisconnectingEventArgs responseReceived;

        // ---------------- Setup / Teardown ----------------

        [SetUp]
        public void TestSetup()
        {
            this.ircConfig = TestHelpers.GetTestIrcConfig();
            this.responseReceived = null;

            DisconnectingEventConfig config = new DisconnectingEventConfig
            {
                LineAction = DisconnectingFunction
            };

            this.uut = new DisconnectingEventHandler( config );
        }

        [TearDown]
        public void TestTeardown()
        {
        }

        // ---------------- Tests ----------------

        [Test]
        public void InvalidConfigTest()
        {
            Assert.Throws<ListedValidationException>(
                () => new DisconnectingEventHandler( new DisconnectingEventConfig() )
            );
        }

        [Test]
        public void ConstructionTest()
        {
            // Keep Handling should be true by default.
            Assert.IsTrue( this.uut.KeepHandling );
        }

        [Test]
        public void SuccessTest()
        {
            DisconnectingEventArgs expectedArgs = new DisconnectingEventArgs
            {
                Protocol = ChaskisEventProtocol.IRC,
                Server = "server"
            };

            HandlerArgs handlerArgs = ConstructArgs( expectedArgs.ToXml() );

            this.uut.HandleEvent( handlerArgs );

            Assert.IsNotNull( this.responseReceived );
            Assert.AreEqual( expectedArgs.Server, this.responseReceived.Server );
            Assert.AreEqual( expectedArgs.Protocol, this.responseReceived.Protocol );
        }

        [Test]
        public void FailureTest()
        {
            HandlerArgs handlerArgs = ConstructArgs( "lol" );

            this.uut.HandleEvent( handlerArgs );

            Assert.IsNull( this.responseReceived );
        }

        // ---------------- Test Helpers ----------------

        private void DisconnectingFunction( DisconnectingEventArgs args )
        {
            this.responseReceived = args;
        }

        private HandlerArgs ConstructArgs( string line )
        {
            HandlerArgs args = new HandlerArgs
            {
                Line = line,
                IrcConfig = this.ircConfig
            };

            return args;
        }
    }
}
