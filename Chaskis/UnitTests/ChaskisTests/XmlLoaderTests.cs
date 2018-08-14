//
//          Copyright Seth Hendrick 2016-2017.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//         http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Chaskis;
using ChaskisCore;
using NUnit.Framework;
using SethCS.Exceptions;

namespace Tests
{
    [TestFixture]
    public class XmlLoaderTests
    {
        // -------- Fields --------

        /// <summary>
        /// Where the test xml files are located.
        /// </summary>
        private static readonly string testXmlFiles =
            Path.Combine(
                TestContext.CurrentContext.TestDirectory, "..", "..", "..", "TestFiles"
            );

        /// <summary>
        /// The IRC Config to use.  Based off of the XML files.
        /// </summary>
        private IrcConfig ircConfig;

        // -------- Setup / Teardown --------

        [SetUp]
        public void TestSetup()
        {
            this.ircConfig = new IrcConfig();
            this.ircConfig.Server = "irc.testdomain.com";
            this.ircConfig.Channels.Add( "#testchannel" );
            this.ircConfig.Port = 6667;
            this.ircConfig.Nick = "testbot";
            this.ircConfig.UserName = "testbot";
            this.ircConfig.RealName = "test bot";
            this.ircConfig.QuitMessage = "I am being shut down!";
            this.ircConfig.RateLimit = 800;
        }

        // -------- Tests --------

        /// <summary>
        /// Tests an XML file that is valid and contains a password.
        /// </summary>
        public void TestValidXmlWithPassword()
        {
            // Can't do this one in Unit Test Land.  Because NUnit V3 no longer runs from the path of
            // the Test assembly, we can't put the file name to the password file in a cross-platform way
            // This needs to be done in regression-test land.
        }

        /// <summary>
        /// Tests an XML file that is valid and contains a password.
        /// </summary>
        [Test]
        public void TestValidXmlWithPasswordAndBridgeBots()
        {
            this.ircConfig.BridgeBots.Add( "telegrambot", @"^(?<bridgeUser>\w+):\s+(?<bridgeMessage>.+)" );
            this.ircConfig.BridgeBots.Add( "slackbot", @"^(?<bridgeUser>\w+)--(?<bridgeMessage>.+)" );

            IIrcConfig config = XmlLoader.ParseIrcConfig(
                Path.Combine( testXmlFiles, "ValidIrcConfigWithBridgeBots.xml" )
            );
            Assert.AreEqual( this.ircConfig, config );
        }

        [Test]
        public void TestValidXmlWithThreeChannelsAndPassword()
        {
            this.ircConfig.Channels.Add( "#mychannel" );
            this.ircConfig.Channels.Add( "#chaskis" );

            IIrcConfig config = XmlLoader.ParseIrcConfig(
                Path.Combine( testXmlFiles, "ValidIrcConfigWithThreeChannels.xml" )
            );
            Assert.AreEqual( this.ircConfig, config );
        }

        /// <summary>
        /// Tests an XML file that is valid and admins
        /// </summary>
        [Test]
        public void TestValidXmlWithAdmins()
        {
            this.ircConfig.Admins.Add( "person1" );
            this.ircConfig.Admins.Add( "person2" );

            IIrcConfig config = XmlLoader.ParseIrcConfig(
                Path.Combine( testXmlFiles, "ValidIrcConfigWithAdmins.xml" )
            );
            Assert.AreEqual( this.ircConfig, config );
        }

        /// <summary>
        /// Tests an XML file that is valid and contains an empty password node.
        /// </summary>
        [Test]
        public void TestValidXmlWithEmptyPassword()
        {
            this.ircConfig.NickServPassword = string.Empty;

            IIrcConfig config = XmlLoader.ParseIrcConfig(
                                    Path.Combine( testXmlFiles, "ValidIrcConfigWithEmptyPassword.xml" )
                                );
            Assert.AreEqual( this.ircConfig, config );
        }

        /// <summary>
        /// Tests an XML file that is valid and contains a password.
        /// </summary>
        [Test]
        public void TestValidXmlWithNoPassword()
        {
            this.ircConfig.NickServPassword = string.Empty;

            IIrcConfig config = XmlLoader.ParseIrcConfig(
                                    Path.Combine( testXmlFiles, "ValidIrcConfigWithNoPassword.xml" )
                                );
            Assert.AreEqual( this.ircConfig, config );
        }

        [Test]
        public void TestValidXmlWithRateLimit()
        {
            this.ircConfig.RateLimit = 0;

            IIrcConfig config = XmlLoader.ParseIrcConfig(
                                    Path.Combine( testXmlFiles, "ValidIrcConfigWithNoRateLimit.xml" )
                                );
            Assert.AreEqual( this.ircConfig, config );
        }

        /// <summary>
        /// Tests and XML file that is valid, but contains just the server
        /// and channel.  Everything else should be Irc defaults.
        /// </summary>
        [Test]
        public void TestValidXmlWithJustChannelAndServer()
        {
            // Our expected behavior is everything but channel and server to be defaults.
            IrcConfig expectedConfig = new IrcConfig();
            expectedConfig.Server = this.ircConfig.Server;
            foreach( string channel in this.ircConfig.Channels )
            {
                expectedConfig.Channels.Add( channel );
            }

            IIrcConfig config = XmlLoader.ParseIrcConfig(
                Path.Combine( testXmlFiles, "ValidIrcConfigJustChannelServer.xml" )
            );
            Assert.AreEqual( expectedConfig, config );
        }

        /// <summary>
        /// Tests an XML file that is invalid since it has an invalid user name.
        /// </summary>
        [Test]
        public void TestInvalidXmlWithUserName()
        {
            Assert.Throws<ValidationException>( () =>
                XmlLoader.ParseIrcConfig( Path.Combine( testXmlFiles, "InvalidIrcConfigEmptyUserName.xml" ) )
            );
        }

        /// <summary>
        /// Tests an XML file that is invalid since it has an invalid nickname.
        /// </summary>
        [Test]
        public void TestInvalidXmlWithEmptyNick()
        {
            Assert.Throws<ValidationException>( () =>
                XmlLoader.ParseIrcConfig( Path.Combine( testXmlFiles, "InvalidIrcConfigEmptyNick.xml" ) )
            );
        }

        /// <summary>
        /// Tests an XML file that is invalid since it has an invalid port.
        /// </summary>
        [Test]
        public void TestInvalidXmlWithEmptyPort()
        {
            Assert.Throws<FormatException>( () =>
                XmlLoader.ParseIrcConfig( Path.Combine( testXmlFiles, "InvalidIrcConfigEmptyPort.xml" ) )
            );
        }

        /// <summary>
        /// Tests an XML file that is invalid since it has an empty real name.
        /// </summary>
        [Test]
        public void TestInvalidXmlWithEmptyRealName()
        {
            Assert.Throws<ValidationException>( () =>
                XmlLoader.ParseIrcConfig( Path.Combine( testXmlFiles, "InvalidIrcConfigEmptyRealName.xml" ) )
            );
        }

        /// <summary>
        /// Tests an XML file that is invalid since it doesn't have a server.
        /// </summary>
        [Test]
        public void TestInvalidXmlWithNoServer()
        {
            Assert.Throws<ValidationException>( () =>
                XmlLoader.ParseIrcConfig( Path.Combine( testXmlFiles, "InvalidIrcConfigNoServer.xml" ) )
            );
        }

        /// <summary>
        /// Tests an XML file that is invalid since it doesn't have a channel.
        /// </summary>
        [Test]
        public void TestInvalidXmlWithNoChannel()
        {
            Assert.Throws<ValidationException>( () =>
                XmlLoader.ParseIrcConfig( Path.Combine( testXmlFiles, "InvalidIrcConfigNoChannel.xml" ) )
            );
        }

        /// <summary>
        /// Tests an XML file that is invalid since it has a channel that is empty.
        /// </summary>
        [Test]
        public void TestInvalidXmlWithEmptyChannel()
        {
            Assert.Throws<ValidationException>( () =>
                XmlLoader.ParseIrcConfig( Path.Combine( testXmlFiles, "InvalidIrcConfigWithEmptyChannel.xml" ) )
            );
        }

        /// <summary>
        /// Tests an XML file that is invalid since it has an empty admin
        /// </summary>
        [Test]
        public void TestInvalidXmlEmptyAdmin()
        {
            Assert.Throws<ValidationException>( () =>
                XmlLoader.ParseIrcConfig( Path.Combine( testXmlFiles, "InvalidIrcConfigWithEmptyAdmins.xml" ) )
            );
        }

        /// <summary>
        /// Ensures an exception is thrown when the root node is not correct.
        /// </summary>
        [Test]
        public void TestIrcConfigBadRootName()
        {
            Assert.Throws<XmlException>( () =>
                XmlLoader.ParseIrcConfig( Path.Combine( testXmlFiles, "InvalidIrcConfigBadRootNode.xml" ) )
            );
        }

        // ---- Plugin Xml ----

        /// <summary>
        /// Ensures the plugin loader works for a valid plugin config.
        /// </summary>
        [Test]
        public void TestValidPluginConfig()
        {
            IList<AssemblyConfig> configs = XmlLoader.ParsePluginConfig( Path.Combine( testXmlFiles, "ValidPluginConfig.xml" ) );
            Assert.AreEqual( 2, configs.Count );

            Assert.AreEqual( "/home/me/.config/GenericIrcBot/plugins/TestPlugin/TestPlugin.dll", configs[0].AssemblyPath );
            Assert.AreEqual( 0, configs[0].BlackListedChannels.Count );

            Assert.AreEqual( "/home/me/.config/GenericIrcBot/plugins/TestPlugin/TestPlugin2.dll", configs[1].AssemblyPath );
            Assert.AreEqual( 2, configs[1].BlackListedChannels.Count );
            Assert.AreEqual( "#blacklist", configs[1].BlackListedChannels[0] );
            Assert.AreEqual( "#evil", configs[1].BlackListedChannels[1] );
        }

        /// <summary>
        /// Ensures that an empty plugin config is okay, we'll just have zero plugins to load.
        /// </summary>
        [Test]
        public void TestValidPluginConfigNoPlugins()
        {
            IList<AssemblyConfig> configs = XmlLoader.ParsePluginConfig( Path.Combine( testXmlFiles, "ValidPluginConfigEmpty.xml" ) );
            Assert.AreEqual( 0, configs.Count );
        }

        /// <summary>
        /// Ensures an exception is thrown when the root node is not correct.
        /// </summary>
        [Test]
        public void TestInvalidPluginConfigBadRootName()
        {
            Assert.Throws<XmlException>( () =>
                XmlLoader.ParsePluginConfig( Path.Combine( testXmlFiles, "InvalidPluginConfigBadRootNode.xml" ) )
            );
        }

        /// <summary>
        /// Ensures an empty path in the XML resutls in an exception.
        /// </summary>
        [Test]
        public void TestInvalidPluginConfigNoPath()
        {
            Assert.Throws<ArgumentNullException>( () =>
                XmlLoader.ParsePluginConfig( Path.Combine( testXmlFiles, "InvalidPluginConfigEmptyPath.xml" ) )
            );
        }
    }
}