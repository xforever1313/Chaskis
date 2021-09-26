//
//          Copyright Seth Hendrick 2016-2021.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using Chaskis.Core;
using Chaskis.UnitTests.Common;
using Moq;
using NUnit.Framework;
using SethCS.Exceptions;

namespace Chaskis.UnitTests.CoreTests.Handlers.Send
{
    [TestFixture]
    public sealed class SendEventHandlerTests
    {
        // ---------------- Fields ----------------

        private SendEventHandler uut;

        private IrcConfig ircConfig;

        private Mock<IIrcWriter> ircWriter;

        private SendEventArgs responseReceived;

        private const string command = "!some command";

        // ---------------- Setup / Teardown ----------------

        [SetUp]
        public void TestSetup()
        {
            this.ircConfig = TestHelpers.GetTestIrcConfig();
            this.ircWriter = new Mock<IIrcWriter>( MockBehavior.Strict );
            this.responseReceived = null;

            SendEventConfig config = new SendEventConfig
            {
                LineAction = SendFunction
            };

            this.uut = new SendEventHandler( config );
        }

        [TearDown]
        public void TestTeardown()
        {
            this.ircWriter = null;
        }

        // ---------------- Tests ----------------

        [Test]
        public void InvalidConfigTest()
        {
            Assert.Throws<ListedValidationException>(
                () => new SendEventHandler( new SendEventConfig() )
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
            SendEventArgs expectedArgs = new SendEventArgs
            {
                Protocol = ChaskisEventProtocol.IRC,
                Server = "server",
                Writer = this.ircWriter.Object,

                Command = command
            };

            HandlerArgs handlerArgs = ConstructArgs( expectedArgs.ToXml() );

            this.uut.HandleEvent( handlerArgs );

            Assert.IsNotNull( this.responseReceived );
            Assert.AreEqual( expectedArgs.Server, this.responseReceived.Server );
            Assert.AreEqual( expectedArgs.Protocol, this.responseReceived.Protocol );
            Assert.AreEqual( expectedArgs.Command, this.responseReceived.Command );
            Assert.AreSame( expectedArgs.Writer, this.responseReceived.Writer );
        }

        [Test]
        public void FailureTest()
        {
            HandlerArgs handlerArgs = ConstructArgs( "lol" );

            this.uut.HandleEvent( handlerArgs );

            Assert.IsNull( this.responseReceived );
        }

        // ---------------- Test Helpers ----------------

        private void SendFunction( SendEventArgs args )
        {
            this.responseReceived = args;
        }

        private HandlerArgs ConstructArgs( string line )
        {
            HandlerArgs args = new HandlerArgs
            {
                Line = line,
                IrcWriter = this.ircWriter.Object,
                IrcConfig = this.ircConfig
            };

            return args;
        }
    }
}
