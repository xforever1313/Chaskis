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
using System.Xml;
using System;
using Moq;

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

        private Mock<IIrcWriter> mockIrcWriter;

        private const string remoteUser = "someuser";

        // ---------------- Setup / Teardown ----------------

        [TestFixtureSetUp]
        public void TestFixtureSetup()
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
        /// Ensures a valid config works correctly.
        /// </summary>
        [Test]
        public void GoodConfigTest()
        {
            IList<IIrcHandler> handlers = XmlLoader.LoadXmlBotConfig( goodConfig, this.testConfig );

            // Slot 0 should be a Message Handler
            {
                MessageHandler handler0 = handlers[0] as MessageHandler;
                Assert.IsNotNull( handler0 );

                Assert.AreEqual( 0, handler0.CoolDown );
                Assert.AreEqual( "[Hh]ello {%nick%}", handler0.LineRegex );
                Assert.AreEqual( ResponseOptions.ChannelAndPms, handler0.ResponseOption );

                // Expect a message to go out.
                string expectedMessage = "Hello " + remoteUser + "!";
                this.mockIrcWriter.Setup( w => w.SendMessageToUser( expectedMessage, this.testConfig.Channel ) );

                handler0.HandleEvent(
                    TestHelpers.ConstructMessageString( remoteUser, this.testConfig.Channel, "Hello " + this.testConfig.Nick ),
                    this.testConfig,
                    this.mockIrcWriter.Object
                );
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
                this.mockIrcWriter.Setup( w => w.SendMessageToUser( expectedMessage, this.testConfig.Channel ) );

                handler1.HandleEvent(
                    TestHelpers.ConstructMessageString( remoteUser, this.testConfig.Channel, "My name is Seth" ),
                    this.testConfig,
                    this.mockIrcWriter.Object
                );
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
                    this.testConfig.Channel
                );
                this.mockIrcWriter.Setup( w => w.SendMessageToUser( expectedMessage, this.testConfig.Channel ) );

                string command = string.Format(
                    "!{0} {1} {2}",
                    this.testConfig.Nick,
                    this.testConfig.Channel,
                    remoteUser
                );

                handler2.HandleEvent(
                    TestHelpers.ConstructMessageString( remoteUser, this.testConfig.Channel, command ),
                    this.testConfig,
                    this.mockIrcWriter.Object
                );
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
                    this.mockIrcWriter.Setup( w => w.SendMessageToUser( expectedMessage, this.testConfig.Channel ) );

                    string command = "What is a mouse";
                    handler3.HandleEvent(
                        TestHelpers.ConstructMessageString( remoteUser, this.testConfig.Channel, command ),
                        this.testConfig,
                        this.mockIrcWriter.Object
                    );
                }

                {
                    string expectedMessage = "An acorn is a thing!";
                    this.mockIrcWriter.Setup( w => w.SendMessageToUser( expectedMessage, this.testConfig.Channel ) );

                    string command = "What is an acorn";
                    handler3.HandleEvent(
                        TestHelpers.ConstructMessageString( remoteUser, this.testConfig.Channel, command ),
                        this.testConfig,
                        this.mockIrcWriter.Object
                    );
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
            this.DoFormatTest<ArgumentNullException>( "NoCommand.xml" );
            this.DoFormatTest<ArgumentNullException>( "EmptyCommand.xml" );
            this.DoFormatTest<ArgumentNullException>( "NoResponse.xml" );
            this.DoFormatTest<ArgumentNullException>( "EmptyResponse.xml" );
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
    }
}
