//
//          Copyright Seth Hendrick 2016-2020.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Chaskis.Cli;
using Chaskis.Core;
using NUnit.Framework;
using SethCS.Exceptions;

namespace Chaskis.UnitTests.ChaskisTests
{
    [TestFixture]
    public class XmlLoaderTests
    {
        // ---------------- Fields ----------------

        /// <summary>
        /// The IRC Config to use.  Based off of the XML files.
        /// </summary>
        private IrcConfig ircConfig;

        // ---------------- Setup / Teardown ----------------

        [SetUp]
        public void TestSetup()
        {
            this.ircConfig = new IrcConfig();
            this.ircConfig.Server = "irc.testdomain.com";
            this.ircConfig.Channels.Add( "#testchannel" );
            this.ircConfig.Port = 6667;
            this.ircConfig.UseSsl = false;
            this.ircConfig.Nick = "testbot";
            this.ircConfig.UserName = "testbot";
            this.ircConfig.RealName = "test bot";
            this.ircConfig.QuitMessage = "I am being shut down!";
            this.ircConfig.RateLimit = TimeSpan.FromMilliseconds( 800 );
        }

        // ---------------- Tests ----------------

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
        public void TestValidXmlWithAndBridgeBots()
        {
            this.ircConfig.BridgeBots.Add( "telegrambot", @"^(?<bridgeUser>\w+):\s+(?<bridgeMessage>.+)" );
            this.ircConfig.BridgeBots.Add( "slackbot", @"^(?<bridgeUser>\w+)--(?<bridgeMessage>.+)" );

            const string xmlString =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<ircbotconfig xmlns=""https://files.shendrick.net/projects/chaskis/schemas/chaskisircconfig/2017/chaskisircconfig.xsd"">
	<server>irc.testdomain.com</server>
    <channels>
        <channel>#testchannel</channel>
    </channels>
	<port>6667</port>
	<username>testbot</username>
	<nick>testbot</nick>
	<realname>test bot</realname>
    <ratelimit>800</ratelimit>
    <quitmessage>I am being shut down!</quitmessage>
    <bridgebots>
        <bridgebot>
            <botname>telegrambot</botname>
            <botregex><![CDATA[^(?<bridgeUser>\w+):\s+(?<bridgeMessage>.+)]]></botregex>
        </bridgebot>
        <bridgebot>
            <botname>slackbot</botname>
            <botregex><![CDATA[^(?<bridgeUser>\w+)--(?<bridgeMessage>.+)]]></botregex>
        </bridgebot>
    </bridgebots>
</ircbotconfig>
";

            IReadOnlyIrcConfig config = XmlLoader.ParseIrcConfigFromString( xmlString );
            Assert.AreEqual( this.ircConfig, config );
        }

        [Test]
        public void TestValidXmlWithThreeChannels()
        {
            this.ircConfig.Channels.Add( "#mychannel" );
            this.ircConfig.Channels.Add( "#chaskis" );

            const string xmlString =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<ircbotconfig xmlns=""https://files.shendrick.net/projects/chaskis/schemas/chaskisircconfig/2017/chaskisircconfig.xsd"">
	<server>irc.testdomain.com</server>
	<channels>
        <channel>#testchannel</channel>
        <channel>#mychannel</channel>
        <channel>#chaskis</channel>
    </channels>
	<port>6667</port>
	<username>testbot</username>
	<nick>testbot</nick>
	<realname>test bot</realname>
    <ratelimit>800</ratelimit>
    <quitmessage>I am being shut down!</quitmessage>
</ircbotconfig>
";

            IReadOnlyIrcConfig config = XmlLoader.ParseIrcConfigFromString( xmlString );
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

            this.ircConfig.UseSsl = true;

            const string xmlString =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<ircbotconfig xmlns=""https://files.shendrick.net/projects/chaskis/schemas/chaskisircconfig/2017/chaskisircconfig.xsd"">
	<server>irc.testdomain.com</server>
    <channels>
	    <channel>#testchannel</channel>
    </channels>
	<port>6667</port>
    <usessl>true</usessl>
	<username>testbot</username>
	<nick>testbot</nick>
	<realname>test bot</realname>
    <ratelimit>800</ratelimit>
    <quitmessage>I am being shut down!</quitmessage>
    <admins>
        <admin>person1</admin>
        <admin>PERSON2</admin>
    </admins>
</ircbotconfig>
";

            IReadOnlyIrcConfig config = XmlLoader.ParseIrcConfigFromString( xmlString );
            Assert.AreEqual( this.ircConfig, config );
        }

        [Test]
        public void TestValidXmlWithInlinePasswords()
        {
            this.ircConfig.ServerPasswordMethod = PasswordMethod.Inline;
            this.ircConfig.ServerPassword = "serverpass";
            this.ircConfig.NickServNick = "SomeNickServ";
            this.ircConfig.NickServPasswordMethod = PasswordMethod.Inline;
            this.ircConfig.NickServPassword = "nickservpass";
            this.ircConfig.NickServMessage = "IDENTIFY AS {%password%}";

            string xmlString =
$@"<?xml version=""1.0"" encoding=""utf-8""?>
<ircbotconfig xmlns=""https://files.shendrick.net/projects/chaskis/schemas/chaskisircconfig/2017/chaskisircconfig.xsd"">
	<server>irc.testdomain.com</server>
    <channels>
	    <channel>#testchannel</channel>
    </channels>
	<port>6667</port>
	<username>testbot</username>
	<nick>testbot</nick>
	<realname>test bot</realname>
    <ratelimit>800</ratelimit>
    <serverpassword method=""inline"">{this.ircConfig.ServerPassword}</serverpassword>
    <nickservpassword method=""inline"">{this.ircConfig.NickServPassword}</nickservpassword>
    <nickservnick>{this.ircConfig.NickServNick}</nickservnick>
    <nickservmessage>{this.ircConfig.NickServMessage}</nickservmessage>
    <quitmessage>I am being shut down!</quitmessage>
</ircbotconfig>
";
            IReadOnlyIrcConfig config = XmlLoader.ParseIrcConfigFromString( xmlString );
            Assert.AreEqual( this.ircConfig, config );
            Assert.AreEqual( this.ircConfig.ServerPassword, config.GetServerPassword() );
            Assert.AreEqual( this.ircConfig.NickServPassword, config.GetNickServPassword() );
            Assert.AreEqual( $"IDENTIFY AS {this.ircConfig.NickServPassword}", config.GetNickServMessage() );
        }

        [Test]
        public void TestValidXmlWithEnvVarPasswords()
        {
            const string expectedServerPass = "serverpass";
            const string expectedNickServPass = "nickservpass";

            this.ircConfig.ServerPasswordMethod = PasswordMethod.EnvVar;
            this.ircConfig.ServerPassword = "CHASKIS_SERVER_PASS";
            this.ircConfig.NickServPasswordMethod = PasswordMethod.EnvVar;
            this.ircConfig.NickServPassword = "CHASKIS_NICKSERV_PASS";
            this.ircConfig.NickServMessage = "IDENTIFY AS {%password%}";

            string oldServerEnvVar = Environment.GetEnvironmentVariable( this.ircConfig.ServerPassword );
            string oldNickServEnvVar = Environment.GetEnvironmentVariable( this.ircConfig.NickServPassword );
            try
            {
                Environment.SetEnvironmentVariable( this.ircConfig.ServerPassword, expectedServerPass );
                Environment.SetEnvironmentVariable( this.ircConfig.NickServPassword, expectedNickServPass );

                string xmlString =
    $@"<?xml version=""1.0"" encoding=""utf-8""?>
<ircbotconfig xmlns=""https://files.shendrick.net/projects/chaskis/schemas/chaskisircconfig/2017/chaskisircconfig.xsd"">
	<server>irc.testdomain.com</server>
    <channels>
	    <channel>#testchannel</channel>
    </channels>
	<port>6667</port>
	<username>testbot</username>
	<nick>testbot</nick>
	<realname>test bot</realname>
    <ratelimit>800</ratelimit>
    <serverpassword method=""envvar"">{this.ircConfig.ServerPassword}</serverpassword>
    <nickservpassword method=""envvar"">{this.ircConfig.NickServPassword}</nickservpassword>
    <nickservmessage>{this.ircConfig.NickServMessage}</nickservmessage>
    <quitmessage>I am being shut down!</quitmessage>
</ircbotconfig>
";
                IReadOnlyIrcConfig config = XmlLoader.ParseIrcConfigFromString( xmlString );
                Assert.AreEqual( this.ircConfig, config );
                Assert.AreEqual( expectedServerPass, config.GetServerPassword() );
                Assert.AreEqual( expectedNickServPass, config.GetNickServPassword() );
                Assert.AreEqual( $"IDENTIFY AS {expectedNickServPass}", config.GetNickServMessage() );
            }
            finally
            {
                Environment.SetEnvironmentVariable( this.ircConfig.ServerPassword, oldServerEnvVar );
                Environment.SetEnvironmentVariable( this.ircConfig.NickServPassword, oldNickServEnvVar );
            }
        }

        /// <summary>
        /// Tests an XML file that is valid and contains an empty password node.
        /// </summary>
        [Test]
        public void TestValidXmlWithEmptyPassword()
        {
            this.ircConfig.NickServPassword = string.Empty;
            this.ircConfig.NickServMessage = string.Empty;
            this.ircConfig.NickServNick = string.Empty;

            const string xmlString =
@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<ircbotconfig xmlns=""https://files.shendrick.net/projects/chaskis/schemas/chaskisircconfig/2017/chaskisircconfig.xsd"">
    <server>irc.testdomain.com</server>
    <channels>
        <channel>#testchannel</channel>
    </channels>
    <port>6667</port>
    <username>testbot</username>
    <nick>testbot</nick>
    <realname>test bot</realname>
    <serverpassword></serverpassword>
    <nickservpassword></nickservpassword>
    <nickservnick></nickservnick>
    <nickservmessage></nickservmessage>
    <ratelimit>800</ratelimit>
    <quitmessage>I am being shut down!</quitmessage>
    <!--
    <bridgebots>
        <bridgebot>
            <botname>telegrambot\d*</botname>
            <botregex><![CDATA[(?<bridgeUser>\w+):\s+(?<bridgeMessage>.+)]]></botregex>
        </bridgebot>
        <bridgebot>
            <botname>slackbot</botname>
            <botregex><![CDATA[(?<bridgeUser>\w+):\s+(?<bridgeMessage>.+)]]></botregex>
        </bridgebot>
    </bridgebots>
    -->
</ircbotconfig>
";

            IReadOnlyIrcConfig config = XmlLoader.ParseIrcConfigFromString( xmlString );
            Assert.AreEqual( this.ircConfig, config );
        }

        /// <summary>
        /// Tests an XML file that is valid and contains a password.
        /// </summary>
        [Test]
        public void TestValidXmlWithNoPassword()
        {
            this.ircConfig.NickServPassword = string.Empty;

            const string xmlString =
@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<ircbotconfig xmlns=""https://files.shendrick.net/projects/chaskis/schemas/chaskisircconfig/2017/chaskisircconfig.xsd"">
    <server>irc.testdomain.com</server>
    <channels>
        <channel>#testchannel</channel>
    </channels>
    <port>6667</port>
    <username>testbot</username>
    <nick>testbot</nick>
    <realname>test bot</realname>
    <ratelimit>800</ratelimit>
    <quitmessage>I am being shut down!</quitmessage>
</ircbotconfig>
";

            IReadOnlyIrcConfig config = XmlLoader.ParseIrcConfigFromString( xmlString );
            Assert.AreEqual( this.ircConfig, config );
        }

        [Test]
        public void TestValidXmlWithNoRateLimit()
        {
            this.ircConfig.RateLimit = TimeSpan.Zero;

            const string xmlString =
@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<ircbotconfig xmlns=""https://files.shendrick.net/projects/chaskis/schemas/chaskisircconfig/2017/chaskisircconfig.xsd"">
    <server>irc.testdomain.com</server>
    <channels>
        <channel>#testchannel</channel>
    </channels>
    <port>6667</port>
    <username>testbot</username>
    <nick>testbot</nick>
    <realname>test bot</realname>
    <quitmessage>I am being shut down!</quitmessage>
</ircbotconfig>
";

            IReadOnlyIrcConfig config = XmlLoader.ParseIrcConfigFromString( xmlString );
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

            const string xmlString =
@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<!--
This is technically a valid config, as everything else will get default values.
However, it is highly recommended not to use this config, which is why
this will fail the schema but pass Chaskis's IrcConfig validation.
-->

<ircbotconfig xmlns=""https://files.shendrick.net/projects/chaskis/schemas/chaskisircconfig/2017/chaskisircconfig.xsd"">
    <server>irc.testdomain.com</server>
    <channels>
        <channel>#testchannel</channel>
    </channels>
</ircbotconfig>
";

            IReadOnlyIrcConfig config = XmlLoader.ParseIrcConfigFromString( xmlString );
            Assert.AreEqual( expectedConfig, config );
        }

        /// <summary>
        /// Tests an XML file that is invalid since it has an invalid user name.
        /// </summary>
        [Test]
        public void TestInvalidXmlWithUserName()
        {
            const string xmlString =
@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<ircbotconfig xmlns=""https://files.shendrick.net/projects/chaskis/schemas/chaskisircconfig/2017/chaskisircconfig.xsd"">
    <server>irc.testdomain.com</server>
    <channels>
        <channel>#testchannel</channel>
    </channels>
    <port>6667</port>
    <username></username>
    <nick>testbot</nick>
    <realname>test bot</realname>
    <password>apassword</password>
</ircbotconfig>
";

            Assert.Throws<ValidationException>( () =>
                XmlLoader.ParseIrcConfigFromString( xmlString )
            );
        }

        /// <summary>
        /// Tests an XML file that is invalid since it has an invalid nickname.
        /// </summary>
        [Test]
        public void TestInvalidXmlWithEmptyNick()
        {
            const string xmlString =
@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<ircbotconfig xmlns=""https://files.shendrick.net/projects/chaskis/schemas/chaskisircconfig/2017/chaskisircconfig.xsd"">
    <server>irc.testdomain.com</server>
    <channels>
        <channel>#testchannel</channel>
    </channels>
    <port>6667</port>
    <username>testbot</username>
    <nick></nick>
    <realname>test bot</realname>
    <password>apassword</password>
</ircbotconfig>
";

            Assert.Throws<ValidationException>( () =>
                XmlLoader.ParseIrcConfigFromString( xmlString )
            );
        }

        /// <summary>
        /// Tests an XML file that is invalid since it has an invalid port.
        /// </summary>
        [Test]
        public void TestInvalidXmlWithEmptyPort()
        {
            const string xmlString =
@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<ircbotconfig xmlns=""https://files.shendrick.net/projects/chaskis/schemas/chaskisircconfig/2017/chaskisircconfig.xsd"">
    <server>irc.testdomain.com</server>
    <channels>
        <channel>#testchannel</channel>
    </channels>
    <port></port>
    <username>testbot</username>
    <nick>testbot</nick>
    <realname>test bot</realname>
    <password>apassword</password>
</ircbotconfig>
";

            Assert.Throws<FormatException>( () =>
                XmlLoader.ParseIrcConfigFromString( xmlString )
            );
        }

        /// <summary>
        /// Tests an XML file that is invalid since it has an empty real name.
        /// </summary>
        [Test]
        public void TestInvalidXmlWithEmptyRealName()
        {
            const string xmlString =
@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<ircbotconfig xmlns=""https://files.shendrick.net/projects/chaskis/schemas/chaskisircconfig/2017/chaskisircconfig.xsd"">
    <server>irc.testdomain.com</server>
    <channels>
        <channel>#testchannel</channel>
    </channels>
    <port>6667</port>
    <username>testbot</username>
    <nick>testbot</nick>
    <realname></realname>
    <password>apassword</password>
</ircbotconfig>
";

            Assert.Throws<ValidationException>( () =>
                XmlLoader.ParseIrcConfigFromString( xmlString )
            );
        }

        /// <summary>
        /// Tests an XML file that is invalid since it doesn't have a server.
        /// </summary>
        [Test]
        public void TestInvalidXmlWithNoServer()
        {
            const string xmlString =
@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<ircbotconfig xmlns=""https://files.shendrick.net/projects/chaskis/schemas/chaskisircconfig/2017/chaskisircconfig.xsd"">
    <channels>
        <channel>#testchannel</channel>
    </channels>
    <port>6667</port>
    <username>testbot</username>
    <nick>testbot</nick>
    <realname>test bot</realname>
    <password>apassword</password>
</ircbotconfig>
";

            Assert.Throws<ValidationException>( () =>
                XmlLoader.ParseIrcConfigFromString( xmlString )
            );
        }

        /// <summary>
        /// Tests an XML file that is invalid since it doesn't have a channel.
        /// </summary>
        [Test]
        public void TestInvalidXmlWithNoChannel()
        {
            const string xmlString =
@"<?xml version=""1.0"" encoding=""utf-8"" ?>
<ircbotconfig xmlns=""https://files.shendrick.net/projects/chaskis/schemas/chaskisircconfig/2017/chaskisircconfig.xsd"">
    <server>irc.testdomain.com</server>
    <port>6667</port>
    <username>testbot</username>
    <nick>testbot</nick>
    <realname>test bot</realname>
    <password>apassword</password>
</ircbotconfig>
";

            Assert.Throws<ValidationException>( () =>
                XmlLoader.ParseIrcConfigFromString( xmlString )
            );
        }

        /// <summary>
        /// Tests an XML file that is invalid since it has a channel that is empty.
        /// </summary>
        [Test]
        public void TestInvalidXmlWithEmptyChannel()
        {
            const string xmlString =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<ircbotconfig xmlns=""https://files.shendrick.net/projects/chaskis/schemas/chaskisircconfig/2017/chaskisircconfig.xsd"">
    <server>irc.testdomain.com</server>
    <channels>
        <channel>#testchannel</channel>
        <channel></channel>
    </channels>
    <port>6667</port>
    <username>testbot</username>
    <nick>testbot</nick>
    <realname>test bot</realname>
    <password>apassword</password>
    <quitmessage>I am being shut down!</quitmessage>
    <admins>
        <admin>person1</admin>
        <admin>person2</admin>
    </admins>
</ircbotconfig>
";

            Assert.Throws<ValidationException>( () =>
                XmlLoader.ParseIrcConfigFromString( xmlString )
            );
        }

        /// <summary>
        /// Tests an XML file that is invalid since it has an empty admin
        /// </summary>
        [Test]
        public void TestInvalidXmlEmptyAdmin()
        {
            const string xmlString =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<ircbotconfig xmlns=""https://files.shendrick.net/projects/chaskis/schemas/chaskisircconfig/2017/chaskisircconfig.xsd"">
	<server>irc.testdomain.com</server>
    <channels>
        <channel>#testchannel</channel>
    </channels>
	<port>6667</port>
	<username>testbot</username>
	<nick>testbot</nick>
	<realname>test bot</realname>
	<password>apassword</password>
    <quitmessage>I am being shut down!</quitmessage>
    <admins>
        <admin></admin>
        <admin>person2</admin>
    </admins>
</ircbotconfig>
";

            Assert.Throws<ValidationException>( () =>
                XmlLoader.ParseIrcConfigFromString( xmlString )
            );
        }

        /// <summary>
        /// Ensures an exception is thrown when the root node is not correct.
        /// </summary>
        [Test]
        public void TestIrcConfigBadRootName()
        {
            const string xmlString =
@"<?xml version=""1.0"" encoding=""utf-8""?>
<derp xmlns=""https://files.shendrick.net/projects/chaskis/schemas/chaskisircconfig/2017/chaskisircconfig.xsd"">
	<server>irc.testdomain.com</server>
	<channels>
        <channel>#testchannel</channel>
    </channels>
	<port>6667</port>
	<username>testbot</username>
	<nick>testbot</nick>
	<realname>test bot</realname>
	<password>apassword</password>
</derp>
";

            Assert.Throws<XmlException>( () =>
                XmlLoader.ParseIrcConfigFromString( xmlString )
            );
        }

        // ---- Plugin Xml ----

        /// <summary>
        /// Ensures the plugin loader works for a valid plugin config.
        /// </summary>
        [Test]
        public void TestValidPluginConfig()
        {
            const string xmlString =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<pluginconfig xmlns=""https://files.shendrick.net/projects/chaskis/schemas/chaskisircconfig/2017/pluginconfigschema.xsd"">
    <assembly path=""/home/me/.config/GenericIrcBot/plugins/TestPlugin/TestPlugin.dll"" />
    <assembly path=""/home/me/.config/GenericIrcBot/plugins/TestPlugin/TestPlugin2.dll"" >
        <ignorechannel>#blacklist</ignorechannel>
        <ignorechannel>#evil</ignorechannel>
    </assembly>
</pluginconfig>
";

            IList<AssemblyConfig> configs = XmlLoader.ParsePluginConfigFromString( xmlString );
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
            const string xmlString =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<pluginconfig xmlns=""https://files.shendrick.net/projects/chaskis/schemas/chaskisircconfig/2017/pluginconfigschema.xsd"">
</pluginconfig>
";

            IList<AssemblyConfig> configs = XmlLoader.ParsePluginConfigFromString( xmlString );
            Assert.AreEqual( 0, configs.Count );
        }

        /// <summary>
        /// Ensures an exception is thrown when the root node is not correct.
        /// </summary>
        [Test]
        public void TestInvalidPluginConfigBadRootName()
        {
            const string xmlString =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<derp xmlns=""https://files.shendrick.net/projects/chaskis/schemas/chaskisircconfig/2017/pluginconfigschema.xsd"">
	<assembly path=""/home/me/.config/GenericIrcBot/plugins/TestPlugin/TestPlugin.dll"" />
	<assembly path=""/home/me/.config/GenericIrcBot/plugins/TestPlugin/TestPlugin2.dll"" />
</derp>
";

            Assert.Throws<XmlException>( () =>
                XmlLoader.ParsePluginConfigFromString( xmlString )
            );
        }

        /// <summary>
        /// Ensures an empty path in the XML resutls in an exception.
        /// </summary>
        [Test]
        public void TestInvalidPluginConfigNoPath()
        {
            const string xmlString =
@"<?xml version=""1.0"" encoding=""UTF-8""?>
<pluginconfig xmlns=""https://files.shendrick.net/projects/chaskis/schemas/chaskisircconfig/2017/pluginconfigschema.xsd"">
	<assembly />
	<assembly path=""/home/me/.config/GenericIrcBot/plugins/TestPlugin/TestPlugin2.dll"" />
</pluginconfig>
";

            Assert.Throws<ArgumentNullException>( () =>
                XmlLoader.ParsePluginConfigFromString( xmlString )
            );
        }
    }
}