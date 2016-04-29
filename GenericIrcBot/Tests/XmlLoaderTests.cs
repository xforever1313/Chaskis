
//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GenericIrcBot;
using NUnit.Framework;
using TestBot;

namespace Tests
{
    [TestFixture]
    public class XmlLoaderTests
    {
        // -------- Fields --------

        /// <summary>
        /// Where the test xml files are located.
        /// </summary>
        private static readonly string testXmlFiles = Path.Combine(
            "..", "..", "TestFiles"
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
            this.ircConfig.Channel = "#testchannel";
            this.ircConfig.Port = 6667;
            this.ircConfig.Nick = "testbot";
            this.ircConfig.UserName = "testbot";
            this.ircConfig.RealName = "test bot";
            this.ircConfig.Password = "apassword";
        }

        // -------- Tests --------

        /// <summary>
        /// Tests an XML file that is valid and contains a password.
        /// </summary>
        [Test]
        public void TestValidXmlWithPassword()
        {
            IIrcConfig config = XmlLoader.ParseIrcConfig(
                Path.Combine( testXmlFiles, "ValidIrcConfigWithPassword.xml" )
            );
            Assert.AreEqual( this.ircConfig, config );
        }

        /// <summary>
        /// Tests an XML file that is valid and contains an empty password node.
        /// </summary>
        [Test]
        public void TestValidXmlWithEmptyPassword()
        {
            this.ircConfig.Password = string.Empty;

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
            this.ircConfig.Password = string.Empty;

            IIrcConfig config = XmlLoader.ParseIrcConfig(
                Path.Combine( testXmlFiles, "ValidIrcConfigWithNoPassword.xml" )
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
            expectedConfig.Channel = this.ircConfig.Channel;

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
            Assert.Throws<ApplicationException>( () =>
                XmlLoader.ParseIrcConfig( Path.Combine( testXmlFiles, "InvalidIrcConfigEmptyUserName.xml" ) )
            );
        }

        /// <summary>
        /// Tests an XML file that is invalid since it has an invalid nickname.
        /// </summary>
        [Test]
        public void TestInvalidXmlWithEmptyNick()
        {
            Assert.Throws<ApplicationException>( () =>
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
            Assert.Throws<ApplicationException>( () =>
                XmlLoader.ParseIrcConfig( Path.Combine( testXmlFiles, "InvalidIrcConfigEmptyRealName.xml" ) )
            );
        }

        /// <summary>
        /// Tests an XML file that is invalid since it doesn't have a server.
        /// </summary>
        [Test]
        public void TestInvalidXmlWithNoServer()
        {
            Assert.Throws<ApplicationException>( () =>
                XmlLoader.ParseIrcConfig( Path.Combine( testXmlFiles, "InvalidIrcConfigNoServer.xml" ) )
            );
        }

        /// <summary>
        /// Tests an XML file that is invalid since it doesn't have a channel.
        /// </summary>
        [Test]
        public void TestInvalidXmlWithNoChannel()
        {
            Assert.Throws<ApplicationException>( () =>
                XmlLoader.ParseIrcConfig( Path.Combine( testXmlFiles, "InvalidIrcConfigNoChannel.xml" ) )
            );
        }
    }
}
