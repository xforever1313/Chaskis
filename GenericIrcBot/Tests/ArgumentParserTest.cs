
//          Copyright Seth Hendrick 2016.
// Distributed under the Boost Software License, Version 1.0.
//    (See accompanying file ../../LICENSE_1_0.txt or copy at
//          http://www.boost.org/LICENSE_1_0.txt)

using System;
using Chaskis;
using NUnit.Framework;
using System.IO;

namespace Tests
{
    [TestFixture]
    public class ArgumentParserTest
    {
        // -------- Fields --------

        /// <summary>
        /// The test root directory.
        /// </summary>
        private static readonly string rootDir = Environment.GetFolderPath( Environment.SpecialFolder.ApplicationData );

        // -------- Tests --------

        /// <summary>
        /// Ensures the default are sane.
        /// </summary>
        [Test]
        public void DefaultTest()
        {
            string[] args = { };

            ArgumentParser uut = new ArgumentParser( args, rootDir );
            Assert.AreEqual( uut.DefaultIrcConfigLocation, uut.IrcConfigLocation );
            Assert.AreEqual( uut.DefaultIrcPluginConfigLocation, uut.IrcPluginConfigLocation );
            Assert.IsFalse( uut.FailOnPluginFailure );
            Assert.IsFalse( uut.PrintHelp );
            Assert.IsFalse( uut.PrintVersion );
            Assert.IsTrue( uut.IsValid );
        }

        /// <summary>
        /// Ensures help is set to true.
        /// </summary>
        [Test]
        public void HelpTest()
        {
            foreach( string s in new string[]{ "--help", "-h", "/?" } )
            {
                string[] args = { s };

                ArgumentParser uut = new ArgumentParser( args, rootDir );
                Assert.AreEqual( uut.DefaultIrcConfigLocation, uut.IrcConfigLocation );
                Assert.AreEqual( uut.DefaultIrcPluginConfigLocation, uut.IrcPluginConfigLocation );
                Assert.IsFalse( uut.FailOnPluginFailure );
                Assert.IsTrue( uut.PrintHelp );
                Assert.IsFalse( uut.PrintVersion );
                Assert.IsTrue( uut.IsValid );
            }
        }

        /// <summary>
        /// Ensures version is set to true.
        /// </summary>
        [Test]
        public void VersionTest()
        {
            string[] args = { "--version" };

            ArgumentParser uut = new ArgumentParser( args, rootDir );
            Assert.AreEqual( uut.DefaultIrcConfigLocation, uut.IrcConfigLocation );
            Assert.AreEqual( uut.DefaultIrcPluginConfigLocation, uut.IrcPluginConfigLocation );
            Assert.IsFalse( uut.FailOnPluginFailure );
            Assert.IsFalse( uut.PrintHelp );
            Assert.IsTrue( uut.PrintVersion );
            Assert.IsTrue( uut.IsValid );
        }

        /// <summary>
        /// Ensures the plugin failure argument is working.
        /// </summary>
        [Test]
        public void FailOnPluginFailureTest()
        {
            {
                string[] args = { "--failOnBadPlugin=yes" };

                ArgumentParser uut = new ArgumentParser( args, rootDir );
                Assert.AreEqual( uut.DefaultIrcConfigLocation, uut.IrcConfigLocation );
                Assert.AreEqual( uut.DefaultIrcPluginConfigLocation, uut.IrcPluginConfigLocation );
                Assert.IsTrue( uut.FailOnPluginFailure );
                Assert.IsFalse( uut.PrintHelp );
                Assert.IsFalse( uut.PrintVersion );
                Assert.IsTrue( uut.IsValid );
            }
            {
                string[] args = { "--failOnBadPlugin=no" };

                ArgumentParser uut = new ArgumentParser( args, rootDir );
                Assert.AreEqual( uut.DefaultIrcConfigLocation, uut.IrcConfigLocation );
                Assert.AreEqual( uut.DefaultIrcPluginConfigLocation, uut.IrcPluginConfigLocation );
                Assert.IsFalse( uut.FailOnPluginFailure );
                Assert.IsFalse( uut.PrintHelp );
                Assert.IsFalse( uut.PrintVersion );
                Assert.IsTrue( uut.IsValid );
            }
            {
                string[] args = { "--failOnBadPlugin=derp" };

                ArgumentParser uut = new ArgumentParser( args, rootDir );
                Assert.AreEqual( uut.DefaultIrcConfigLocation, uut.IrcConfigLocation );
                Assert.AreEqual( uut.DefaultIrcPluginConfigLocation, uut.IrcPluginConfigLocation );
                Assert.IsFalse( uut.FailOnPluginFailure );
                Assert.IsFalse( uut.PrintHelp );
                Assert.IsFalse( uut.PrintVersion );
                Assert.IsFalse( uut.IsValid );
            }
        }

        /// <summary>
        /// Irc Config Argument test.
        /// </summary>
        [Test]
        public void IrcConfigArg()
        {
            {
                const string xmlFile = "config.xml";
                string[] args = { "--configPath=" + xmlFile };

                ArgumentParser uut = new ArgumentParser( args, rootDir );
                Assert.AreEqual( Path.Combine( rootDir, xmlFile ), uut.IrcConfigLocation );
                Assert.AreEqual( uut.DefaultIrcPluginConfigLocation, uut.IrcPluginConfigLocation );
                Assert.IsFalse( uut.FailOnPluginFailure );
                Assert.IsFalse( uut.PrintHelp );
                Assert.IsFalse( uut.PrintVersion );
                Assert.IsTrue( uut.IsValid );
            }
            {
                string[] args = { "--configPath=" };

                ArgumentParser uut = new ArgumentParser( args, rootDir );
                Assert.AreEqual( uut.DefaultIrcConfigLocation, uut.IrcConfigLocation );
                Assert.AreEqual( uut.DefaultIrcPluginConfigLocation, uut.IrcPluginConfigLocation );
                Assert.IsFalse( uut.FailOnPluginFailure );
                Assert.IsFalse( uut.PrintHelp );
                Assert.IsFalse( uut.PrintVersion );
                Assert.IsFalse( uut.IsValid );
            }
        }

        /// <summary>
        /// Irc Plugin Config Argument test.
        /// </summary>
        [Test]
        public void IrcPluginConfigArg()
        {
            {
                const string xmlFile = "config.xml";
                string[] args = { "--pluginConfigPath=" + xmlFile };

                ArgumentParser uut = new ArgumentParser( args, rootDir );
                Assert.AreEqual( uut.DefaultIrcConfigLocation, uut.IrcConfigLocation );
                Assert.AreEqual( Path.Combine( rootDir, xmlFile ), uut.IrcPluginConfigLocation );
                Assert.IsFalse( uut.FailOnPluginFailure );
                Assert.IsFalse( uut.PrintHelp );
                Assert.IsFalse( uut.PrintVersion );
                Assert.IsTrue( uut.IsValid );
            }
            {
                string[] args = { "--pluginConfigPath=" };

                ArgumentParser uut = new ArgumentParser( args, rootDir );
                Assert.AreEqual( uut.DefaultIrcConfigLocation, uut.IrcConfigLocation );
                Assert.AreEqual( uut.DefaultIrcPluginConfigLocation, uut.IrcPluginConfigLocation );
                Assert.IsFalse( uut.FailOnPluginFailure );
                Assert.IsFalse( uut.PrintHelp );
                Assert.IsFalse( uut.PrintVersion );
                Assert.IsFalse( uut.IsValid );
            }
        }
    }
}

