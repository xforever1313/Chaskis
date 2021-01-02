//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System.Collections.Generic;
using System.IO;
using Chaskis.Core;
using Chaskis.Plugins.XmlBot;
using Chaskis.UnitTests.Common;
using NUnit.Framework;
using System.Xml;
using System;
using Moq;
using SethCS.Exceptions;

namespace Chaskis.UnitTests.PluginTests.Plugins.XmlBot
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
            TestHelpers.PluginTestsDir,
            "Plugins",
            "XmlBot",
            "TestFiles"
        );

        private IReadOnlyIrcConfig testConfig;

        private Mock<IIrcWriter> mockIrcWriter;

        private const string remoteUser = "someuser";

        // ---------------- Setup / Teardown ----------------

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            this.testConfig = TestHelpers.GetTestIrcConfig();
        }

        [SetUp]
        public void TestSetup()
        {
            this.mockIrcWriter = new Mock<IIrcWriter>( MockBehavior.Strict );
        }

        // ---------------- Tests ----------------

        /// <summary>
        /// Ensures if bad arguments get passed in, exceptions get thrown.
        /// </summary>
        [Test]
        public void ArgumentTest()
        {
            Assert.Throws<ArgumentNullException>( () => XmlLoader.LoadXmlBotConfig( goodConfig, null ) );

            // File Not Found
            Assert.Throws<FileNotFoundException>( () => XmlLoader.LoadXmlBotConfig( "derp", this.testConfig ) );
        }

        /// <summary>
        /// Ensures a valid config works correctly.
        /// </summary>
        [Test]
        public void GoodConfigTest()
        {
            // Let's hope that the pseudo-random numbers are the same for all .NET versions, or this is
            // going to end pooly.
            Random random = new Random( 1 );

            IList<IIrcHandler> handlers = XmlLoader.LoadXmlBotConfig( goodConfig, this.testConfig, random );

            // Slot 0 should be a Message Handler
            {
                MessageHandler handler0 = handlers[0] as MessageHandler;
                Assert.IsNotNull( handler0 );

                Assert.AreEqual( 0, handler0.CoolDown );
                Assert.AreEqual( "[Hh]ello {%nick%}", handler0.LineRegex );
                Assert.AreEqual( ResponseOptions.ChannelAndPms, handler0.ResponseOption );

                string ircString = TestHelpers.ConstructMessageString( remoteUser, this.testConfig.Channels[0], "Hello " + this.testConfig.Nick );

                // Expect a message to go out.
                {
                    string expectedMessage = "Hello " + remoteUser + "!";
                    this.mockIrcWriter.Setup( w => w.SendMessage( expectedMessage, this.testConfig.Channels[0] ) );
                    
                    handler0.HandleEvent( this.ConstructArgs( ircString ) );
                    this.mockIrcWriter.VerifyAll();
                }

                {
                    string expectedMessage = "Hello " + remoteUser + "!";
                    this.mockIrcWriter.Setup( w => w.SendMessage( expectedMessage, this.testConfig.Channels[0] ) );

                    handler0.HandleEvent( this.ConstructArgs( ircString ) );
                    this.mockIrcWriter.VerifyAll();
                }

                {
                    string expectedMessage = "Greetings " + remoteUser + "!";
                    this.mockIrcWriter.Setup( w => w.SendMessage( expectedMessage, this.testConfig.Channels[0] ) );

                    handler0.HandleEvent( this.ConstructArgs( ircString ) );
                    this.mockIrcWriter.VerifyAll();
                }
            }

            // Slot 1 should be a Message Handler
            {
                MessageHandler handler1 = handlers[1] as MessageHandler;
                Assert.IsNotNull( handler1 );

                Assert.AreEqual( 1, handler1.CoolDown );
                Assert.AreEqual( @"[Mm]y\s+name\s+is\s+(?<name>\w+)", handler1.LineRegex );
                Assert.AreEqual( ResponseOptions.ChannelOnly, handler1.ResponseOption );

                // Expect a message to go out.
                string expectedMessage = "Hello Seth!";
                this.mockIrcWriter.Setup( w => w.SendMessage( expectedMessage, this.testConfig.Channels[0] ) );

                string ircString = TestHelpers.ConstructMessageString( remoteUser, this.testConfig.Channels[0], "My name is Seth" );
                handler1.HandleEvent( this.ConstructArgs( ircString ) );
            }

            // Slot 2 should be a message Handler
            {
                MessageHandler handler2 = handlers[2] as MessageHandler;
                Assert.IsNotNull( handler2 );

                Assert.AreEqual( 0, handler2.CoolDown );
                Assert.AreEqual( @"^!{%nick%} {%channel%} {%user%}", handler2.LineRegex );
                Assert.AreEqual( ResponseOptions.ChannelAndPms, handler2.ResponseOption );

                // Expect a message to go out.
                string expectedMessage = string.Format(
                    "Hello {0}, I am {1} on channel {2}!",
                    remoteUser,
                    this.testConfig.Nick,
                    this.testConfig.Channels
                );
                this.mockIrcWriter.Setup( w => w.SendMessage( expectedMessage, this.testConfig.Channels[0] ) );

                string command = string.Format(
                    "!{0} {1} {2}",
                    this.testConfig.Nick,
                    this.testConfig.Channels,
                    remoteUser
                );

                string ircString = TestHelpers.ConstructMessageString( remoteUser, this.testConfig.Channels[0], command );

                handler2.HandleEvent( this.ConstructArgs( ircString ) );
            }

            // Slot 3 should be a message Handler
            {
                MessageHandler handler3 = handlers[3] as MessageHandler;
                Assert.IsNotNull( handler3 );

                Assert.AreEqual( 0, handler3.CoolDown );
                Assert.AreEqual( @"[Ww]hat is a(?<an>n)? (?<thing>\w+)", handler3.LineRegex );
                Assert.AreEqual( ResponseOptions.ChannelAndPms, handler3.ResponseOption );

                // Expect a message to go out.
                {
                    string expectedMessage = "A mouse is a thing!";
                    this.mockIrcWriter.Setup( w => w.SendMessage( expectedMessage, this.testConfig.Channels[0] ) );

                    string command = "What is a mouse";

                    string ircString = TestHelpers.ConstructMessageString( remoteUser, this.testConfig.Channels[0], command );
                    handler3.HandleEvent( this.ConstructArgs( ircString ) );
                }

                {
                    string expectedMessage = "An acorn is a thing!";
                    this.mockIrcWriter.Setup( w => w.SendMessage( expectedMessage, this.testConfig.Channels[0] ) );

                    string command = "What is an acorn";
                    string ircString = TestHelpers.ConstructMessageString( remoteUser, this.testConfig.Channels[0], command );
                    handler3.HandleEvent( this.ConstructArgs( ircString ) );
                }
            }

            // Slot 4 should be an action handler
            {
                ActionHandler handler4 = handlers[4] as ActionHandler;
                Assert.IsNotNull( handler4 );

                Assert.AreEqual( 0, handler4.CoolDown );
                Assert.AreEqual( @"^sighs$", handler4.LineRegex );
                Assert.AreEqual( ResponseOptions.ChannelAndPms, handler4.ResponseOption );

                // Expect a message to go out.
                {
                    string expectedMessage = $"@{remoteUser} - What's wrong?";
                    this.mockIrcWriter.Setup( w => w.SendMessage( expectedMessage, this.testConfig.Channels[0] ) );

                    string command = "sighs";

                    string ircString = TestHelpers.ConstructActionString( remoteUser, this.testConfig.Channels[0], command );
                    handler4.HandleEvent( this.ConstructArgs( ircString ) );
                }
            }
        }

        /// <summary>
        /// Ensures if we have a bad root node, we get an exception.
        /// </summary>
        [Test]
        public void BadRootNodeTest()
        {
            string fileName = Path.Combine( testFilesDir, "BadRootNode.xml" );
            Assert.Throws<XmlException>( () => XmlLoader.LoadXmlBotConfig( fileName, this.testConfig ) );
        }

        /// <summary>
        /// Ensures various formatting exceptions fails.
        /// </summary>
        [Test]
        public void FormattingTest()
        {
            this.DoFormatTest<ListedValidationException>( "NoCommand.xml" );
            this.DoFormatTest<ListedValidationException>( "EmptyCommand.xml" );
            this.DoFormatTest<ValidationException>( "NoResponse.xml" );
            this.DoFormatTest<ValidationException>( "EmptyResponse.xml" );
            this.DoFormatTest<FormatException>( "BadCooldown.xml" );
            this.DoFormatTest<FormatException>( "BadRespondTo.xml" );
        }

        private void DoFormatTest<TException>( string fileName ) where TException : Exception
        {
            fileName = Path.Combine( testFilesDir, fileName );

            // Check to see if file exists so we don't mistake our Exception for a
            // FileNotFoundException.
            Assert.IsTrue( File.Exists( fileName ) );
            Assert.Throws<TException>( () => XmlLoader.LoadXmlBotConfig( fileName, this.testConfig ) );
        }

        /// <summary>
        /// Ensures not having a cooldown or response is okay.
        /// </summary>
        [Test]
        public void NoCooldownOrResponseTest()
        {
            string fileName = Path.Combine( testFilesDir, "NoCooldownOrRespondTo.xml" );

            IList<IIrcHandler> handlers = XmlLoader.LoadXmlBotConfig( fileName, this.testConfig );

            Assert.AreEqual( 1, handlers.Count );

            MessageHandler handler = handlers[0] as MessageHandler;
            Assert.IsNotNull( handler );

            Assert.AreEqual( 0, handler.CoolDown ); // Defaulted to 0.
            Assert.AreEqual( ResponseOptions.ChannelAndPms, handler.ResponseOption ); // Defaulted to both.
        }

        private HandlerArgs ConstructArgs( string line )
        {
            HandlerArgs args = new HandlerArgs();
            args.Line = line;
            args.IrcWriter = this.mockIrcWriter.Object;
            args.IrcConfig = this.testConfig;

            return args;
        }
    }
}
